
namespace OcrStatement
{
    using System;
    using System.Drawing;
    using Tesseract;

    public class TesseractSimpleOcrEngine : ISimpleOcrEngine
    {
        private TesseractEngine _engine = null;

        public void Initialise(string tessdataPath, string allowedCharacters)
        {
            _engine = new TesseractEngine(tessdataPath, "eng", EngineMode.Default);

            _engine.SetVariable("load_system_dawg", false);
            _engine.SetVariable("load_freq_dawg", false);
            _engine.SetVariable("load_punc_dawg", false);
            _engine.SetVariable("load_number_dawg", false);
            _engine.SetVariable("load_unambig_dawg", false);
            _engine.SetVariable("load_bigram_dawg", false);
            _engine.SetVariable("load_fixed_length_dawgs", false);
            _engine.SetVariable("tessedit_char_whitelist", allowedCharacters);
        }

        public string GetText(Bitmap bitmap, Rect rect)
        {
            string result = String.Empty;

            using (var page = _engine.Process(bitmap, rect, PageSegMode.SingleBlock))
            {
                result = page.GetText();
            }
            return result;
        }

        public void Dispose()
        {
            _engine?.Dispose();
            _engine = null;
        }
    }
}
