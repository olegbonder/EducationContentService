using CSharpFunctionalExtensions;
using EducationContentService.Core.Database;
using EducationContentService.Infrastructure.Postgres;
using EducationContentService.IntegrationTests.Mocks;
using FileService.Contracts;
using FileService.Contracts.Dtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using NSubstitute;
using Shared.SharedKernel;
using Testcontainers.PostgreSql;

namespace EducationContentService.IntegrationTests.Infrastructure;

public class IntegrationTestsWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres")
        .WithDatabase("education-service_db_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();
        
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.IntegrationTests.json"), optional: true);
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Database"] = _dbContainer.GetConnectionString()
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<EducationDbContext>();
            services.RemoveAll<IEducationReadDbContext>();

            services.AddDbContextPool<EducationDbContext>((sp, options) =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
            });

            services.AddDbContextPool<IEducationReadDbContext, EducationDbContext>((sp, options) =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
            });
            
            services.RemoveAll<IFileCommunicationService>();

            IFileCommunicationService mock = Substitute.For<IFileCommunicationService>();

            var response = new GetMediaAssetsResponse([
                new GetMediaAssetsDto(Guid.NewGuid(), "ready", "video", "url"),
                new GetMediaAssetsDto(Guid.NewGuid(), "ready", "video", "url"),
                new GetMediaAssetsDto(Guid.NewGuid(), "ready", "video", "url")
            ]);

            var result = Result.Success<GetMediaAssetsResponse, Error>(response);

            mock.GetMediaAssets(Arg.Any<GetMediaAssetsRequest>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(result));
            services.AddScoped<IFileCommunicationService, FileServiceCommunicationMock>();
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EducationDbContext>();
        
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
        
    }
}