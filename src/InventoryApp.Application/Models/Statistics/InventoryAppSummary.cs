namespace InventoryApp.Application.Models.Statistics;

/// <summary>
/// Агрегированные счётчики для главной страницы
/// </summary>
public sealed class InventoryAppSummary
{
    #region Свойства

    /// <summary>
    /// Число активных предметов
    /// </summary>
    public int ItemsCount { get; init; }

    /// <summary>
    /// Число комнат
    /// </summary>
    public int RoomsCount { get; init; }

    /// <summary>
    /// Число категорий
    /// </summary>
    public int CategoriesCount { get; init; }

    /// <summary>
    /// Число пользователей
    /// </summary>
    public int UsersCount { get; init; }

    /// <summary>
    /// Предметы со сроком от сегодня до +30 дней
    /// </summary>
    public int ItemsExpiringWithin30Days { get; init; }

    /// <summary>
    /// Предметы с просроченным сроком
    /// </summary>
    public int ItemsExpired { get; init; }

    #endregion Свойства
}
