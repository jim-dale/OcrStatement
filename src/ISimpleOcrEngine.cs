
namespace OcrStatement
{
    using System;
    using Tesseract;
    using System.Drawing;

    public interface ISimpleOcrEngine : IDisposable
    {
        string GetText(Bitmap bitmap, Rect rect);
    }
}
