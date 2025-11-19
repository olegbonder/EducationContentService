using EducationContentService.Domain.Lesson;
using EducationContentService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EducationContentService.Infrastructure.Postgres.Configurations
{
    public static class Index
    {
        public const string TITLE = "ix_lessons_title";
    }
    public class LessonConfiguration : IEntityTypeConfiguration<Lesson>
    {
        public void Configure(EntityTypeBuilder<Lesson> builder)
        {
            builder.ToTable("lessons");

            builder.HasKey(l => l.Id);

            builder.Property(l => l.Id)
                .HasColumnName("id");

            builder.OwnsOne(l => l.Title, lb =>
            {
                lb.Property(l => l.Value)
                    .HasColumnName("title")
                    .IsRequired(true);
                lb.HasIndex(l => l.Value).IsUnique().HasDatabaseName(Index.TITLE);
            });

            builder.Property(l => l.Description)
                .HasConversion(
                    l => l.Value,
                    l => Description.Create(l).Value)
                .HasColumnName("description")
                .IsRequired(true);

            builder.Property(l => l.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");

            builder.Property(l => l.DeleteAt)
                .IsRequired(false)
                .HasColumnName("deletion_date");

            builder.Property(l => l.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("timezone('utc', now())")
                .HasColumnName("created_at");

            builder.Property(l => l.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("timezone('utc', now())")
                .HasColumnName("updated_at");            

            builder.HasQueryFilter(l => !l.IsDeleted);
        }
    }
}
