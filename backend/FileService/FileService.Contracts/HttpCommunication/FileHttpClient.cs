using CSharpFunctionalExtensions;
using FileService.Contracts.Dtos;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace FileService.Contracts.HttpCommunication;

internal sealed class FileHttpClient : IFileCommunicationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FileHttpClient> _logger;

    public FileHttpClient(HttpClient httpClient, ILogger<FileHttpClient>  logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    public async Task<Result<GetMediaAssetsResponse, Error>> GetMediaAssets(GetMediaAssetsRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/files/batch", cancellationToken);
            return await response.HandleResponseAsync<GetMediaAssetsResponse>(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting media assets for {MediaAssetIds}",  request.MediaAssetIds);
            return Error.Failure("server.error", "Failed to get media assets");
        }
    }
}