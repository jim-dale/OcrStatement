
namespace OcrStatement
{
    public interface IImagePreprocessor
    {
        string GetIntermediateFileName(string path);
        void CreateNewImageFrom(string source, string target);
        void Cleanup(string path);
    }
}
