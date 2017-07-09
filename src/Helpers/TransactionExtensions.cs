
namespace OcrStatement
{
    public static class TransactionExtensions
    {
        public static string ToCsvString(this Transaction item)
        {
            string result = $"{item.TxType},{item.PostedDate:d},{item.TransactionDate:d},\"{item.Name}\",\"{item.Note}\",{item.Amount}";
            return result;
        }
    }
}
