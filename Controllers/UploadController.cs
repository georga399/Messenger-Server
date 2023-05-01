using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.StaticFiles;
using Messenger.Helpers;
namespace Messenger.Controllers;
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UploadController: ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private readonly IFileValidator _fileValidator;
    public UploadController(IWebHostEnvironment environment, IFileValidator fileValidator)
    {
        _environment = environment;
        _fileValidator = fileValidator;
    }
    [HttpPost("attach")]
    public async Task<IActionResult> UploadAttachment(IFormFile file)
    {
        if(!_fileValidator.IsValidMedia(file))
            return BadRequest("File validation failed!");
        var fileName = DateTime.Now.ToString("yyyymmddMMss") + "_" + Path.GetFileName(file.FileName);
        var folderPath = Path.Combine(_environment.ContentRootPath, "uploads/attachments");
        var filePath = Path.Combine(folderPath, fileName);
        if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }
        return Accepted(fileName);
    }   
    [HttpGet("attach/{fileName}")]
    public async Task<IActionResult> GetAttachment(string fileName)
    {
        var folderPath = Path.Combine(_environment.ContentRootPath, "uploads/attachments");
        var filePath = Path.Combine(folderPath, fileName);
        if(filePath == null)
        {
            return BadRequest("File not found");
        }
        var provider = new FileExtensionContentTypeProvider();
        if(!provider.TryGetContentType(filePath, out var contenttype))
        {
            contenttype ="application/octet-stream";
        }
        var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
        return File(bytes, contenttype, Path.GetFileName(filePath));
    }
    [HttpDelete("attach/{fileName}")]
    public IActionResult DeleteAttachment(string fileName)
    {
        var folderPath = Path.Combine(_environment.ContentRootPath, "uploads/attachments");
        var filePath = Path.Combine(folderPath, fileName);
        FileInfo fileInf = new FileInfo(filePath);
        if(fileInf.Exists)
        {
            fileInf.Delete();
            return Accepted("Deleted");
        }
        return BadRequest("File not found");
        
    }
}