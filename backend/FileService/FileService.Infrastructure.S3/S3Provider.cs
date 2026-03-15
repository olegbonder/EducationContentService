using Amazon.S3;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Core.FilesStorage;
using FileService.Core.Models;
using FileService.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.SharedKernel;

namespace FileService.Infrastructure.S3;

public class S3Provider: IS3Provider
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<S3Provider> _logger;
    private readonly S3Options _s3Options;

    private readonly SemaphoreSlim _requestsSemaphore;

    public S3Provider(IAmazonS3 s3Client, IOptions<S3Options> s3Options, ILogger<S3Provider> logger)
    {
        _s3Client = s3Client;
        _logger = logger;
        _s3Options = s3Options.Value;
        _requestsSemaphore = new SemaphoreSlim(1, _s3Options.MaxConcurrentRequests);
    }

    public async Task<Result<string, Error>> StartMultiPartUploadAsync(
        StorageKey storageKey,
        MediaData mediaData,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new InitiateMultipartUploadRequest
            {
                BucketName = storageKey.Location, 
                Key = storageKey.Value, 
                ContentType = mediaData.ContentType.Value
            };
            var result = await _s3Client.InitiateMultipartUploadAsync(request, cancellationToken);

            return result.UploadId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting multipart upload");

            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<IReadOnlyList<ChunkUploadUrl>, Error>> GenerateAllChunksUploadUrlsAsync(
        StorageKey storageKey,
        string uploadId,
        int totalChunks,
        CancellationToken cancellationToken)
    {
        try
        {
            var tasks = Enumerable.Range(1, totalChunks)
                .Select(async partNumber =>
                {
                    await _requestsSemaphore.WaitAsync(cancellationToken);
                    try
                    {
                        var request = new GetPreSignedUrlRequest
                        {
                            BucketName = storageKey.Location,
                            Key = storageKey.Value,
                            Verb = HttpVerb.PUT,
                            UploadId = uploadId,
                            PartNumber = partNumber,
                            Expires = DateTime.UtcNow.AddHours(_s3Options.UploadUrlExpirationHours),
                            Protocol = _s3Options.WithSsl ? Protocol.HTTPS : Protocol.HTTP
                        };
                        string? url = await _s3Client.GetPreSignedURLAsync(request);

                        return new ChunkUploadUrl(partNumber, url);
                    }
                    finally
                    {
                        _requestsSemaphore.Release();
                    }
                });

            ChunkUploadUrl[] results = await Task.WhenAll(tasks);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error get chunks url multipart upload");
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<string, Error>> GenerateDownloadUrlAsync(StorageKey storageKey)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = storageKey.Location,
                Key = storageKey.Value,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddDays(_s3Options.DownloadExpirationDays),
                Protocol = _s3Options.WithSsl ? Protocol.HTTPS : Protocol.HTTP
            };
            string? response = await _s3Client.GetPreSignedURLAsync(request);

            return response;
        }
        catch (Exception ex)
        {
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<IReadOnlyList<MediaUrl>, Error>> GenerateDownloadUrlsAsync(IEnumerable<StorageKey> storageKeys)
    {
        try
        {
            var tasks = storageKeys.Select(async storageKey =>
            {
                await _requestsSemaphore.WaitAsync();
                try
                {
                    var request = new GetPreSignedUrlRequest
                    {
                        BucketName = storageKey.Location,
                        Key = storageKey.Value,
                        Verb = HttpVerb.GET,
                        Expires = DateTime.UtcNow.AddDays(_s3Options.DownloadExpirationDays),
                        Protocol = _s3Options.WithSsl ? Protocol.HTTPS : Protocol.HTTP
                    };
                    string? response = await _s3Client.GetPreSignedURLAsync(request);

                    return new MediaUrl(storageKey, response);
                }
                finally
                {
                    _requestsSemaphore.Release();
                }
            });

            var results =  await Task.WhenAll(tasks);
            return results;
        }
        catch (Exception ex)
        {
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<string, Error>> GenerateUploadUrlAsync(StorageKey storageKey)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = storageKey.Location,
                Key = storageKey.Value,
                Verb = HttpVerb.PUT,
                Expires = DateTime.UtcNow.AddHours(_s3Options.UploadUrlExpirationHours),
                Protocol = _s3Options.WithSsl ? Protocol.HTTPS : Protocol.HTTP
            };
            string? response = await _s3Client.GetPreSignedURLAsync(request);

            return response;
        }
        catch (Exception ex)
        {
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<string, Error>> CompleteMultiPartUploadAsync(
        StorageKey storageKey,
        string uploadId,
        IReadOnlyList<PartEtagDto> partETags,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new CompleteMultipartUploadRequest
            {
                BucketName = storageKey.Location,
                Key = storageKey.Value,
                UploadId = uploadId,
                PartETags = partETags.Select(p => new PartETag { ETag = p.ETag, PartNumber = p.PartNumber }).ToList(),
            };

            var response = await _s3Client.CompleteMultipartUploadAsync(request, cancellationToken);

            return response.Key;
        }
        catch (Exception ex)
        {
            return S3ErrorMapper.ToError(ex);
        }
    }    
}