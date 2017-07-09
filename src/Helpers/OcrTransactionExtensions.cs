
namespace OcrStatement
{
    public static class OcrTransactionExtensions
    {
        public static string ToCsvString(this OcrTransaction item)
        {
            string result = $"\"{item.Credit}\",\"{item.PostedDate}\",\"{item.TransactionDate}\",\"{item.Name}\",\"{item.Note}\",\"{item.Amount}\"";
            return result;
        }
    }
}
