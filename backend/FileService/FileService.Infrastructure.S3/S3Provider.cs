using Amazon.S3;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.SharedKernel;

namespace FileService.Infrastructure.S3;

public class S3Provider: IDisposable, IS3Provider
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
        string bucketName,
        string key,
        string contentType,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new InitiateMultipartUploadRequest
            {
                BucketName = bucketName, Key = key, ContentType = contentType
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

    public async Task<Result<IReadOnlyList<string>, Error>> GenerateAllChunksUploadUrlsAsync(
        string bucketName,
        string key,
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
                            BucketName = bucketName,
                            Key = key,
                            Verb = HttpVerb.PUT,
                            UploadId = uploadId,
                            PartNumber = partNumber,
                            Expires = DateTime.UtcNow.AddHours(_s3Options.UploadUrlExpirationHours),
                            Protocol = _s3Options.WithSsl ? Protocol.HTTPS : Protocol.HTTP
                        };
                        string? url = await _s3Client.GetPreSignedURLAsync(request);

                        return url;
                    }
                    finally
                    {
                        _requestsSemaphore.Release();
                    }
                });

            string[] results = await Task.WhenAll(tasks);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error get chunks url multipart upload");
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<string, Error>> GenerateDownloadUrlAsync(string bucketName, string key)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = key,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddHours(_s3Options.DownloadExpirationHours),
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

    public async Task<Result<string, Error>> GenerateUploadUrlAsync(string bucketName, string key)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = key,
                Verb = HttpVerb.PUT,
                Expires = DateTime.UtcNow.AddHours(_s3Options.DownloadExpirationHours),
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
        string bucketName,
        string key,
        string uploadId,
        IReadOnlyList<PartEtagDto> partETags,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new CompleteMultipartUploadRequest
            {
                BucketName = bucketName,
                Key = key,
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

    public void Dispose()
    {
        _s3Client.Dispose();
        _requestsSemaphore.Dispose();
    }
}