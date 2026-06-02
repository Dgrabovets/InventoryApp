using InventoryApp.Application.Models.Items;

namespace InventoryApp.Application.Services.Items;

/// <summary>
/// Чтение каталога предметов для списка и карточки
/// </summary>
public interface IInventoryItemCatalogService
{
    /// <summary>
    /// Возвращает строки списка предметов с поиском и фильтром по сроку
    /// </summary>
    /// <param name="search">Строка поиска или null</param>
    /// <param name="expiryFilter">Фильтр по сроку годности</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task<IReadOnlyList<ItemListRowDto>> ListItemsAsync(
        string? search = null,
        ItemExpiryListFilter expiryFilter = ItemExpiryListFilter.All,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает детальную карточку предмета или null
    /// </summary>
    /// <param name="itemId">Идентификатор предмета</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task<ItemDetailDto?> GetItemDetailAsync(int itemId, CancellationToken cancellationToken = default);
}
