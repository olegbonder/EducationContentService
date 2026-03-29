using System.Net;
using System.Net.Http.Json;
using Amazon.S3;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Contracts.Dtos;
using FileService.Core.Features;
using FileService.Domain;
using FileService.Domain.Assets;
using FileService.Domain.MediaProcessing;
using FileService.IntegrationTests.Infrastructure;
using FileService.VideoProcessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.SharedKernel;

namespace FileService.IntegrationTests.Features;

public class VideoProcessingTests : FileServiceTestsBase
{
    public VideoProcessingTests(IntegrationTestsWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ProcessVideoAsync_WhenValidVideoUploaded_ShouldCompleteProcessingSuccessfully()
    {
        // arrange
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        
        var processingService = scope.ServiceProvider.GetRequiredService<IVideoProcessingService>();

        var videoAssetId = await UploadTestVideoAsync(cancellationToken);
        
        // act
        var result = await processingService.ProcessVideoAsync(videoAssetId, cancellationToken);
        
        // assert
        Assert.True(result.IsSuccess);

        MediaAsset? mediaAsset = null;
        string? rawKey = null;
        
        await ExecuteInDb(async db =>
        {
            mediaAsset = await db.MediaAssets
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == videoAssetId, cancellationToken);
            
            VideoProcess? videoProcess = await db.VideoProcesses
                .AsNoTracking()
                .FirstOrDefaultAsync(vp => vp.VideoAssetId == videoAssetId, cancellationToken);
            
            Assert.NotNull(mediaAsset);
            Assert.Equal(MediaStatus.READY, mediaAsset.Status);
            
            Assert.NotNull(mediaAsset.Key);
            Assert.Equal($"hls/{videoAssetId}/master.m3u8", mediaAsset.Key.Value);

            Assert.NotNull(videoProcess);
            Assert.Equal(ProcessingStatus.COMPLETED, videoProcess.Status);

            VideoAsset? videoAsset = mediaAsset as VideoAsset;
            Assert.NotNull(videoAsset);
            Assert.NotNull(videoAsset.RawKey);
            rawKey = videoAsset.RawKey.Value;
        });

        await ExecuteInS3(async s3Client =>
        {
            StorageKey key = mediaAsset?.Key ?? throw new InvalidOperationException("MediaAsset Key is null");
            string prefix =  key.Prefix;

            var listRequest = new ListObjectsV2Request
            {
                BucketName = VideoAsset.LOCATION,
                Prefix = prefix,
            };
            
            ListObjectsV2Response listResponse = await s3Client.ListObjectsV2Async(listRequest, cancellationToken);
            
            Assert.NotEmpty(listResponse.S3Objects);
            
            GetObjectMetadataResponse objectData = await s3Client
                .GetObjectMetadataAsync(VideoAsset.LOCATION, key.Value, cancellationToken);
            Assert.NotNull(objectData);
            
            AmazonS3Exception exception = await Assert.ThrowsAsync<AmazonS3Exception>(
                async () =>
                    await s3Client.GetObjectMetadataAsync(VideoAsset.LOCATION, rawKey, cancellationToken));
            
            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        });
    }

    private async Task<Guid> UploadTestVideoAsync(CancellationToken cancellationToken)
    {
        FileInfo fileInfo = new(Path.Combine(AppContext.BaseDirectory, TEST_FILE_DIRECTORY, TEST_FILE_NAME));
        
        var startResponse = await StartMultiPartUpload(fileInfo, cancellationToken);
        
        var partEtags = await UploadChunksAsync(fileInfo, startResponse, cancellationToken);
        
        await CompleteMultiPartUpload(startResponse, partEtags, cancellationToken);

        return startResponse.MediaAssetId;
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
        
        return startMultipartResult.Value;
    }

    private async Task<IReadOnlyList<PartEtagDto>> UploadChunksAsync(
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