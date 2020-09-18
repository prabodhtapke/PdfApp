using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SF.Pdf.Operations;

namespace SF.Pdf.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PdfController : ControllerBase
    {
        private readonly IPdfOperation _pdfOperation;
        public PdfController(IPdfOperation pdfOperation)
        {
            _pdfOperation = pdfOperation;
        }

        [HttpPost("ConvertToImage", Name = "ConvertToImage")]
        [ProducesResponseType(typeof(string), 200)]
        [AllowAnonymous]
        public IActionResult ConvertToImage([FromBody] ConvertRequest convertRequest)
        {
            return Ok(_pdfOperation.ConvertToImage(convertRequest));
        }

        [HttpPost("DeleteFolder", Name = "DeleteFolder")]
        [ProducesResponseType(typeof(string), 200)]
        [AllowAnonymous]
        public IActionResult DeleteFolder([FromBody] DeleteFolderRequest deleteFolderRequest)
        {
            _pdfOperation.DeleteFolder(deleteFolderRequest);
            return Ok();
        }
    }
}