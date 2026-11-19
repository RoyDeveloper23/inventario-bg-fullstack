using InventarioBg.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventarioBg.Infrastructure.Data.Configurations;

public sealed class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> builder)
    {
        builder.ToTable("productos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.CodigoSku).HasColumnName("codigo_sku").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(1000);
        builder.Property(x => x.Categoria).HasColumnName("categoria").HasMaxLength(100);
        builder.Property(x => x.Activo).HasColumnName("activo").IsRequired();
        builder.HasIndex(x => x.CodigoSku).IsUnique();
        builder.HasIndex(x => x.Nombre);
    }
}
