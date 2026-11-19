using InventarioBg.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventarioBg.Infrastructure.Data.Configurations;

public sealed class ProductoInventarioConfiguration : IEntityTypeConfiguration<ProductoInventario>
{
    public void Configure(EntityTypeBuilder<ProductoInventario> builder)
    {
        builder.ToTable("productos_inventario");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ProductoId).HasColumnName("producto_id").IsRequired();
        builder.Property(x => x.ProveedorId).HasColumnName("proveedor_id").IsRequired();
        builder.Property(x => x.NumeroLote).HasColumnName("numero_lote").HasMaxLength(100).IsRequired();
        builder.Property(x => x.PrecioUnitario).HasColumnName("precio_unitario").HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.StockDisponible).HasColumnName("stock_disponible").IsRequired();
        builder.Property(x => x.FechaIngreso).HasColumnName("fecha_ingreso").IsRequired();

        builder.HasIndex(x => new { x.ProductoId, x.ProveedorId, x.NumeroLote }).IsUnique();
        builder.HasIndex(x => x.ProductoId);
        builder.HasIndex(x => x.ProveedorId);

        builder.HasOne(x => x.Producto)
            .WithMany(x => x.Inventarios)
            .HasForeignKey(x => x.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Proveedor)
            .WithMany(x => x.Inventarios)
            .HasForeignKey(x => x.ProveedorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(t => t.HasCheckConstraint("CK_productos_inventario_precio", "precio_unitario >= 0"));
        builder.ToTable(t => t.HasCheckConstraint("CK_productos_inventario_stock", "stock_disponible >= 0"));
    }
}
