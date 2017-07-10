
namespace OcrStatement
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    public class ArgsProcessor
    {
        private enum ParseState
        {
            ExpectOption,
            ExpectSource,
            ExpectTargetFileName,
            ExpectIntermediateFolder,
            ExpectSearchPatterns,
            ExpectAllowedCharacters
        }

        public static void ShowHelp()
        {
            Console.WriteLine("OCR bank statements.");
            Console.WriteLine();
            Console.WriteLine("  -?        Display this help information.");
            Console.WriteLine("  -s path            Source.");
            Console.WriteLine("  -t [path]          Target csv file to write transactions to.");
            Console.WriteLine("  -i [path]          Intermediate folder for storing scanned text and processed images.");
            Console.WriteLine("  -p [patterns]      Comma separated list of file search patterns (default *.jpg,*.png).");
            Console.WriteLine("  -c [characters]    Set of characters allowed in OCR engine.");
            Console.WriteLine("  -f                 Force image processing even if processed image is available.");
            Console.WriteLine("  -o                 Force OCR even if the OCR'rd text is cached.");
            Console.WriteLine();
        }

        public static void ShowConfig(AppConfig cfg)
        {
            string patterns = String.Join(", ", cfg.SearchPatterns);

            Console.WriteLine($"Source=\"{cfg.Source}\"");
            Console.WriteLine($"TargetFileName=\"{cfg.TargetFileName}\"");
            Console.WriteLine($"IntermediateFolder=\"{cfg.IntermediateFolder}\"");
            Console.WriteLine($"TessdataPath=\"{cfg.TessdataPath}\"");
            Console.WriteLine($"Patterns={patterns}");
            Console.WriteLine($"Characters=\"{cfg.AllowedCharacters}\"");
            Console.WriteLine($"Force image processing={cfg.ForceImageProcessing}");
            Console.WriteLine($"Force OCR={cfg.ForceOcr}");
        }

        public static AppConfig Parse(string[] args)
        {
            var result = GetDefaultConfig();

            ParseState state = ParseState.ExpectOption;
            foreach (var arg in args)
            {
                switch (state)
                {
                    case ParseState.ExpectOption:
                        state = ParseOption(arg, result);
                        break;
                    case ParseState.ExpectSource:
                        result.Source = GetPathWithEnvVars(arg);
                        state = ParseState.ExpectOption;
                        break;
                    case ParseState.ExpectTargetFileName:
                        result.TargetFileName = GetPathWithEnvVars(arg);
                        state = ParseState.ExpectOption;
                        break;
                    case ParseState.ExpectIntermediateFolder:
                        result.IntermediateFolder = GetPathWithEnvVars(arg);
                        state = ParseState.ExpectOption;
                        break;
                    case ParseState.ExpectSearchPatterns:
                        result.SearchPatterns = GetSearchPatterns(arg);
                        state = ParseState.ExpectOption;
                        break;
                    case ParseState.ExpectAllowedCharacters:
                        result.AllowedCharacters = arg;
                        state = ParseState.ExpectOption;
                        break;
                    default:
                        break;
                }
            }
            return result;
        }

        private static AppConfig GetDefaultConfig()
        {
            var currentDirectory = Directory.GetCurrentDirectory();

            var result = new AppConfig()
            {
                Source = currentDirectory,
                TargetFileName = Path.Combine(currentDirectory, "statements.csv"),
                IntermediateFolder = Path.Combine(currentDirectory, "TEMP"),
                TessdataPath = GetTessdataPath(),
                SearchPatterns = new string[] { "*.png", "*.jpg" },
                AllowedCharacters = @"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789&./()[]'-,*@",
            };
            return result;
        }

        private static ParseState ParseOption(string arg, AppConfig config)
        {
            ParseState result = ParseState.ExpectOption;

            if (arg.Length > 1 && (arg[0] == '-' || arg[0] == '/'))
            {
                switch (Char.ToLowerInvariant(arg[1]))
                {
                    case '?':
                        config.ShowHelp = true;
                        break;
                    case 'f':
                        config.ForceImageProcessing = true;
                        break;
                    case 'o':
                        config.ForceOcr = true;
                        break;
                    case 's':
                        result = ParseState.ExpectSource;
                        break;
                    case 't':
                        result = ParseState.ExpectTargetFileName;
                        break;
                    case 'i':
                        result = ParseState.ExpectIntermediateFolder;
                        break;
                    case 'p':
                        result = ParseState.ExpectSearchPatterns;
                        break;
                    case 'c':
                        result = ParseState.ExpectAllowedCharacters;
                        break;
                    default:
                        break;
                }
            }
            return result;
        }

        private static string GetTessdataPath()
        {
            const string EnvName = "TESSDATA_PREFIX";

            string result = Environment.GetEnvironmentVariable(EnvName);
            if (string.IsNullOrEmpty(result))
            {
                string exe = Assembly.GetExecutingAssembly().Location;
                string folder = Path.GetDirectoryName(exe);
                result = Path.Combine(folder, "tessdata");
            }
            return result;
        }

        private static string GetPathWithEnvVars(string s)
        {
            string result = Environment.ExpandEnvironmentVariables(s);
            return Path.GetFullPath(result);
        }

        private static string[] GetSearchPatterns(string arg)
        {
            string[] patterns = arg.Split(',');
            string[] result = (from s in patterns
                               select s.Trim()).ToArray();
            return result;
        }
    }
}
