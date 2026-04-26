using Core.Validation;
using CSharpFunctionalExtensions;
using EducationContentService.Contracts;
using EducationContentService.Core.Database;
using FileService.Contracts;
using FileService.Contracts.Dtos;
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
    public sealed class GetLessonRequestValidator : AbstractValidator<GetLessonsRequest>
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
                [AsParameters] GetLessonsRequest request,
                [FromServices] GetHanlder handler, 
                CancellationToken cancellationToken) =>
                    await handler.Handle(request, cancellationToken)
            );
        }
    }

    public sealed class GetHanlder
    {
        private readonly IEducationReadDbContext _readDbContext;
        private readonly IFileCommunicationService _fileCommunicationService;
        private readonly IValidator<GetLessonsRequest> _validator;

        public GetHanlder(
            IEducationReadDbContext readDbContext,
            IFileCommunicationService fileCommunicationService,
            IValidator<GetLessonsRequest> validator)
        {
            _readDbContext = readDbContext;
            _fileCommunicationService = fileCommunicationService;
            _validator = validator;
        }

        public async Task<Result<PaginationLessonResponse, Error>> Handle(
            GetLessonsRequest request, 
            CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (validationResult.IsValid == false)
            {
                return validationResult.ToError();
            }

            var query = _readDbContext.LessonQuery.IgnoreQueryFilters();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(l => l.Title.Value.Contains(request.Search));
            }

            if (request.IsDeleted.HasValue)
            {
                query = query.Where(l => l.IsDeleted == request.IsDeleted.Value);
            }

            int lessonsCount = await query.CountAsync(cancellationToken);

            var lessons = await query
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => new LessonDto
            {
                Id = l.Id,
                Title = l.Title.Value,
                Description = l.Description.Value,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt,
                IsDeleted = l.IsDeleted,
                Video = l.VideoId.HasValue ? new MediaDto
                {
                    Id = l.VideoId.Value
                } : null
            })
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                
                .ToListAsync(cancellationToken);

            int totalPages = (int)Math.Ceiling(lessonsCount/ (double)request.PageSize);

            var mediaAssetIds = lessons
                .Where(l => l.Video != null)
                .Select(l => l.Video!.Id)
                .ToList();
            var mediaAssets = await _fileCommunicationService
                .GetMediaAssets(new GetMediaAssetsRequest(mediaAssetIds), cancellationToken);
            if (mediaAssets.IsFailure)
                return mediaAssets.Error;

            var mediaAssetsDict = mediaAssets.Value.Items
                .ToDictionary(x => x.Id, x => x);

            foreach (var lessonDto in lessons)
            {
                if (lessonDto.Video != null && mediaAssetsDict.TryGetValue(lessonDto.Video.Id, out GetMediaAssetsDto? mediaAsset))
                {
                    lessonDto.Video = new MediaDto
                    {
                        Id = mediaAsset.Id,
                        Status = mediaAsset.Status,
                        Url = mediaAsset.Url,
                    };
                }
            }
            return new PaginationLessonResponse(lessons, lessonsCount, request.Page, request.PageSize, totalPages);
        }
    }
}
