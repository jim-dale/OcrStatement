
namespace OcrStatement
{
    using System.IO;

    public class OcrStoreService
    {
        private string _folder;

        public OcrStoreService(string path)
        {
            _folder = path;
            Directory.CreateDirectory(_folder);
        }

        public OcrTextStore GetTextStoreFor(string path)
        {
            string _store = GetIntermediateTextFilePath(path);
            return new OcrTextStore(_store);
        }

        private string GetIntermediateTextFilePath(string path)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);
            string result = Path.Combine(_folder, fileName + ".txt");
            return Path.GetFullPath(result);
        }
    }
}
