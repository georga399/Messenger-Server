namespace Messenger.Repositories;
public interface IUnitOfWork: IDisposable
{
    IConnectionRepository ConnectionRepository{get;}
    IUserRepository UserRepository{get;}
    IChatRepository ChatRepository{get;}
    IMessageRepository MessageRepository{get;}
    Task SaveChangesAsync();
}