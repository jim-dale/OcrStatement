
namespace OcrStatement
{
    using System.IO;

    public class OcrTextStore
    {
        private string _store;

        public OcrTextStore(string store)
        {
            _store = store;
        }

        public bool IsCached()
        {
            return File.Exists(_store);
        }

        public string GetText()
        {
            return File.ReadAllText(_store);
        }

        public void SaveText(string text)
        {
            File.WriteAllText(_store, text);
        }
    }
}
