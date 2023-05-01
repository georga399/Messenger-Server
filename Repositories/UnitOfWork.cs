using Messenger.Data;
using AutoMapper;
namespace Messenger.Repositories;
public class UnitOfWork: IUnitOfWork
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    public UnitOfWork(ApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }
    private IChatRepository? _chatRepository;
    private IUserRepository? _userRepository;
    private IMessageRepository? _messageRepository;
    private IConnectionRepository? _connectionRepository;

    public IUserRepository UserRepository
    {
        get
        {
            if(_userRepository == null)
            {
                _userRepository = new UserRepository(_dbContext);
            }
            return _userRepository;
        }
    }
    public IChatRepository ChatRepository
    {
        get
        {
            if(_chatRepository == null)
            {
                _chatRepository = new ChatRepository(_dbContext, _mapper);
            }
            return _chatRepository;
        }
    }
    public IMessageRepository MessageRepository
    {
        get
        {
            if(_messageRepository == null)
            {
                _messageRepository = new MessageRepository(_dbContext, _mapper);
            }
            return _messageRepository;
        }
    }
    public IConnectionRepository ConnectionRepository
    {
        get
        {
            if(_connectionRepository == null)
            {
                _connectionRepository = new ConnectionRepository(_dbContext);
            }
            return _connectionRepository;
        }
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