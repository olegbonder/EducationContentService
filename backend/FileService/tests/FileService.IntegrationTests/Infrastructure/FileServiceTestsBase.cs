using Amazon.S3;
using FileService.Infrastructure.Postgres;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.IntegrationTests.Infrastructure;

[Collection("FileTestsCollection")]
public class FileServiceTestsBase : IAsyncLifetime, IClassFixture<IntegrationTestsWebFactory>
{
    private readonly IntegrationTestsWebFactory _factory;
    protected const string TEST_FILE_DIRECTORY = "Resources";
    protected const string TEST_FILE_NAME = "test-file.mp4";

    protected FileServiceTestsBase(IntegrationTestsWebFactory factory)
    {
        _factory = factory;
        AppHttpClient = _factory.CreateClient();
        HttpClient = new HttpClient();
        Services =  _factory.Services;
    }

    protected HttpClient HttpClient { get; init; }
    protected IServiceProvider Services { get; init; }
    protected HttpClient AppHttpClient  { get; init; }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    protected async Task ExecuteInDb(Func<FileServiceDbContext, Task> action)
    {
        await using var scope = Services.CreateAsyncScope();

        FileServiceDbContext dbContext = scope.ServiceProvider.GetRequiredService<FileServiceDbContext>();

        await action(dbContext);
    }
    
    protected async Task ExecuteInS3(Func<IAmazonS3, Task> action)
    {
        await using var scope = Services.CreateAsyncScope();

        IAmazonS3 s3Client = scope.ServiceProvider.GetRequiredService<IAmazonS3>();

        await action(s3Client);
    }
}