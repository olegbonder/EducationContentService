using CSharpFunctionalExtensions;
using EducationContentService.Domain.Shared;

namespace EducationContentService.Domain.ModuleItems
{
    public record Position
    {
        private const int INITIAL_STEP = 5;

        public Position(ItemType itemType, decimal value)
        {
        }
        
        public decimal Value { get; }

        public ItemType ItemType { get; }

        public static Position First(ItemType itemType) => new(itemType, INITIAL_STEP);

        public static Result<Position, Error> Between(Position before, Position after)
        {
            if (before.ItemType == after.ItemType)
            {
                return Error.Validation(new ErrorMessage("position.item.type", "Типы элементов не совпадают", "position"));
            }

            if (before.Value >= after.Value)
            {
                return Error.Validation(new ErrorMessage("position.value", "Позиция до больше чем позиция после", "position"));
            }

            return new Position(before.ItemType, (before.Value + after.Value) / 2);
        }

        public static Position After(Position previous) =>
            new(previous.ItemType, previous.Value + INITIAL_STEP);
    }
}
