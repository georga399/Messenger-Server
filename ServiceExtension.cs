using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Messenger.Models;
using Messenger.Data;

namespace Messenger;
public static class ServiceExtensions
{
    public static void ConfigureIdentity(this IServiceCollection services, IConfiguration Configuration)
    {
        services.AddDbContextPool<ApplicationDbContext> (options => options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));
        services.AddIdentity<User, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();     
    }
}