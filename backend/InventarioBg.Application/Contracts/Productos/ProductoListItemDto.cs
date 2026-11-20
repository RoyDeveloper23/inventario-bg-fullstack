namespace InventarioBg.Application.Contracts.Productos;

public sealed record ProductoListItemDto(
    Guid Id,
    string CodigoSku,
    string Nombre,
    string? Descripcion,
    string? Categoria,
    bool Activo,
    long StockTotal,
    IReadOnlyList<string> Proveedores);
