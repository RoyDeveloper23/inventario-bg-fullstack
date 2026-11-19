namespace InventarioBg.Application.Contracts.Inventario;

public sealed record ProductoInventarioDto(
    Guid Id,
    Guid ProductoId,
    Guid ProveedorId,
    string ProveedorNombre,
    string NumeroLote,
    decimal PrecioUnitario,
    int StockDisponible,
    DateTime FechaIngreso);
