namespace InventarioBg.Application.Contracts.Productos;

public sealed record ProductoDto(
    Guid Id,
    string CodigoSku,
    string Nombre,
    string? Descripcion,
    string? Categoria,
    bool Activo);
