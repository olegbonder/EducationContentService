using System.Net.Http.Json;
using FileService.Contracts;
using FileService.Core.Features;
using FileService.Infrastructure.Postgres;
using FileService.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using FileService.Domain.Assets;
using FileService.Domain;
using Shared.SharedKernel;
using CSharpFunctionalExtensions;
using Amazon.S3;
using FileService.Contracts.Dtos;

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
        // arrange
        var cancellationToken = new CancellationTokenSource().Token;

        FileInfo fileInfo = new(Path.Combine(AppContext.BaseDirectory, TEST_FILE_DIRECTORY, TEST_FILE_NAME));
        
        // act
        var startMultiPartUploadResponse = await StartMultiPartUpload(fileInfo, cancellationToken);

        var partEtags = await UploadChunks(fileInfo, startMultiPartUploadResponse, cancellationToken);

        var result = await CompleteMultiPartUpload(startMultiPartUploadResponse, partEtags, cancellationToken);

        // assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async db =>
        {            
            MediaAsset? mediaAsset = await db.MediaAssets
                .FirstOrDefaultAsync(m => m.Id == startMultiPartUploadResponse.MediaAssetId, cancellationToken);

            Assert.NotNull(mediaAsset);
            Assert.Equal(MediaStatus.UPLOADED, mediaAsset?.Status);

            var amazonS3Client = Services.GetRequiredService<IAmazonS3>();

            var objectResponse = await amazonS3Client.GetObjectAsync(
                mediaAsset.Key.Location,
                mediaAsset.Key.Value,
                cancellationToken
            );
            
            Assert.Equal(fileInfo.Length, objectResponse.ContentLength);
        });        
    }

    private async Task<StartMultiPartUploadResponse> StartMultiPartUpload(FileInfo fileInfo, CancellationToken cancellationToken)
    {
        var request = new StartMultiPartUploadRequest(
            fileInfo.Name,
            "video",
            "video/mp4",
            fileInfo.Length,
            "lesson",
            Guid.NewGuid());
        
        //act
        var startMultipartResponse = await AppHttpClient.PostAsJsonAsync("/api/files/multipart-upload", request, cancellationToken);
             
        var startMultipartResult = await startMultipartResponse
            .HandleResponseAsync<StartMultiPartUploadResponse>(cancellationToken);
                
        //assert
        Assert.True(startMultipartResult.IsSuccess);
        Assert.NotNull(startMultipartResult.Value.UploadId);

        await ExecuteInDb(async db =>
        {            
            MediaAsset? mediaAsset = await db.MediaAssets
                .FirstOrDefaultAsync(m => m.Id == startMultipartResult.Value.MediaAssetId, cancellationToken);

            Assert.NotNull(mediaAsset);
            Assert.Equal(MediaStatus.UPLOADING, mediaAsset?.Status);
        });
        
        return startMultipartResult.Value;
    }

    private async Task<IReadOnlyList<PartEtagDto>> UploadChunks(
        FileInfo fileInfo, 
        StartMultiPartUploadResponse startMultiPartUploadResponse,
        CancellationToken cancellationToken)
    {
        await using var stream = fileInfo.OpenRead();

        var parts = new List<PartEtagDto>();

        foreach (ChunkUploadUrl chunkUploadUrl in startMultiPartUploadResponse.ChunkUploadUrls.OrderBy(c => c.PartNumber))
        {
            var chunk = new byte[startMultiPartUploadResponse.ChunkSize];
            int bytesRead = await stream.ReadAsync(chunk.AsMemory(0, startMultiPartUploadResponse.ChunkSize), cancellationToken);
            if (bytesRead == 0)
                break;

            var content = new ByteArrayContent(chunk);
            var response = await HttpClient.PutAsync(chunkUploadUrl.UploadUrl, content, cancellationToken);

            var etag = response.Headers.ETag?.Tag.Trim('"');

            parts.Add(new PartEtagDto(chunkUploadUrl.PartNumber, etag!));
        }

        return parts;
    }

    private async Task<UnitResult<Error>> CompleteMultiPartUpload(
        StartMultiPartUploadResponse startMultiPartUploadResponse, 
        IEnumerable<PartEtagDto> partETags,
        CancellationToken cancellationToken)
    {
        var completeRequest = new CompleteMultiPartUploadRequest(
            startMultiPartUploadResponse.MediaAssetId,
            startMultiPartUploadResponse.UploadId,
            partETags.ToList()
        );

        var completeResponse = await AppHttpClient.PostAsJsonAsync("/api/files/complete-upload", completeRequest, cancellationToken);
        var completeResult = await completeResponse.HandleResponseAsync(cancellationToken);

        return completeResult;
    }
}