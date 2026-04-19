using System.Text.Json;
using FileService.Domain;
using FileService.Domain.Assets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace FileService.Infrastructure.Postgres.Configurations;

public class MediaAssetConfiguration : IEntityTypeConfiguration<MediaAsset>
{
    public void Configure(EntityTypeBuilder<MediaAsset> builder)
    {
        builder.ToTable("media_assets");
        builder.HasKey(m => m.Id);

        builder.OwnsOne(m => m.MediaData, mb =>
        {
            mb.ToJson("media_data");
            mb.OwnsOne(md => md.ContentType, cb =>
            {
                cb.Property(c => c.Category).HasConversion<string>().HasColumnName("category").IsRequired();
                cb.Property(c => c.Value).HasColumnName("value").IsRequired();
            });

            mb.OwnsOne(md => md.FileName, fb =>
            {
                fb.Property(f => f.Name).HasColumnName("name").IsRequired();
                fb.Property(f => f.Extension).HasColumnName("extension").IsRequired();
                fb.Property(f => f.Value).HasColumnName("value").IsRequired();
            });

            mb.Property(md => md.Size).HasColumnName("size").IsRequired();
            mb.Property(md => md.ExpectedChunksCount).HasColumnName("expected_chunks_count");
        });

        builder.OwnsOne(m => m.Owner, ob =>
        {
            ob.ToJson("owner");
            ob.Property(o => o.Context).HasColumnName("context").IsRequired();
            ob.Property(o => o.EntityId).HasColumnName("entity_id").IsRequired();
        });

        builder.Property(m => m.Id).HasColumnName("id");

        builder.Property(m => m.Status).HasConversion<string>().HasColumnName("status").IsRequired();

        builder.Property(m => m.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(m => m.UpdatedAt).HasColumnName("updated_at");

        builder.Property(m => m.Key)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<StorageKey>(v, (JsonSerializerOptions?)null)!)
            .HasColumnName("key")
            .HasColumnType("jsonb");
        builder.Property(m => m.RawKey)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<StorageKey>(v, (JsonSerializerOptions?)null)!)
            .HasColumnName("raw_key")
            .HasColumnType("jsonb");

        builder.Property(m => m.AssetType)
            .HasColumnName("asset_type")
            .HasConversion<string>()
            .IsRequired();

        builder.HasDiscriminator(m => m.AssetType)
            .HasValue<VideoAsset>(AssetType.VIDEO)
            .HasValue<PreviewAsset>(AssetType.PREVIEW);

        builder.HasIndex(m => new { m.Status, m.CreatedAt });

        builder.Property(m => m.UploadId).IsRequired(false).HasColumnName("upload_id");
    }
}