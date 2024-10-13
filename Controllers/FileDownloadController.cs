using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace YourNamespace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileDownloadController : ControllerBase
    {
        [HttpGet("download-text-file")]
        public IActionResult DownloadTextFile()
        {
            // making a file
            var content = "This is an example.";
            var byteArray = Encoding.UTF8.GetBytes(content);

            return File(byteArray, "application/octet-stream", "sample.txt");
        }
    }
}
