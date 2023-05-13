
namespace Messenger.Helpers;
public interface IFileValidator
{
    bool IsValidPicture(IFormFile file);
    bool IsValidMedia(IFormFile file);
}
