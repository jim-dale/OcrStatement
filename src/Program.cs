﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Autofac;

namespace OcrStatement
{
    //IDictionary<string, string> bla = new Dictionary<string, string>();

    //Configuration config = new Configuration();

    //config["DeskewPercentage"] = "40";

    //float dp = config["DeskewPercentage"].As<float>(50);
    //var statementDate = config["StatementDate"].As<DateTime>();
    class Program
    {
        static void Main(string[] args)
        {
            var cfg = new AppConfig()
            {
                Source = Environment.ExpandEnvironmentVariables(@"%SCANDOCS%\Finance\John Lewis\Statements\2016"),
                TargetFileName = Environment.ExpandEnvironmentVariables(@"%SCANDOCS%\Finance\John Lewis\Statements\2016 John Lewis Partnership.csv"),
                IntermediateFolder = Environment.ExpandEnvironmentVariables(@"%SCANDOCS%\Finance\John Lewis\Statements\TEMP"),
                SearchPatterns = new string[] { "*.png", "*.jpg" },
                AllowedCharacters = @"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789&./()[]'-,*@",
                ForceOcr = false,
                ForceDeskew = false,
            };

            IContainer container = BuildContainer(cfg);

            var ctx = container.Resolve<AppContext>();
            ctx.Container = container;
            ctx.ImagePreprocessor = ctx.Container.Resolve<IImagePreprocessor>();
            ctx.OcrEngine = ctx.Container.Resolve<Lazy<ISimpleOcrEngine>>();

            OcrSource(ctx);

            CsvHelper.Save(ctx.Statements, ctx.Config.TargetFileName);
        }

        private static void OcrSource(AppContext ctx)
        {
            if (File.Exists(ctx.Config.Source))
            {
                ProcessFile(ctx, ctx.Config.Source);
            }
            else
            {
                if (Directory.Exists(ctx.Config.Source))
                {
                    foreach (var searchPattern in ctx.Config.SearchPatterns)
                    {
                        var files = Directory.EnumerateFiles(ctx.Config.Source, searchPattern, SearchOption.TopDirectoryOnly);

                        ProcessFiles(ctx, files);
                    }
                }
            }
        }

        private static void ProcessFiles(AppContext ctx, IEnumerable<string> files)
        {
            foreach (var file in files)
            {
                var statement = ProcessFile(ctx, file);
                if (statement != null)
                {
                    ctx.Statements.Add(statement);

                    Console.WriteLine($"{file},{statement.AccountNumber},{statement.StatementDate}");

                    foreach (var ocrResult in statement.OcrResults)
                    {
                        if (ocrResult.Tx == null)
                        {
                            Console.WriteLine(ocrResult.OcrTx.ToCsvString());
                        }
                        else
                        {
                            Console.WriteLine(ocrResult.Tx.ToCsvString());
                        }
                    }
                }
            }
        }

        private static Statement ProcessFile(AppContext ctx, string path)
        {
            Statement result = null;

            try
            {
                string text = GetText(ctx, path);

                result = JLPParserV1.GetStatement(text, path);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                Console.Error.WriteLine($"Failed to process \"{path}\": {ex.Message}");
                Console.WriteLine("Details: ");
                Console.WriteLine(ex.ToString());
            }
            return result;
        }

        private static string GetText(AppContext ctx, string path)
        {
            string result = string.Empty;

            var store = ctx.Container.Resolve<OcrTextStore>(TypedParameter.From(path));

            if (ctx.Config.ForceOcr == false && store.IsCached())
            {
                result = store.GetText();
            }
            else
            {
                result = ExtractTextFromFile(ctx, path);
                store.SaveText(result);
            }

            return result;
        }

        private static string ExtractTextFromFile(AppContext ctx, string file)
        {
            string result = null;

            string imageFileName = ctx.ImagePreprocessor.GetIntermediateFileName(file);

            if (File.Exists(imageFileName) == false || ctx.Config.ForceDeskew)
            {
                ctx.ImagePreprocessor.CreateNewImageFrom(file, imageFileName);
            }
            try
            {
                using (var bitmap = new Bitmap(imageFileName))
                {
                    var rect = ImageMask.CalculateMaskRectangle(bitmap);

                    result = ctx.OcrEngine.Value.GetText(bitmap, rect);
                }
            }
            finally
            {
                ctx.ImagePreprocessor.Cleanup(imageFileName);
            }
            return result;
        }

        private static IContainer BuildContainer(AppConfig cfg)
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(cfg)
                .SingleInstance();
            builder.RegisterType<AppContext>()
                .SingleInstance();
            builder.RegisterType<OcrStoreService>()
                .SingleInstance()
                .WithParameter(TypedParameter.From(cfg.IntermediateFolder));
            builder.RegisterType<ImageMagickPreprocessor>()
                .As<IImagePreprocessor>()
                .SingleInstance()
                .WithParameter(TypedParameter.From(40.0))
                .WithParameter(TypedParameter.From(ImagePreprocessorFilePolicy.RenamedFilePolicy))
                .WithParameter(TypedParameter.From(cfg.IntermediateFolder));
            builder.Register((c) =>
            {
                var config = c.Resolve<AppConfig>();
                var iEngine = new Lazy<ISimpleOcrEngine>(() =>
                {
                    var engine = new TesseractSimpleOcrEngine();
                    engine.Initialise(config.AllowedCharacters);
                    return engine;
                });
                return builder;
            })
                .SingleInstance();
            builder.Register((c, p) =>
            {
                string path = p.TypedAs<string>();
                var factory = c.Resolve<OcrStoreService>();
                return factory.GetTextStoreFor(path);
            });

            var result = builder.Build();
            return result;
        }
    }
}