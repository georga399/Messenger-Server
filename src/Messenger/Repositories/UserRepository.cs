using Messenger.Models;
using Messenger.Data;
namespace Messenger.Repositories;
public class UserRepository: IUserRepository
{
    private readonly ApplicationDbContext _dbContext;
    public UserRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public User? GetById(string userId)
    {
        return _dbContext.Users.FirstOrDefault(u => u.Id == userId);
    }
    public User? GetByName(string userName)
    {
        return _dbContext.Users.FirstOrDefault(u => u.UserName == userName);
    }
    public User? GetByEmail(string email)
    {
        return _dbContext.Users.FirstOrDefault(u => u.Email == email);
    }
    public List<User> GetAllUsers()
    {
        return _dbContext.Users.ToList();
    }
}