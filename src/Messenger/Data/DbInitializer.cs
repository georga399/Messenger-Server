using Messenger.Models;
using Messenger.Repositories;
using Messenger.ViewModels;
using Microsoft.AspNetCore.Identity;
using AutoMapper;
using LoremNET;
namespace Messenger.Data;

public static class DbInitializer
{
    public static async Task PopulateDb(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("First log");
        var user1Email = Lorem.Email();
        // Creating users
        if(db.Messages.Count() != 0) return;
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
        var user1 = new User{UserName = user1Email.Split('@')[0], Email = user1Email};
        await userManager.CreateAsync(user1, "Qwerty_1111");
        var user2Email = Lorem.Email();
        var user2 = new User{UserName = user2Email.Split('@')[0], Email = user2Email};
        await userManager.CreateAsync(user2, "Qwerty_1111");
        var user3Email = Lorem.Email();
        var user3 = new User{UserName = user3Email.Split('@')[0], Email = user3Email}; 
        await userManager.CreateAsync(user3, "Qwerty_1111");
        // Creating chats
        var chatViewModel1 = new ChatViewModel()
        {
            Title = "Чат номер 1",
            IsGroup = false,
            AdminId = user1.Id
        };
        chatViewModel1.UsersId.Add(user1.Id); chatViewModel1.UsersId.Add("kgjdfgsdfg"); chatViewModel1.UsersId.Add(user3.Id);
        var chatViewModel2 = new ChatViewModel()
        {
            Title = "Чат номер 2",
            IsGroup = false,
            AdminId = user2.Id
        };
        chatViewModel2.UsersId.Add(user1.Id); chatViewModel2.UsersId.Add(user2.Id); chatViewModel2.UsersId.Add(user3.Id);
        var chatViewModel3 = new ChatViewModel()
        {
            Title = "Чат номер 3",
            IsGroup = false,
            AdminId = user3.Id
        };
        chatViewModel3.UsersId.Add(user1.Id); chatViewModel3.UsersId.Add(user2.Id); chatViewModel3.UsersId.Add(user3.Id);
        var chat1 = unitOfWork.ChatRepository.AddChat(chatViewModel1); 
        var chat2 = unitOfWork.ChatRepository.AddChat(chatViewModel2);
        var chat3 = unitOfWork.ChatRepository.AddChat(chatViewModel3);
        await unitOfWork.SaveChangesAsync();
        // Added new messages
        for(int i = 0; i<10; i++)
        {
            await unitOfWork.MessageRepository.Add(new Message()
            {
                FromUser = user1,
                Content = Lorem.Paragraph(10, i+1),
                Chat = chat1,
                Timestamp = DateTime.UtcNow

            });
            await unitOfWork.MessageRepository.Add(new Message()
            {
                FromUser = user1,
                Content = Lorem.Paragraph(10, i+1),
                Chat = chat2,
                Timestamp = DateTime.UtcNow
            });
            await unitOfWork.MessageRepository.Add(new Message()
            {
                FromUser = user1,
                Content = Lorem.Paragraph(10, i+1),
                Chat = chat3,
                Timestamp = DateTime.UtcNow

            });
        }
        await unitOfWork.SaveChangesAsync();
        //Set last read message
        logger.LogInformation($"Set last read message user = {user1.Id} chat = {chat1?.Id} message = 1 will set.");
        if(!(await unitOfWork.MessageRepository.SetLastReadMessageAsync(user1.Id, chat1!.Id, 1)))
            logger.LogInformation($"Set last read message user = {user1.Id} chat = {chat1.Id} message = 1 wasn't setted.");
        await unitOfWork.SaveChangesAsync();
    }
}

