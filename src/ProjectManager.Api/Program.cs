using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.Json;
using ProjectManager.Data;
using System;

// TODO: dockerfile with https://csharp.christiannagel.com/2022/08/30/no-more-dockerfiles-with-help-of-net-cli/
// TODO: be careful with CRUD-oriented controllers!!!

namespace ProjectManager.Api;
public static class Program
{
    private static string ContentRootPath = Directory.GetCurrentDirectory();

    public static async Task Main(string[] args)
    {
        var builder = CreateHostBuilder(args);
        var host = builder.Build();

        // TODO: can take how much time...?
        // TODO: cooperation in scaled up app
        await MigrateDb(host);

        await host.RunAsync();
    }

    private static async Task MigrateDb(IHost host)
    {
        using var scope = host.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Database.MigrateAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) =>
            {
                // TODO: motivation?
                for (int pos = configurationBuilder.Sources.Count - 1; pos >= 0; --pos)
                {
                    ContentRootPath = hostBuilderContext.HostingEnvironment.ContentRootPath;
                    if (configurationBuilder.Sources[pos] is JsonConfigurationSource)
                    {
                        var source = new JsonConfigurationSource()
                        {
                            Path = Path.Join(ContentRootPath, "appsettings.local.json"),
                            Optional = true,
                            ReloadOnChange = true,
                        };
                        source.ResolveFileProvider();
                        configurationBuilder.Sources.Insert(pos + 1, source);
                    }
                }
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            })
            ;
    }
}
