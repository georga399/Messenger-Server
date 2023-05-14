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
        var users = new List<User>(); // Adding users
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
        var chats = new List<Chat>(); //Adding chats
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
        
        dbContext.Users.Add(new User() //Adding user without chats
        {
                Id = "#" + (100+1).ToString(),
                UserName = "User" + (100+1).ToString(),
                Email = "User" + (100+1).ToString() + "@example.com"
        });
        
        var messages = new List<Message>(); // Adding messages
        for(int i = 0; i<10; i++)
        {
            foreach(var ch in chats)
            {
                foreach(var usr in users)
                {
                    var msg = new Message()
                    {
                        FromUser = usr,
                        Chat = ch,
                        Content = "Message in chatId=" + ch.Id.ToString() + 
                         " from userId=" + usr.Id.ToString() + " Number=" + (i+1).ToString(),
                    };
                    messages.Add(msg);
                }
            }
        }
        dbContext.Messages.AddRange(messages);
        
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