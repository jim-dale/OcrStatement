
namespace OcrStatement
{
    using System.Collections.Generic;

    public class Configuration : IConfiguration
    {
        Dictionary<string, string> _items = new Dictionary<string, string>();

        public string this[string key]
        {
            get
            {
                string result = null;
                _items.TryGetValue(key, out result);

                return result;
            }
            set
            {
                _items.Add(key, value);
            }
        }
    }
}
