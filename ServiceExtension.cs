using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Messenger.Models;
using Messenger.Data;
using Messenger.Repositories;

namespace Messenger;
public static class ServiceExtensions
{
    public static void ConfigureIdentityDb(this IServiceCollection services, IConfiguration Configuration)
    {
        services.AddDbContextPool<ApplicationDbContext> (options => options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));
        services.AddIdentity<User, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();     
    }
    public static void ConfigureRepos(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IChatRepository, ChatRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IConnectionRepository, ConnectionRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }
}