using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

using DotNetAssignment2.Data;
using DotNetAssignment2.Models;

[Route("api/[controller]")]
[ApiController]
public class FileUploadController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public FileUploadController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost]
    [Route("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        Console.WriteLine("UPLOAD!!!");
        if (file == null || file.Length == 0)
        {
            return BadRequest("This file does not exist.");
        }

        var filePath = Path.Combine("UploadedFiles", file.FileName);

        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var uploadedFile = new UploadedFile
        {
            FileName = file.FileName,
            FilePath = filePath
        };

        _dbContext.UploadedFiles.Add(uploadedFile);
        await _dbContext.SaveChangesAsync();

        return Ok(new {Message = "File uploaded!", FileName = file.FileName});
    }
}