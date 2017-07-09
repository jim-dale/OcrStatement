
namespace OcrStatement
{
    using System;
    using System.Collections.Generic;

    public class Statement
    {
        public string AccountNumber { get; set; }
        public DateTime StatementDate { get; set; }
        public List<OcrResult> OcrResults { get; set; }

        public Statement()
        {
            OcrResults = new List<OcrResult>();
        }
    }
}
