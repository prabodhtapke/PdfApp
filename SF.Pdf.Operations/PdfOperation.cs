using Docnet.Core;
using Docnet.Core.Models;
using Microsoft.Extensions.Logging;
using SerilogTimings;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace SF.Pdf.Operations
{
    public class PdfOperation : IPdfOperation
    {
        private readonly ILogger<PdfOperation> _logger;
        public PdfOperation(ILogger<PdfOperation> logger)
        {
            _logger = logger;
        }
        public string ConvertToImage(ConvertRequest convertRequest)
        {
            var folder = string.Empty;
            using (var filePath = Operation.Begin($"Started {convertRequest.FilePath}"))
            {
                using (var library = DocLib.Instance)
                {
                    var dimensions = new PageDimensions(720, 1280);

                    using (var docReader = library.GetDocReader(convertRequest.FilePath, dimensions))
                    {
                        var fileInfo = new FileInfo(convertRequest.FilePath);

                        _logger.LogInformation($"Processing {fileInfo.Name}");
                        folder = Path.Combine(fileInfo.DirectoryName, convertRequest.FileId.ToString());

                        _logger.LogInformation($"Creating folder {folder}");
                        CreateDirectoryIfNotExist(folder);

                        var pageCount = docReader.GetPageCount();

                        using (var file = Operation.Begin($"Processing {fileInfo.Name} with {pageCount} pages"))
                        {
                            for (int i = 0; i < docReader.GetPageCount(); i++)
                            {
                                using (var pageReader = docReader.GetPageReader(i))
                                {
                                    using (var page = Operation.Begin($"Saving page {1} for {fileInfo.Name}"))
                                    {
                                        var rawBytes = pageReader.GetImage();

                                        var width = pageReader.GetPageWidth();
                                        var height = pageReader.GetPageHeight();

                                        var characters = pageReader.GetCharacters();

                                        using var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);

                                        AddBytes(bmp, rawBytes);

                                        using var stream = new MemoryStream();

                                        bmp.Save(stream, ImageFormat.Png);

                                        _logger.LogInformation($"Saving Page_{i + 1}.jpeg");
                                        File.WriteAllBytes(Path.Combine(folder, $"Page_{i + 1}.jpeg"), stream.ToArray());

                                        page.Complete();
                                    }
                                }
                            }

                            file.Complete();
                        }
                    }
                }

                filePath.Complete();
            }

            return folder;
        }

        public void DeleteFolder(DeleteFolderRequest deleteFolderRequest)
        {
            var directoryInfo = new DirectoryInfo(deleteFolderRequest.folderName);

            if (directoryInfo.Exists)
            {

                foreach (var file in directoryInfo.GetFiles())
                {
                    file.Delete();
                }

                Directory.Delete(deleteFolderRequest.folderName);
            }
        }

        private void CreateDirectoryIfNotExist(string folderName)
        {
            var directoryInfo = new DirectoryInfo(folderName);
            if (!directoryInfo.Exists)
            {
                Directory.CreateDirectory(folderName);
            }
        }

        private void AddBytes(Bitmap bmp, byte[] rawBytes)
        {
            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);

            var bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);
            var pNative = bmpData.Scan0;

            Marshal.Copy(rawBytes, 0, pNative, rawBytes.Length);
            bmp.UnlockBits(bmpData);
        }
    }
}