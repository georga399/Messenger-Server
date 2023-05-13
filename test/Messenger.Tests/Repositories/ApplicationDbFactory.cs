using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

using Messenger.Models;

namespace Messenger.Tests.Repositories;
public class ApplicationDbFactory
{
    // public static async Task<ApplicationDbContext> GetDbContext()
    // {
    //     var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    //         .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
    //         .Options;
    //     var dbContext = new ApplicationDbContext(options);
    //     dbContext.Database.EnsureCreated();
    //     //Seed Data        
    //     var user = new User()
    //     {
    //         Email = "a@gmail.com",
    //         UserName = "a"
    //     };
    //     var userManager = new UserManager<User>();
    //     await dbContext.SaveChangesAsync();
    //     Console.WriteLine(user.Id);
    //     return dbContext;            
    // }
}