using Messenger.Repositories;
using Messenger.Models;
namespace Messenger.Tests.Repositories;
public class ConnectionRepositoryTests: IDisposable
{
    private readonly ApplicationDbContext dbContext;
    private readonly ConnectionRepository connectionRepository;
    public ConnectionRepositoryTests()
    {
        dbContext = ApplicationDbFactory.GetDbContext();
        connectionRepository = new ConnectionRepository(dbContext);
    }
    public async void Dispose()
    {
        await ApplicationDbFactory.Destroy(dbContext);
    }

    [Fact]
    public async Task ConnectionRepository_GetAllConnectionsOfChat_Returns_ListOfConnections()
    {
        //Arrange
        int chatId = 1;
        //Act
        var result = await connectionRepository.GetAllConnectionsOfChat(chatId);
        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<List<Connection>>();
    }
    [Fact]
    public async Task ConnectionRepository_GetAllConnectionsOfUser_Returns_ListOfConnections()
    {
        //Arrange
        string userId = "#1";
        //Act
        var result = await connectionRepository.GetConnectionsOfUser(userId);
        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<List<Connection>>();
    }
}