using Messenger.Data;
using AutoMapper;
namespace Messenger.Repositories;
public class UnitOfWork: IUnitOfWork
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    public IChatRepository ChatRepository{get; private set;}
    public IUserRepository UserRepository{get; private set;}
    public IMessageRepository MessageRepository{get; private set;}
    public IConnectionRepository ConnectionRepository{get; private set;}

    public UnitOfWork(ApplicationDbContext dbContext, IMapper mapper, 
        IChatRepository chatRepository, IUserRepository userRepository,
        IMessageRepository messageRepository, IConnectionRepository connectionRepository)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        ChatRepository = chatRepository;
        UserRepository = userRepository;
        MessageRepository = messageRepository;
        ConnectionRepository = connectionRepository;
    }
    public Task SaveChangesAsync()
    {
        return _dbContext.SaveChangesAsync();
    }

    private bool _disposed;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _dbContext.Dispose();
            }
        }
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}