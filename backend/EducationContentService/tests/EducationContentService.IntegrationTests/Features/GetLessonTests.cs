using System.Net.Http.Json;
using Core.HttpCommunication;
using EducationContentService.Contracts;
using EducationContentService.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace EducationContentService.IntegrationTests.Features;

public class GetLessonTests : EducationServiceTestsBase
{
    public GetLessonTests(IntegrationTestsWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetLessons_FullCycle_PersistsMediaFile()
    {
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        var getLessonsRequest = new GetLessonsRequest(null, null, 1, 3);

        var queryParams = new Dictionary<string, string>
        {
            {
                "page", getLessonsRequest.Page.ToString()
            },
            {
                "pageSize", getLessonsRequest.PageSize.ToString()
            }
        };
        string url = QueryHelpers.AddQueryString("api/lessons", queryParams);
        var getLessonsResponse = await AppHttpClient.GetAsync(url, cancellationToken);
        
        // act
        var lessonsResult = await getLessonsResponse
            .HandleResponseAsync<PaginationLessonResponse>(cancellationToken);
                
        // assert
        Assert.True(lessonsResult.IsSuccess);
        Assert.Equal(3, lessonsResult.Value.Items.Count);
    }
}
