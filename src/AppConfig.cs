
namespace OcrStatement
{
    public class AppConfig
    {
        public bool ShowHelp { get; set; }
        public string Source { get; set; }
        public string TargetFileName { get; set; }
        public string[] SearchPatterns { get; set; }
        public string IntermediateFolder { get; set; }
        public string TessdataPath { get; set; }
        public string AllowedCharacters { get; set; }
        public bool ForceImageProcessing { get; set; }
        public bool ForceOcr { get; set; }
    }
}
