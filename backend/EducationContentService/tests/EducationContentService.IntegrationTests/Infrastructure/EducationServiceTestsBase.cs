namespace EducationContentService.IntegrationTests.Infrastructure;

public class EducationServiceTestsBase : IClassFixture<IntegrationTestsWebFactory>
{
    public EducationServiceTestsBase(IntegrationTestsWebFactory  factory)
    {
        AppHttpClient = factory.CreateClient();
        HttpClient = new HttpClient();
        Services =  factory.Services;
    }

    protected HttpClient HttpClient { get; init; }
    protected IServiceProvider Services { get; init; }
    protected HttpClient AppHttpClient  { get; init; }
}