
namespace OcrStatement
{
    using System.Collections.Generic;
    using System.IO;

    public static class CsvHelper
    {
        public static void Save(IEnumerable<Statement> statements, string path)
        {
            using (var sw = new StreamWriter(path))
            {
                sw.WriteLine("Type,Posted,Transaction,Name,Note,Amount");

                foreach (var statement in statements)
                {
                    foreach (var ocrResult in statement.OcrResults)
                    {
                        if (ocrResult.Tx == null)
                        {
                            sw.WriteLine(ocrResult.OcrTx.ToCsvString());
                        }
                        else
                        {
                            sw.WriteLine(ocrResult.Tx.ToCsvString());
                        }
                    }
                }
            }
        }
    }
}
