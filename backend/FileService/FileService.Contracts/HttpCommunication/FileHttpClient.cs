using CSharpFunctionalExtensions;
using FileService.Contracts.Dtos;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;
using System.Net.Http.Json;

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
            var response = await _httpClient.PostAsync($"api/files/batch", JsonContent.Create(request), cancellationToken);
            return await response.HandleResponseAsync<GetMediaAssetsResponse>(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting media assets for {MediaAssetIds}",  request.MediaAssetIds);
            return Error.Failure("server.error", "Failed to get media assets");
        }
    }

    public async Task<Result<CheckMediaAssetExistsResponse, Error>> CheckMediaAssetExists(Guid mediaAssetId,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient
                .GetAsync($"api/files/{mediaAssetId}/exists", cancellationToken);
            return await response.HandleResponseAsync<CheckMediaAssetExistsResponse>(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking media asset exists for {MediaAssetId}",  mediaAssetId);
            return Error.Failure("server.error", "Failed to check media asset exists");
        }
    }
}