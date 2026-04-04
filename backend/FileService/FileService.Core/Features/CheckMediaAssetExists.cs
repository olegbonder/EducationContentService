using CSharpFunctionalExtensions;
using FileService.Contracts.Dtos;
using FileService.Core.Database;
using Framework;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Shared.SharedKernel;

namespace FileService.Core.Features
{
    public class CheckMediaAssetExistsEndpoint : IEndpoint
    {
        public void MapEndPoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/files/{mediaAssetId:guid}/exists", 
                async Task<EndpointResult<CheckMediaAssetExistsResponse>> (
                    [FromRoute] Guid mediaAssetId,
                    [FromServices] CheckMediaAssetExistsHandler handler,
                    CancellationToken cancellationToken) =>
                await handler.Handle(mediaAssetId, cancellationToken));
        }
    }
    
    public sealed class CheckMediaAssetExistsHandler
    {        
        private readonly IReadDbContext _readDbContext;

        public CheckMediaAssetExistsHandler(IReadDbContext readDbContext)
        {
            _readDbContext = readDbContext;
        }

        public async Task<Result<CheckMediaAssetExistsResponse, Error>> Handle(Guid mediaAssetId, CancellationToken cancellationToken)
        {
            bool exists = await _readDbContext.MediaAssetsQuery
                .AnyAsync(m => m.Id == mediaAssetId, cancellationToken);

            return new CheckMediaAssetExistsResponse(exists);
        }
    }
}