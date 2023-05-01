using Messenger.Models;
namespace Messenger.Repositories;
public interface IUserRepository
{
    User? GetById(string userId);
    User? GetByName(string userName);
    User? GetByEmail(string email);
    List<User> GetAllUsers();
    //TODO: void Update(User user); 
}