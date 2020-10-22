using Docnet.Core;
using Docnet.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SerilogTimings;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Web;

namespace SF.Pdf.Operations
{
    public class PdfOperation : IPdfOperation
    {
        private readonly ILogger<PdfOperation> _logger;
        private readonly PdfSettings _pdfSettings;

        public PdfOperation(ILogger<PdfOperation> logger, IOptions<PdfSettings> pdfSettings)
        {
            _logger = logger;
            _pdfSettings = pdfSettings.Value;
        }
        public string ConvertToImage(ConvertRequest convertRequest)
        {
            var convertedImagesfolder = string.Empty;
            using (var fileNameSerilogEvent = Operation.Begin($"Started {convertRequest.fileName}"))
            {
                using (var library = DocLib.Instance)
                {
                    var dimensions = new PageDimensions(_pdfSettings.DimensionOne, _pdfSettings.DimensionTwo);

                    var uploadedFilesPath = Path.Combine(_pdfSettings.UploadedFilePath, convertRequest.fileName);
                    using (var docReader = library.GetDocReader(uploadedFilesPath, dimensions))
                    {
                        var fileInfo = new FileInfo(uploadedFilesPath);

                        _logger.LogInformation($"Processing {fileInfo.Name}");
                        convertedImagesfolder = System.Guid.NewGuid().ToString();
                        var folderPath = Path.Combine(_pdfSettings.ConvertedImagesPath, convertedImagesfolder);

                        _logger.LogInformation($"Creating folder {folderPath}");
                        CreateDirectoryIfNotExist(folderPath);

                        var pageCount = docReader.GetPageCount();

                        using (var fileSerilogEvent = Operation.Begin($"Processing {fileInfo.Name} with {pageCount} pages"))
                        {
                            for (int i = 0; i < docReader.GetPageCount(); i++)
                            {
                                using (var pageReader = docReader.GetPageReader(i))
                                {
                                    using (var page = Operation.Begin($"Saving page_{i + 1} for {fileInfo.Name}"))
                                    {
                                        var rawBytes = pageReader.GetImage();
                                        var width = pageReader.GetPageWidth();
                                        var height = pageReader.GetPageHeight();
                                        var characters = pageReader.GetCharacters();

                                        using var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                                        AddBytes(bmp, rawBytes);

                                        using var stream = new MemoryStream();
                                        bmp.Save(stream, ImageFormat.Png);

                                        File.WriteAllBytes(Path.Combine(folderPath, $"{i + 1}.jpeg"), stream.ToArray());
                                        page.Complete();
                                    }
                                }
                            }

                            fileSerilogEvent.Complete();
                        }
                    }
                }

                fileNameSerilogEvent.Complete();
            }

            return convertedImagesfolder;
        }

        public void DeleteFolder(DeleteFolderRequest deleteFolderRequest)
        {
            _logger.LogInformation($"Request to delete {deleteFolderRequest.folderName} received");

            if (_pdfSettings.DeleteOlderFiles)
            {
                var directoryInfo = new DirectoryInfo(_pdfSettings.ConvertedImagesPath);

                foreach (var directory in directoryInfo.GetDirectories())
                {
                    _logger.LogInformation($"Checking last folder access for {directory.Name}");
                    if (directory.LastAccessTimeUtc >= DateTime.UtcNow.AddHours(-_pdfSettings.DeleteFilesOlderThanXHours))
                    {
                        DeleteFiles(directory);
                    }
                }
            }

            DeleteFiles(new DirectoryInfo(Path.Combine(_pdfSettings.ConvertedImagesPath, deleteFolderRequest.folderName)));
        }

        public List<string> ProcessFiles()
        {
            var processedFileList = new List<string>();

            var pdfFilesInDirectory = Directory.GetFiles(HttpUtility.UrlDecode(_pdfSettings.UploadedFilePath), "*.pdf");

            foreach (var pdfFile in pdfFilesInDirectory)
            {
                var result = ConvertToImage(new ConvertRequest
                {
                    fileName = pdfFile
                });

                if (!string.IsNullOrWhiteSpace(result))
                {
                    processedFileList.Add(result);
                }
            }

            return processedFileList;
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

        private void DeleteFiles(DirectoryInfo directoryInfo)
        {
            if (directoryInfo.Exists)
            {
                _logger.LogInformation($"{directoryInfo.FullName} exists");
                foreach (var file in directoryInfo.GetFiles())
                {
                    _logger.LogInformation($"deleting {file}");
                    file.Delete();
                }

                _logger.LogInformation($"deleting {directoryInfo.FullName}");
                Directory.Delete(directoryInfo.FullName);
            }
        }
    }
}