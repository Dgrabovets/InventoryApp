namespace InventoryApp.Application.Models.Statistics;

/// <summary>
/// Статистика локального пользователя для экрана профиля
/// </summary>
public sealed class LocalUserProfileStats
{
    #region Свойства

    /// <summary>
    /// Имя пользователя
    /// </summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// Название роли
    /// </summary>
    public string RoleName { get; init; } = "—";

    /// <summary>
    /// Добавлено предметов (перемещения с пустым источником)
    /// </summary>
    public int ItemsAdded { get; init; }

    /// <summary>
    /// Удалено предметов
    /// </summary>
    public int ItemsDeleted { get; init; }

    /// <summary>
    /// Перемещений между местами
    /// </summary>
    public int ItemMovements { get; init; }

    /// <summary>
    /// Редактирований по журналу аудита
    /// </summary>
    public int ItemsEdited { get; init; }

    /// <summary>
    /// Всего действий (перемещения + аудит)
    /// </summary>
    public int TotalActions { get; init; }

    /// <summary>
    /// Последняя активность или null
    /// </summary>
    public DateTime? LastActivity { get; init; }

    /// <summary>
    /// Создано предметов
    /// </summary>
    public int CreatedItems { get; init; }

    /// <summary>
    /// Создано зданий
    /// </summary>
    public int CreatedBuildings { get; init; }

    /// <summary>
    /// Создано комнат
    /// </summary>
    public int CreatedRooms { get; init; }

    /// <summary>
    /// Создано контейнеров
    /// </summary>
    public int CreatedContainers { get; init; }

    /// <summary>
    /// Создано категорий
    /// </summary>
    public int CreatedCategories { get; init; }

    #endregion Свойства
}
