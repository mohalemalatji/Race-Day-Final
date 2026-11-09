using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using RaceDay.Api.Data;

namespace RaceDay.Api.Tests;

public sealed class TestFactory : WebApplicationFactory<Program>
{
    private const string TestJwtKey = "test-secret-key-that-is-long-enough-for-hmac-sha256";
    private const string TestJwtIssuer = "RaceDay.Api";
    private const string TestJwtAudience = "RaceDay.Client";
    private readonly string _databaseName = Guid.NewGuid().ToString("N");


    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = TestJwtKey,
            ["Jwt:Issuer"] = TestJwtIssuer,
            ["Jwt:Audience"] = TestJwtAudience,
            ["Database:ApplyMigrationsOnStartup"] = "false"
        }));
        builder.ConfigureServices(services =>
        {
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters.ValidIssuer = TestJwtIssuer;
                options.TokenValidationParameters.ValidAudience = TestJwtAudience;
                options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtKey));
            });

            services.RemoveAll<DbContextOptions<RaceDayDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<RaceDayDbContext>>();
            services.RemoveAll<RaceDayDbContext>();
            services.AddDbContext<RaceDayDbContext>(options => options.UseInMemoryDatabase(_databaseName)
                .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
        });
    }
}
