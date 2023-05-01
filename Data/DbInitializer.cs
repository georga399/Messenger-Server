using Messenger.Models;
using Microsoft.AspNetCore.Identity;
using LoremNET;
namespace Messenger.Data;

public static class DbInitializer
{
    public static async Task PopulateDb(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user1Email = Lorem.Email();
        var user1 = new User{UserName = user1Email.Split('@')[0], Email = user1Email};
        await userManager.CreateAsync(user1, "Qwerty_1111");
        var user2Email = Lorem.Email();
        var user2 = new User{UserName = user2Email.Split('@')[0], Email = user2Email};
        await userManager.CreateAsync(user2, "Qwerty_1111");
        var user3Email = Lorem.Email();
        var user3 = new User{UserName = user3Email.Split('@')[0], Email = user3Email}; 
        await userManager.CreateAsync(user3, "Qwerty_1111");
        // var chat1 = new Chat{IsGroup = true, Title = "Group #1"};
        // db.Chats.Add(chat1);
        // // chat1.Users.Add(user1); chat1.Users.Add(user2);
        // user1.ChatUsers.Add(new ChatUser{Chat = chat1});
        // user2.ChatUsers.Add(new ChatUser{Chat = chat1});
        // chat1.Admin = user1;
        // db.SaveChanges();
        
    }
}

