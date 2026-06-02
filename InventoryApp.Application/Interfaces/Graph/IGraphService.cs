using InventoryApp.Application.Models.Graph;

namespace InventoryApp.Application.Services.Graph;

/// <summary>
/// Построение графа инвентаря для визуализации
/// </summary>
public interface IGraphService
{
    /// <summary>
    /// Строит граф инвентаря с опциональным поиском по подписи
    /// </summary>
    /// <param name="userId">Идентификатор пользователя или null</param>
    /// <param name="search">Строка поиска или null</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task<GraphView> BuildInventoryGraphAsync(
        int? userId = null,
        string? search = null,
        CancellationToken cancellationToken = default);
}
