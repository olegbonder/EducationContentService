using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Core.Models;
using FileService.Domain;
using Shared.SharedKernel;

namespace FileService.Core.FilesStorage;

public interface IS3Provider
{
    Task<Result<string, Error>> StartMultiPartUploadAsync(
        StorageKey storageKey,
        MediaData mediaData,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<ChunkUploadUrl>, Error>> GenerateAllChunksUploadUrlsAsync(
        StorageKey storageKey,
        string uploadId,
        int totalChunks,
        CancellationToken cancellationToken);

    Task<Result<string, Error>> GenerateDownloadUrlAsync(StorageKey storageKey);

    Task<Result<IReadOnlyList<MediaUrl>, Error>> GenerateDownloadUrlsAsync(IEnumerable<StorageKey> storageKeys); 

    Task<Result<string, Error>> GenerateUploadUrlAsync(StorageKey storageKey);
    Task<Result<string, Error>> CompleteMultiPartUploadAsync(
        StorageKey storageKey,
        string uploadId,
        IReadOnlyList<PartEtagDto> partETags,
        CancellationToken cancellationToken);
}