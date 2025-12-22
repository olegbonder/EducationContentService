using Core.Validation;
using CSharpFunctionalExtensions;
using EducationContentService.Contracts;
using EducationContentService.Core.Database;
using FluentValidation;
using Framework;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Shared.SharedKernel;

namespace EducationContentService.Core.Features.Lessons
{
    public sealed class GetLessonRequestValidator : AbstractValidator<GetLessonRequest>
    {
        public GetLessonRequestValidator()
        {
            RuleFor(l => l.Search)
                .MaximumLength(1000).WithError(GeneralErrors.ValueIsInvalid("search"));

            RuleFor(l => l.Page)
                .NotNull().WithError(GeneralErrors.ValueIsInvalid("page"))
                .GreaterThan(0).WithError(GeneralErrors.ValueIsInvalid("page"));

            RuleFor(l => l.PageSize)
                .NotNull()
                .WithError(GeneralErrors.ValueIsInvalid("pageSize"))
                .GreaterThan(0).WithError(GeneralErrors.ValueIsInvalid("pageSize"));
        }
    }
    public sealed class GetEndpoint : IEndpoint
    {
        public void MapEndPoint(IEndpointRouteBuilder routeBuilder)
        {
            routeBuilder.MapGet("/lessons", 
                async Task<EndpointResult<PaginationLessonResponse>>(
                [AsParameters] GetLessonRequest request,
                [FromServices] GetHanlder handler, 
                CancellationToken cancellationToken) =>
                    await handler.Handle(request, cancellationToken)
            );
        }
    }

    public sealed class GetHanlder
    {
        private readonly IEducationReadDbContext _readDbContext;
        private readonly IValidator<GetLessonRequest> _validator;

        public GetHanlder(
            IEducationReadDbContext readDbContext,
            IValidator<GetLessonRequest> validator)
        {
            _readDbContext = readDbContext;
            _validator = validator;
        }

        public async Task<Result<PaginationLessonResponse, Error>> Handle(
            GetLessonRequest request, 
            CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (validationResult.IsValid == false)
            {
                return validationResult.ToError();
            }

            var query = _readDbContext.LessonQuery;

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(l => l.Title.Value.Contains(request.Search));
            }

            int lessonsCount = await query.CountAsync(cancellationToken);

            var lessons = await query.Select(l => new LessonDto
            {
                Id = l.Id,
                Title = l.Title.Value,
                Description = l.Description.Value,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt
            })
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PaginationLessonResponse(lessons, lessonsCount);
        }
    }
}
