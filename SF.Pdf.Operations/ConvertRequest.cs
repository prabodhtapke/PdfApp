using System;

namespace SF.Pdf.Operations
{
    public class ConvertRequest
    {
        public Guid FileId { get; set; }
        public string FilePath { get; set; }
    }
}