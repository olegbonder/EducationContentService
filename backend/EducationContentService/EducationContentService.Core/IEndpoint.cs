using Microsoft.AspNetCore.Routing;

namespace EducationContentService.Core
{
    public interface IEndpoint
    {
        void MapEndPoint(IEndpointRouteBuilder routeBuilder);
    }
}