using Messenger.Repositories;
using Microsoft.Extensions.Logging;
using AutoMapper;
namespace Messenger.Tests.Repositories;
public class MessageRepositoryTests: IDisposable
{
    protected readonly ApplicationDbContext dbContext;
    protected readonly MessageRepository messageRepository;
    protected readonly ILogger<MessageRepository> logger;
    protected readonly IMapper mapper;
    public MessageRepositoryTests()
    {
        dbContext = ApplicationDbFactory.GetDbContext();
        logger = A.Fake<ILogger<MessageRepository>>();
        mapper = A.Fake<IMapper>();
        messageRepository = new MessageRepository(dbContext, mapper, logger);
    }
    public async void Dispose()
    {
        await ApplicationDbFactory.Destroy(dbContext);
    }
    
}