namespace FileService.Core;

public interface IS3Provider
{
    Task UploadFile(Stream stream, string bucketName,
        string key, string contentType, CancellationToken cancellationToken = default);

    Task<string> GenerateDownloadAsync(string bucketName, string key);
}