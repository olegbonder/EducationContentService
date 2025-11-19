using CSharpFunctionalExtensions;
using EducationContentService.Core.Database;
using Framework;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace EducationContentService.Core.Features.Lessons
{
    public sealed class SoftDeleteEndpoint : IEndpoint
    {
        public void MapEndPoint(IEndpointRouteBuilder routeBuilder)
        {
            routeBuilder.MapDelete("/lessons/{lessonId:guid}",
                async Task<EndpointResult<Guid>> ([FromRoute] Guid lessonId,
                [FromServices] SoftDeleteHanlder handler,
                CancellationToken cancellationToken) =>
                    await handler.Handle(lessonId, cancellationToken)
            );
        }
    }

    public sealed class SoftDeleteHanlder
    {
        private readonly ILogger<CreateHanlder> _logger;
        private readonly ITransactionManager _transactionManager;
        private readonly ILessonsRepository _lessonsRepository;

        public SoftDeleteHanlder(
            ILogger<CreateHanlder> logger,
            ITransactionManager transactionManager,
            ILessonsRepository lessonsRepository)
        {
            _logger = logger;
            _transactionManager = transactionManager;
            _lessonsRepository = lessonsRepository;
        }

        public async Task<Result<Guid, Error>> Handle(
            Guid lessonId,
            CancellationToken cancellationToken)
        {
            var lessonResult = await _lessonsRepository.GetBy(l => l.Id == lessonId, cancellationToken);
            if (lessonResult.IsFailure)
            {
                return lessonResult.Error;
            }

            lessonResult.Value.SoftDelete();

            await _transactionManager.SaveChangesAsync(cancellationToken);

            return lessonResult.Value.Id;
        }
    }
}
