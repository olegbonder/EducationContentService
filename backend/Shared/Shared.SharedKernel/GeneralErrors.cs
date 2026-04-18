namespace Shared.SharedKernel
{
    public static class GeneralErrors
    {
        public static Error ValueIsInvalid(string? name = null)
        {
            string label = name ?? "значение";
            return Error.Validation("value.is.invalid", $"{label} недействительно");
        }

        public static Error NotFound(Guid? id = null, string? name = null)
        {
            string forId = id == null ? string.Empty : $" по Id '{id}'";
            return Error.NotFound("record.not.found", $"{name ?? "запись"} не найдена{forId}");
        }

        public static Error ValueIsRequired(string? name = null)
        {
            string label = name == null ? string.Empty : " " + name + " ";
            return Error.Validation("length.is.invalid", $"Поле{label}обязательно");
        }

        public static Error AlreadyExist()
        {
            return Error.Validation("record.already.exist", "Запись уже существует");
        }

        public static Error Failure(string? message = null)
        {
            return Error.Failure("server.failure", message ?? "Серверная ошибка");
        }

        public static Error ConcurrencyConflict()
        {
            return Error.Conflict("db.concurrency.conflict", "Конфликт параллельного обновления данных");
        }

        public static Error OperationCancelled()
        {
            return Error.Failure("operation.cancelled", "Операция была отменена");
        }

        public static Error DatabaseError(string? message = null)
        {
            return Error.Failure("db.error", message ?? "Ошибка при работе с базой данных");
        }

    }
}