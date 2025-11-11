using CSharpFunctionalExtensions;

namespace EducationContentService.Domain.Shared;

public static class EducationErrors
{
    public static Error TitleConflict(string title) =>
        Error.Conflict(new ErrorMessage("lesson.title.conflict", $"Урок с заголовком {title} уже существует", null));

    public static Error DataBaseError() =>
        Error.Failure(new ErrorMessage("lesson.database.error", "Ошибка базы данных при работе с уроком", null));
    
    public static Error OperationCancelled() => 
        Error.Failure(new ErrorMessage("education.operation.cancelled", "Операция была отменена", null));
}