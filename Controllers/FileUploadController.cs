using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using DotNetAssignment2.Data;
using DotNetAssignment2.Models;
using System;

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
        if (file.Length > 50 * 1024 * 1024) // 50 MB size limit
        {
            return BadRequest("File size exceeds the allowed limit.");
        }
        string timestamp = $"{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}-";
        var filePath = Path.Combine("UploadedFiles", timestamp + file.FileName);

        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var uploadedFile = new UploadedFile
        {
            FileName = file.FileName,
            FilePath = filePath,
            UploadTime = DateTime.Now // 確保插入上傳時間
        };

        _dbContext.UploadedFiles.Add(uploadedFile);
        await _dbContext.SaveChangesAsync();

        return Ok(new { 
            Message = "File uploaded!", 
            FileName = file.FileName,
            FilePath = uploadedFile.FilePath,
            Id = uploadedFile.Id 
        });
    }

    // 查詢所有已上傳檔案的 API
    [HttpGet]
    [Route("files")]
    public IActionResult GetUploadedFiles()
    {
        var files = _dbContext.UploadedFiles.ToList<UploadedFile>();

        if (!files.Any())
        {
            return NotFound("No files found.");
        }

        return Ok(files);
    }


    [HttpGet]
    [Route("csv/columns/{id}")]
    public IActionResult GetCsvColumnsById(int id)
    {
        try
        {
            // 從資料庫中查詢檔案
            var uploadedFile = _dbContext.UploadedFiles.FirstOrDefault(f => f.Id == id);

            if (uploadedFile == null)
            {
                return NotFound($"File with Id '{id}' not found.");
            }

            // 檢查檔案是否存在於檔案系統
            if (!System.IO.File.Exists(uploadedFile.FilePath))
            {
                return NotFound($"File '{uploadedFile.FilePath}' not found on disk.");
            }

            // 讀取 CSV 文件的第一行
            using (var reader = new StreamReader(uploadedFile.FilePath))
            {
                var headerLine = reader.ReadLine(); // 讀取第一行

                if (string.IsNullOrEmpty(headerLine))
                {
                    return BadRequest("The file is empty or not a valid CSV.");
                }

                // 將第一行依據逗號分隔，返回欄位名稱列表
                var columns = headerLine.Split(',').ToList();
                return Ok(new { columns = columns }); // 返回 JSON 格式的欄位名稱
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return StatusCode(500, "An error occurred while reading the CSV file.");
        }
    }

    [HttpGet]
    [Route("file/path/{id}")]
    public IActionResult GetFilePathById(int id)
    {
        try
        {
            var uploadedFile = _dbContext.UploadedFiles.FirstOrDefault(f => f.Id == id);

            if (uploadedFile == null)
            {
                return NotFound($"File with Id '{id}' not found.");
            }

            if (!System.IO.File.Exists(uploadedFile.FilePath))
            {
                return NotFound($"File '{uploadedFile.FilePath}' not found on disk.");
            }

            return Ok(new { FilePath = uploadedFile.FilePath, FileName = uploadedFile.FileName });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return StatusCode(500, "An error occurred while retrieving the file path.");
        }
    }


}
