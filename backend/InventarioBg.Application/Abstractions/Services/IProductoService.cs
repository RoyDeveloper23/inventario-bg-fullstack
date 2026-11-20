using InventarioBg.Application.Contracts.Common;
using InventarioBg.Application.Contracts.Productos;

namespace InventarioBg.Application.Abstractions.Services;

public interface IProductoService
{
    Task<PagedResultDto<ProductoListItemDto>> ListarAsync(
        string? busqueda,
        Guid? proveedorId,
        string? categoria,
        bool? activo,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<ProductoDto> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductoDto> CrearAsync(ProductoRequestDto solicitud, CancellationToken cancellationToken = default);
    Task<ProductoDto> ActualizarAsync(Guid id, ProductoRequestDto solicitud, CancellationToken cancellationToken = default);
    Task DesactivarAsync(Guid id, CancellationToken cancellationToken = default);
    Task EliminarPermanenteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductoDto> ReactivarAsync(Guid id, CancellationToken cancellationToken = default);
}
