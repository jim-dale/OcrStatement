
namespace OcrStatement
{
    using System;

    public enum TransactionType
    {
        CREDIT,
        DEBIT
    }

    public class Transaction
    {
        public TransactionType TxType { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime PostedDate { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public decimal Amount { get; set; }
    }
}
