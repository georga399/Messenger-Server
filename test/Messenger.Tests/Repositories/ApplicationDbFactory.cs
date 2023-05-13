using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

using Messenger.Models;

namespace Messenger.Tests.Repositories;
public class ApplicationDbFactory
{
    public static ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var dbContext = new ApplicationDbContext(options);
        dbContext.Database.EnsureCreated();
        //Seed Data       
        var users = new List<User>();
        for(int i = 0; i<10; i++)
        {
            var user = new User()
            {
                Id = "#" + (i+1).ToString(),
                UserName = "User" + (i+1).ToString(),
                Email = "User" + (i+1).ToString() + "@example.com"
            };
            users.Add(user);
        }
        dbContext.Users.AddRange(users);
        var chats = new List<Chat>();
        for(int i = 0; i<5; i++)
        {
            var chat = new Chat()
            {
                Id = i+1,
                Title = "Chat#" + (i+1).ToString(),
                Admin = users.FirstOrDefault(u => u.Id == "#"+(i+1).ToString())
            };
            chats.Add(chat);
            foreach(var u in users)
            {
                var cu = new ChatUser()
                {
                    Chat = chat,
                    User = u
                };
                chat.ChatUsers.Add(cu);
                u.ChatUsers.Add(cu);
            }   
            dbContext.Chats.Add(chat);
        }
        
        dbContext.Users.Add(new User()
        {
                Id = "#" + (100+1).ToString(),
                UserName = "User" + (100+1).ToString(),
                Email = "User" + (100+1).ToString() + "@example.com"
        });
        dbContext.SaveChanges();
        // Console.WriteLine(user.Id);
        return dbContext;            
    }
    public static async Task Destroy(ApplicationDbContext context)
    {
        await context.Database.EnsureDeletedAsync();
        context.Dispose();
    }
}