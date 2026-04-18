using Serilog;
using System.Globalization;
using FileService.Web.Configuration;
using FileService.Core.Messaging;
using FileService.Infrastructure.Postgres.Initializers;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateLogger();

try
{
    Log.Information("Starting web application");

    var builder = WebApplication.CreateBuilder(args);

    var environment = builder.Environment.EnvironmentName;

    builder.Configuration.AddJsonFile($"appsettings.{environment}.json", true, true);

    builder.Configuration.AddEnvironmentVariables();

    builder.Services.AddConfiguration(builder.Configuration);

    builder.AddWolverine();

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var quartzDbInitializer = scope.ServiceProvider.GetRequiredService<QuartzDbInitializer>();
        await quartzDbInitializer.InitializeAsync();

    }

    app.Configure();

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public abstract partial class Program;