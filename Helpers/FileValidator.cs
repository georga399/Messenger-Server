namespace Messenger.Helpers;
public class FileValidator: IFileValidator
{
    private readonly IConfiguration _configuration;
    private readonly int _fileSizeLimit;
    private readonly string[] _allowedExtensionsMedia;
    private readonly string[] _allowedExtensionsPictures;
    public FileValidator(IConfiguration configuration)
    {
        _configuration = configuration;            
        _fileSizeLimit = _configuration.GetValue("FileUpload:FileSizeLimitInBytes", 10 * 1024 * 1024); // 10MB
        _allowedExtensionsMedia = _configuration
        .GetValue("FileUpload:AllowedExtensionsMedia", ".jpg,.jpeg,.png,.mp3,.mp4,.avi")!
        .Split(",");
        _allowedExtensionsPictures = _configuration
        .GetValue("FileUpload:AllowedExtensionsPictures", ".jpg,.jpeg,.png,.mp3,.mp4,.avi")!
        .Split(",");
    }
    public bool IsValidPicture(IFormFile file)
    {
        if (file?.Length > 0)
            {
                if (file.Length > _fileSizeLimit)
                    return false;

                if (file.FileName.Length > 255)
                    return false;
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(extension) || !_allowedExtensionsPictures.Any(e => e.Contains(extension)))
                    return false;

                return true;
            }

        return false;
    }
    public bool IsValidMedia(IFormFile file)
    {
        if (file?.Length > 0)
            {
                if (file.Length > _fileSizeLimit)
                    return false;

                if (file.FileName.Length > 255)
                    return false;
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(extension) || !_allowedExtensionsMedia.Any(e => e.Contains(extension)))
                    return false;

                return true;
            }

        return false;
    }
}