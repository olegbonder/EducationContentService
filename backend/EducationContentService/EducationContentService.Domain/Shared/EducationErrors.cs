using CSharpFunctionalExtensions;

namespace EducationContentService.Domain.Shared;

public static class EducationErrors
{
    public static Error TitleConflict(string title) =>
        Error.Conflict("lesson.title.conflict", $"Урок с заголовком {title} уже существует");

    public static Error DataBaseError() =>
        Error.Failure("lesson.database.error", "Ошибка базы данных при работе с уроком");
    
    public static Error OperationCancelled() => 
        Error.Failure("education.operation.cancelled", "Операция была отменена");
}