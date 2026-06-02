namespace InventoryApp.Application.Models.Items;

/// <summary>
/// Фильтр списка предметов по дате истечения срока (только у позиций с заполненным сроком)
/// </summary>
public enum ItemExpiryListFilter
{
    /// <summary>
    /// Все активные предметы
    /// </summary>
    All,

    /// <summary>
    /// Срок от сегодня до сегодня+30 дней включительно
    /// </summary>
    ExpiringWithin30Days,

    /// <summary>
    /// Срок раньше сегодняшнего дня
    /// </summary>
    Expired,
}
