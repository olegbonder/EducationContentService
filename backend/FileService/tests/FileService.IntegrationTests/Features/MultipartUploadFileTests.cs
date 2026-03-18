using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Core.Features;
using FileService.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Shared.SharedKernel;

namespace FileService.IntegrationTests.Features;

public class MultipartUploadFileTests : FileServiceTestsBase
{
    public MultipartUploadFileTests(IntegrationTestsWebFactory factory) 
        : base(factory)
    {
    }

    [Fact]
    public async Task MultipartUpload_FullCycle_PersistsMediaFile()
    {
        var cancellationToken = new CancellationTokenSource().Token;
        var request = new StartMultiPartUploadRequest(
            "file.mp4",
            "video",
            "video/mp4",
            10000,
            "lesson",
            Guid.NewGuid());
        
        var response = await AppHttpClient.PostAsJsonAsync("/api/files/multipart-upload", request, cancellationToken);

        /*var error = await response.Content.ReadFromJsonAsync<Envelope<StartMultiPartUploadResponse>?>(cancellationToken);

        Result<StartMultiPartUploadResponse, Error> startMultiPartResult;
        if (!response.IsSuccessStatusCode)
        {
            
            startMultiPartResponse = error?.Error ?? Error.Failure("test.error", "Unknown error");
        }*/

        response.EnsureSuccessStatusCode();
        
        var data = await response.Content.ReadAsStringAsync();//
        var data1 = await response.Content.ReadFromJsonAsync<Envelope<object>?>(cancellationToken);
    }
}