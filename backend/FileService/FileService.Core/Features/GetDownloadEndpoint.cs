using Framework;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FileService.Core.Features;

public sealed class GetDownloadEndpoint : IEndpoint
{
    public void MapEndPoint(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapGet("/files/url", async (
            string bucket,
            string key,
            [FromServices] IS3Provider s3Provider,
            CancellationToken cancellationToken) =>
        {
            string result = await s3Provider.GenerateDownloadAsync(bucket, key);

            return Results.Ok(result);
        });
    }
}