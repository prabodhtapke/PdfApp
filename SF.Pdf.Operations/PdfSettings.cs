
namespace SF.Pdf.Operations
{
    public class PdfSettings
    {
        public int DimensionOne { get; set; }
        public int DimensionTwo { get; set; }
        public string UploadedFilePath { get; set; }
        public string ConvertedImagesPath { get; set; }
        public bool DeleteOlderFiles { get; set; }
        public double DeleteFilesOlderThanXHours { get; set; }
    }
}