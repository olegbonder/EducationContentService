namespace FileService.IntegrationTests.Infrastructure;

public class FileServiceTestsBase : IClassFixture<IntegrationTestsWebFactory>
{
    public FileServiceTestsBase(IntegrationTestsWebFactory  factory)
    {
        AppHttpClient = factory.CreateClient();
        HttpClient = new HttpClient();
        Services =  factory.Services;
    }

    protected HttpClient HttpClient { get; init; }
    protected IServiceProvider Services { get; init; }
    protected HttpClient AppHttpClient  { get; init; }
}