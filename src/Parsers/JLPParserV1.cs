
namespace OcrStatement
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;

    public static class JLPParserV1
    {
        public static Statement GetStatement(string text, string path)
        {
            var result = new Statement()
            {
                AccountNumber = GetAccountNumber(text),
                StatementDate = GetStatementDate(text, path, DateTime.Now)
            };

            GetTransactions(result, text);

            if (result.OcrResults.Count == 0)
            {
                result = null;
            }
            return result;
        }

        private static string GetAccountNumber(string text)
        {
            const string Pattern = @"Account Number\s+(\d{4}\s+\d{4}\s+\d{4}\s+\d{4})";

            string result = string.Empty;

            var regex = new Regex(Pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            var matches = regex.Matches(text);
            foreach (Match match in matches)
            {
                if (match.Success && match.Groups.Count == 2)
                {
                    result = match.Groups[1].Value;
                    break;
                }
            }
            return result;
        }

        private static DateTime GetStatementDate(string text, string file, DateTime defaultValue)
        {
            DateTime result = defaultValue;

            DateTime? fileDate = GetStatementDateFromFileName(file);
            if (fileDate.HasValue)
            {
                result = fileDate.Value;
            }
            else
            {
                DateTime? ocrDate = GetStatementDateFromText(text);
                if (ocrDate.HasValue)
                {
                    result = ocrDate.Value;
                }
            }
            return result;
        }

        private static void GetTransactions(Statement statement, string text)
        {
            var ocrTransactions = JLPParserV1.GetOcrTransactions(statement.StatementDate, text);
            foreach (var ocrTransaction in ocrTransactions)
            {
                var transaction = CreateTransaction(ocrTransaction);

                var ocrResult = new OcrResult()
                {
                    OcrTx = ocrTransaction,
                    Tx = transaction
                };
                statement.OcrResults.Add(ocrResult);
            }
        }

        private static IEnumerable<OcrTransaction> GetOcrTransactions(DateTime statementDate, string text)
        {
            var regex = GetTransactionRegex();

            var matches = regex.Matches(text);
            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    var result = CreateOcrTransaction(statementDate, match.Groups);
                    yield return result;
                }
            }
        }

        private static OcrTransaction CreateOcrTransaction(DateTime statementDate, GroupCollection groups)
        {
            string postedDateStr = groups[1].Value.Trim() + " " + statementDate.Year;
            string transactionDateStr = groups[2].Value.Trim() + " " + statementDate.Year;

            DateTime? postedDate = JLPParserV1.GetTransactionDate(postedDateStr);
            DateTime? transactionDate = JLPParserV1.GetTransactionDate(transactionDateStr);

            if (postedDate.HasValue && transactionDate.HasValue)
            {
                if (postedDate.Value > statementDate)
                {
                    postedDate = postedDate.Value.AddYears(-1);
                }
                if (transactionDate.Value > statementDate)
                {
                    transactionDate = transactionDate.Value.AddYears(-1);
                }
            }
            postedDateStr = (postedDate.HasValue) ? postedDate.Value.ToShortDateString() : postedDateStr;
            transactionDateStr = (transactionDate.HasValue) ? transactionDate.Value.ToShortDateString() : transactionDateStr;

            var result = new OcrTransaction()
            {
                PostedDate = postedDateStr,
                TransactionDate = transactionDateStr,
                Name = groups[3].Value.Trim(),
                Amount = groups[4].Value.Trim(),
                Credit = groups[5].Value.Trim(),
                Note = groups[6].Value.Trim()
            };
            return result;
        }

        private static Transaction CreateTransaction(OcrTransaction ocrTransaction)
        {
            DateTime? postedDate = JLPParserV1.GetTransactionDate(ocrTransaction.PostedDate);
            DateTime? transactionDate = JLPParserV1.GetTransactionDate(ocrTransaction.PostedDate);
            string name = ocrTransaction.Name;
            TransactionType txType = JLPParserV1.GetTransactionType(ocrTransaction.Credit);
            decimal? amount = ocrTransaction.Amount.TryAsDecimal();

            if (txType == TransactionType.DEBIT)
            {
                amount = 0 - amount;
            }

            Transaction result = null;
            if (postedDate.HasValue && transactionDate.HasValue && String.IsNullOrWhiteSpace(name) == false && amount.HasValue)
            {
                result = new Transaction()
                {
                    TxType = txType,
                    PostedDate = postedDate.Value,
                    TransactionDate = transactionDate.Value,
                    Name = name,
                    Note = ocrTransaction.Note,
                    Amount = amount.Value
                };
            }
            return result;
        }

        private static DateTime? GetTransactionDate(string s)
        {
            DateTime? result = null;

            string str = s.ToLowerInvariant().Replace("sept", "sep");
            if (DateTime.TryParse(str, out DateTime date))
            {
                result = date;
            }
            return result;
        }

        private static TransactionType GetTransactionType(string s)
        {
            TransactionType result = (string.IsNullOrEmpty(s)) ? TransactionType.DEBIT : TransactionType.CREDIT;

            return result;
        }

        private static DateTime? GetStatementDateFromFileName(string path)
        {
            const string Pattern = @"(\d{4})\W(\d{2})\W(\d{2})";

            DateTime? result = null;

            string fileName = Path.GetFileNameWithoutExtension(path);

            var regex = new Regex(Pattern);

            var matches = regex.Matches(fileName);
            foreach (Match match in matches)
            {
                if (match.Success && match.Groups.Count == 4)
                {
                    int year = int.Parse(match.Groups[1].Value);
                    int month = int.Parse(match.Groups[2].Value);
                    int day = int.Parse(match.Groups[3].Value);

                    result = new DateTime(year, month, day);

                    break;
                }
            }
            return result;
        }

        private static DateTime? GetStatementDateFromText(string text)
        {
            const string Pattern = @"(\d{1,2}\s+\w{3,9}\s+\d{4})";

            DateTime? result = null;

            var regex = new Regex(Pattern, RegexOptions.Compiled);

            var matches = regex.Matches(text);
            foreach (Match match in matches)
            {
                if (match.Success && match.Groups.Count == 2)
                {
                    if (DateTime.TryParse(match.Groups[1].Value, out DateTime temp))
                    {
                        result = temp;
                        break;
                    }
                }
            }
            return result;
        }

        private static Regex GetTransactionRegex()
        {
            const string DateRegex = @"\d{1,2}\s+[A-Za-z]{3,4}";
            const string DateCapture = "(" + DateRegex + ")";
            const string NameCapture = "(.+)";
            const string AmountRegex = @"(?:\d{1,3},\d{3}\.\d{2})|(?:\d{1,6}\.\d{2})";
            const string AmountCapture = "(" + AmountRegex + ")";
            const string CreditCapture = @"(CR)?";
            const string CurrencyRegex = @"\w+";
            const string RateRegex = @"\d{1,3}\.\d{1,4}";
            const string EndOfLine = @"\r*\n";

            const string TxCapture = DateCapture + @"\s+" + DateCapture + @"\s+" + NameCapture + @"\s+" + AmountCapture + @"\s*" + CreditCapture + EndOfLine;
            const string CurrencyConvCapture = @"(" + AmountRegex + @"\s+" + CurrencyRegex + @"\s*@\s*" + RateRegex + @")?";

            const string RegexPattern = TxCapture + @"\s*" + CurrencyConvCapture;

            var result = new Regex(RegexPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            return result;
        }
    }
}
