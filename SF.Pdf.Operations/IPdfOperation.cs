using System;

namespace SF.Pdf.Operations
{
    public interface IPdfOperation
    {
        string ConvertToImage(ConvertRequest convertRequest);
        void DeleteFolder(DeleteFolderRequest deleteFolderRequest);
    }
}