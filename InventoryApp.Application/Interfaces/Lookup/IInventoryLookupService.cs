using InventoryApp.Application.Models.Lookup;

namespace InventoryApp.Application.Services.Lookup;

/// <summary>
/// Справочники для форм создания и редактирования
/// </summary>
public interface IInventoryLookupService
{
    /// <summary>
    /// Активные категории
    /// </summary>
    /// <param name="cancellationToken">Токен отмены</param>
    Task<IReadOnlyList<ListItemDto>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Комнаты с подписью «здание · этаж · комната»
    /// </summary>
    /// <param name="cancellationToken">Токен отмены</param>
    Task<IReadOnlyList<ListItemDto>> GetRoomsDisplayListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Типы контейнеров
    /// </summary>
    /// <param name="cancellationToken">Токен отмены</param>
    Task<IReadOnlyList<ListItemDto>> GetContainerTypesAsync(CancellationToken cancellationToken = default);
}
