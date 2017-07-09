
namespace OcrStatement
{
    using System;
    using System.Collections.Generic;
    using Autofac;

    class AppContext
    {
        public AppContext(AppConfig config)
        {
            Config = config;
        }

        public AppConfig Config { get; private set; }
        public IContainer Container { get; set; }

        #region Services
        public IImagePreprocessor ImagePreprocessor { get; set; }
        public Lazy<ISimpleOcrEngine> OcrEngine { get; set; }
        #endregion

        public List<Statement> Statements { get; } = new List<Statement>();
    }
}
