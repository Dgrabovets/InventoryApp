using InventoryApp.Application.Models.Movements;

namespace InventoryApp.Application.Services.Movements;

/// <summary>
/// Журнал перемещений предметов
/// </summary>
public interface IItemMovementHistoryService
{
    /// <summary>
    /// Возвращает последние перемещения с опциональным поиском
    /// </summary>
    /// <param name="take">Максимум записей (1–2000)</param>
    /// <param name="search">Поиск по названию предмета или пользователю</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task<IReadOnlyList<ItemMovementRowDto>> ListRecentAsync(
        int take = 300,
        string? search = null,
        CancellationToken cancellationToken = default);
}
