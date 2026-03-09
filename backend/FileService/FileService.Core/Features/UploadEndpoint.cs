using Framework;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FileService.Core.Features;

public sealed class UploadEndpoint : IEndpoint
{
    public void MapEndPoint(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapPost("/files", async (
            [FromForm] IFormFile formFile,
            [FromServices] IS3Provider s3Provider,
            CancellationToken cancellationToken) =>
        {
            string key = $"raw/{Guid.NewGuid()}";
            await s3Provider.UploadFile(formFile.OpenReadStream(), "pictures", key, formFile.ContentType, cancellationToken);
        }).DisableAntiforgery();
    }
}