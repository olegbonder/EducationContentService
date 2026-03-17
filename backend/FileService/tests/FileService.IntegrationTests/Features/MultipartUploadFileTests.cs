using System.Net.Http.Json;
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
        
        response.EnsureSuccessStatusCode();
        
        var data = await response.Content.ReadFromJsonAsync<Envelope<StartMultiPartUploadResponse>?>(cancellationToken);
    }
}