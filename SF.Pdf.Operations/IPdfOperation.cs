using System.Collections.Generic;

namespace SF.Pdf.Operations
{
    public interface IPdfOperation
    {
        string ConvertToImage(ConvertRequest convertRequest);
        void DeleteFolder(DeleteFolderRequest deleteFolderRequest);
        List<string> ProcessFiles();
    }
}