using CSharpFunctionalExtensions;
using FileService.Contracts;
using Shared.SharedKernel;

namespace FileService.Core;

public interface IS3Provider
{
    Task<Result<string, Error>> StartMultiPartUploadAsync(
        string bucketName,
        string key,
        string contentType,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<string>, Error>> GenerateAllChunksUploadUrlsAsync(
        string bucketName,
        string key,
        string uploadId,
        int totalChunks,
        CancellationToken cancellationToken);

    Task<Result<string, Error>> GenerateDownloadUrlAsync(string bucketName, string key);

    Task<Result<string, Error>> GenerateUploadUrlAsync(string bucketName, string key);

    Task<Result<string, Error>> CompleteMultiPartUploadAsync(
        string bucketName,
        string key,
        string uploadId,
        IReadOnlyList<PartEtagDto> partETags,
        CancellationToken cancellationToken);
}