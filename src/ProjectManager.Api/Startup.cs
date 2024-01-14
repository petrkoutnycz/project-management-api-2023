using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NodaTime;
using ProjectManager.Api.BackgroundServices;
using ProjectManager.Api.Services;
using ProjectManager.Api.Settings;
using ProjectManager.Data;
using ProjectManager.Data.Entities;

namespace ProjectManager.Api;

public class Startup
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public Startup(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
         {
             var connectionString = _configuration.GetConnectionString("DefaultConnection");
             options.UseNpgsql(connectionString, builder =>
             {
                 builder.UseNodaTime();
             });
         });

        services.AddIdentityCore<ApplicationUser>(options =>
            options.SignIn.RequireConfirmedAccount = true
            )
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager();

        services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 6;
            options.Password.RequiredUniqueChars = 1;
        });

        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

        services.AddSingleton<IClock>(SystemClock.Instance);
        services.AddScoped<EmailSenderService>();
        services.AddHostedService<EmailSenderBackgroundService>();

        services.Configure<SmtpSettings>(_configuration.GetSection("SmtpSettings"));

        services.AddControllers()
            .AddNewtonsoftJson();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }

    public void Configure(IApplicationBuilder app)
    {
        // Configure the HTTP request pipeline.
        if (_environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
