using Microsoft.AspNetCore.Routing;

namespace Framework
{
    public interface IEndpoint
    {
        void MapEndPoint(IEndpointRouteBuilder routeBuilder);
    }
}