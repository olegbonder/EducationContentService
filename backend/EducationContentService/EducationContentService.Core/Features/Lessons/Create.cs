using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace EducationContentService.Core.Features.Lessons
{
    public class CreateEndpoint : IEndpoint
    {
        public void MapEndPoint(IEndpointRouteBuilder routeBuilder)
        {
            routeBuilder.MapPost("api/lessons", async (CreateHanlder handler) =>
            {
                await handler.Handle();
            });
        }
    }

    public sealed class CreateHanlder
    {
        private readonly ILogger<CreateHanlder> _logger;

        public CreateHanlder(ILogger<CreateHanlder> logger)
        {
            _logger = logger;
        }

        public async Task Handle()
        {
            _logger.LogInformation("Create a new lesson");
            await Task.Delay(TimeSpan.FromSeconds(2));
        }
    }
}
