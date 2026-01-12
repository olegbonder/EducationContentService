using System.Globalization;
using EducationContentService.Core.Features.Lessons;
using EducationContentService.Infrastructure.Postgres;
using EducationContentService.Web.Configuration;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web application");

    var builder = WebApplication.CreateBuilder(args);

    var environment = builder.Environment.EnvironmentName;

    builder.Configuration.AddJsonFile($"appsettings.{environment}.json", true, true);

    builder.Configuration.AddEnvironmentVariables();

    builder.Services.AddConfiguration(builder.Configuration);

    //builder.Services.AddScoped<ILessonsRepository, LessonsRepository>();

    var app = builder.Build();

    app.Configure();

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<EducationDbContext>();
        dbContext.Database.EnsureCreated(); // Создает БД без миграций
                                            // ИЛИ с миграциями:
                                            // dbContext.Database.Migrate();
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}