namespace EducationContentService.Domain.ModuleItems
{
    public sealed class ModuleItem
    {
        public ModuleItem(Guid? id, Guid moduleId)
        {
            Id = id ?? Guid.NewGuid();
            ModuleId = moduleId;
        }

        private ModuleItem()
        {
        }

        public Guid Id { get; private set; }

        public Guid ModuleId { get; private set; }
    }
}
