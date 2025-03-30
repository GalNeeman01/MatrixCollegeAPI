using AspNetCoreRateLimit;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Matrix;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Setup Serilog Logger
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration).CreateLogger();

        builder.Host.UseSerilog();

        // Setup ratelimit
        builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("RateLimit"));
        builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        builder.Services.AddInMemoryRateLimiting();

        // Add DI services
        builder.Services.AddDbServices();
        builder.Services.AddUtilityServices();
        builder.Services.AddDaoServices();

        // Add Fluent DI validators
        builder.Services.AddValidatorsFromAssemblyContaining<UserValidator>();

        // IOptions DIs
        builder.Services.Configure<LogSettings>(
            builder.Configuration.GetSection(nameof(LogSettings)));

        builder.Services.Configure<AuthSettings>(
            builder.Configuration.GetSection(nameof(AuthSettings)));

        builder.Services.Configure<DatabaseSettings>(
            builder.Configuration.GetSection(nameof(DatabaseSettings)));

        // Add jobs
        builder.Services.AddHostedService<LogCleanerService>();

        // Add CORS Policies
        builder.Services.AddCorsPolicies();

        // Add global filters
        builder.Services.AddMvc(options => {
            options.Filters.Add<CatchAllFilter>();
            options.Filters.Add<AutoValidationFilter>();
        });

        // Ignore EF ModelState input validation (To allow Fluent to work)
        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        // Apply JWT settings
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            AuthSettings authSettings = builder.Configuration.GetSection(nameof(AuthSettings)).Get<AuthSettings>()!;
            JwtBearerOptionsSetup.Configure(options, authSettings);
        });

        builder.Services.AddControllers();

        var app = builder.Build();

        // Apply CORS
        app.UseCors("AllowAll");

        // Middleware
        app.UseIpRateLimiting();
        app.UseSerilogRequestLogging();
        app.UseAuthorization();
        app.MapControllers();
        app.UseNullOrEmptyJsonMiddleware();

        app.Run();
    }
}
