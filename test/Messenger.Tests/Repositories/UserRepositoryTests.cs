using Microsoft.EntityFrameworkCore;
using Messenger.Data;
using Messenger.Models;
using Messenger.Repositories;
using FluentAssertions;
namespace Messenger.Tests.Repositories;
public class UserRepositoryTests: IDisposable
{
    protected readonly ApplicationDbContext dbContext;
    protected readonly UserRepository userRepository;
    public UserRepositoryTests()
    {
        dbContext = ApplicationDbFactory.GetDbContext();
        userRepository = new UserRepository(dbContext);
    }

    [Fact]
    public void UserRepository_GetById_Returns_UserObj()
    {
        //Arrange

        //Act
        var result = userRepository.GetById("#3");
        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<User>();
        
    }
    [Fact]
    public void UserRepository_GetById_Returns_Null()
    {
        //Arrange

        //Act
        var result = userRepository.GetById("#");
        //Assert
        result.Should().BeNull();        
    }
    [Fact]
    public void UserRepository_GetByName_Returns_UserObj()
    {
        //Arrange

        //Act
        var result = userRepository.GetByName("User4");
        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<User>();
    }
    [Fact]
    public void UserRepository_GetByName_Returns_Null()
    {
        //Arrange

        //Act
        var result = userRepository.GetByName("----");
        //Assert
        result.Should().BeNull();
    }
    [Fact]
    public void UserRepository_GetByEmail_Returns_UserObj()
    {
        //Arrange

        //Act
        var result = userRepository.GetByEmail("User4@example.com");
        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<User>();
    }
    [Fact]
    public void UserRepository_GetByEmail_Returns_Null()
    {
        //Arrange

        //Act
        var result = userRepository.GetByEmail("--------");
        //Assert
        result.Should().BeNull();
    }
    [Fact]
    public void UserRepository_GetAll_Return_ListOFUsers()
    {
        //Arrange
        //Act
        var result = userRepository.GetAllUsers();
        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<List<User>>();
    }

    public async void Dispose()
    {
        await ApplicationDbFactory.Destroy(dbContext);
    }
}