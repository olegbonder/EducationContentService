using FileService.Domain.MediaProcessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileService.Infrastructure.Postgres.Configurations
{
    public class VideoProcessingConfiguration : IEntityTypeConfiguration<VideoProcess>
    {
        public void Configure(EntityTypeBuilder<VideoProcess> builder)
        {
            builder.ToTable("video_processing");
            builder.HasKey(v => v.Id);
            
            builder.Property(v => v.Id).HasColumnName("id");
            builder.Property(v => v.VideoAssetId).HasColumnName("video_asset_id");            
            builder.Property(v => v.Status).HasConversion<string>().HasColumnName("status");
            builder.Property(v => v.ProgressPercentage).HasColumnName("progress_percentage");
            builder.Property(v => v.ErrorMessage).HasColumnName("error_message");
            builder.Property(v => v.StartedAt).HasColumnName("started_at");
            builder.Property(v => v.CompletedAt).HasColumnName("completed_at");
            
            builder.OwnsOne(v => v.MetaData, mdb =>
            {
                mdb.ToJson("meta_data");
                mdb.Property(md => md.Duration).HasColumnName("duration").IsRequired();
                mdb.Property(md => md.Width).HasColumnName("width").IsRequired();
                mdb.Property(md => md.Height).HasColumnName("height").IsRequired();
            });

            builder.OwnsMany(vp => vp.Steps, sb =>
            {
                sb.ToTable("processing_steps");
                sb.HasKey(s => s.Id);

                sb.Property(s => s.Id).HasColumnName("id");
                sb.Property(s => s.StepType).HasConversion<string>().HasColumnName("step_type");
                sb.Property(s => s.Order).HasColumnName("order");
                sb.Property(s => s.Status).HasConversion<string>().HasColumnName("status");
                sb.Property(s => s.ResultData).HasColumnName("result_data").HasColumnType("jsonb");
                sb.Property(s => s.ErrorMessage).HasColumnName("error_message");
                sb.Property(s => s.StartedAt).HasColumnName("started_at");
                sb.Property(s => s.CompletedAt).HasColumnName("completed_at");

                sb.WithOwner().HasForeignKey("VideoProcessingId");
                sb.Property<Guid>("VideoProcessingId").HasColumnName("video_processing_id");

                sb.HasIndex(s => new {s.StepType}).HasDatabaseName("ix_processing_steps_step_type");
                sb.HasIndex(s => new {s.Status}).HasDatabaseName("ix_processing_steps_status");
            });
            
            builder.HasIndex(v => new {v.Status}).HasDatabaseName("ix_video_processing_status");
            builder.HasIndex(v => new {v.Status, v.StartedAt}).HasDatabaseName("ix_video_processing_status_started_at");
        }
    }
}