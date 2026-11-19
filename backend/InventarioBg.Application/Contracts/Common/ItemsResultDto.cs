namespace InventarioBg.Application.Contracts.Common;

public sealed record ItemsResultDto<T>(IReadOnlyList<T> Items);
