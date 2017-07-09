
namespace OcrStatement
{
    class AppConfig
    {
        public string Source { get; set; }
        public string TargetFileName { get; set; }
        public string[] SearchPatterns { get; set; }
        public string IntermediateFolder { get; set; }
        public string AllowedCharacters { get; set; }
        public bool ForceDeskew { get; set; }
        public bool ForceOcr { get; set; }
    }
}
