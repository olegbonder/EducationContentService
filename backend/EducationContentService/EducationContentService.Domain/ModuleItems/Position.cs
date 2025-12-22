using CSharpFunctionalExtensions;
using Shared.SharedKernel;

namespace EducationContentService.Domain.ModuleItems
{
    public record Position
    {
        private const decimal INITIAL_STEP = 1000m;

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
                return Error.Validation("position.item.type", "Типы элементов не совпадают");
            }

            if (before.Value >= after.Value)
            {
                return Error.Validation("position.value", "Позиция до больше чем позиция после");
            }

            return new Position(before.ItemType, (before.Value + after.Value) / 2);
        }

        public static Position After(Position previous) =>
            new(previous.ItemType, previous.Value + INITIAL_STEP);
    }
}
