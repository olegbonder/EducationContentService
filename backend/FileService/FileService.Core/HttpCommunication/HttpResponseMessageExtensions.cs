using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using FileService.Core.Features;
using Shared.SharedKernel;

namespace FileService.Core.HttpCommunication;

public static class HttpResponseMessageExtensions
{
    public static async Task<Result<TResponse, Error>> HandleResponseAsync<TResponse>(
        this HttpResponseMessage response,
        CancellationToken cancellationToken = default) where TResponse: class
    {
        try
        {
            Envelope<TResponse>? startMultiPartResponse = await response.Content
            .ReadFromJsonAsync<Envelope<TResponse>?>(cancellationToken);
        
            if (!response.IsSuccessStatusCode)
            {            
                return startMultiPartResponse?.Error ?? GeneralErrors.Failure("Error while reading response");
            }

            if (startMultiPartResponse is null)
            {            
                return GeneralErrors.Failure("Error while reading response");
            }

            if (startMultiPartResponse.Error is not null)
            {
                return startMultiPartResponse.Error;
            }

            if (startMultiPartResponse.Result is null)
            {
                return GeneralErrors.Failure("Error while reading response");
            }

            return startMultiPartResponse.Result;
        }
        catch
        {
            return GeneralErrors.Failure("Error while reading response");
        }      
    }

    public static async Task<UnitResult<Error>> HandleResponseAsync(
        this HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Envelope? startMultiPartResponse = await response.Content
                .ReadFromJsonAsync<Envelope>(cancellationToken);
        
            if (!response.IsSuccessStatusCode)
            {            
                return startMultiPartResponse?.Error ?? GeneralErrors.Failure("Error while reading response");
            }

            if (startMultiPartResponse is null)
            {            
                return GeneralErrors.Failure("Error while reading response");
            }

            if (startMultiPartResponse.Error is not null)
            {
                return startMultiPartResponse.Error;
            }

            return UnitResult.Success<Error>();
        }
        catch
        {
            return GeneralErrors.Failure("Error while reading response");
        }
    }
}