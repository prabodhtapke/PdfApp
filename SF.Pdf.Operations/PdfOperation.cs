using Docnet.Core;
using Docnet.Core.Models;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace SF.Pdf.Operations
{
    public class PdfOperation : IPdfOperation
    {
        public string ConvertToImage(ConvertRequest convertRequest)
        {
            var folder = string.Empty;
            using (var library = DocLib.Instance)
            {
                var dimensions = new PageDimensions(720, 1280);

                using (var docReader = library.GetDocReader(convertRequest.FilePath, dimensions))
                {
                    var fileInfo = new FileInfo(convertRequest.FilePath);

                    folder = Path.Combine(fileInfo.DirectoryName, convertRequest.FileId.ToString());

                    CreateDirectoryIfNotExist(folder);

                    for (int i = 0; i < docReader.GetPageCount(); i++)
                    {
                        using (var pageReader = docReader.GetPageReader(i))
                        {
                            var rawBytes = pageReader.GetImage();

                            var width = pageReader.GetPageWidth();
                            var height = pageReader.GetPageHeight();

                            var characters = pageReader.GetCharacters();

                            using var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);

                            AddBytes(bmp, rawBytes);

                            using var stream = new MemoryStream();

                            bmp.Save(stream, ImageFormat.Png);

                            File.WriteAllBytes($"{folder}\\Page_{i + 1}.jpeg", stream.ToArray());
                        }
                    }
                }
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