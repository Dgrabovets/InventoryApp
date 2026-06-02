namespace InventoryApp.Application.Models.Export;

/// <summary>
/// Область экспорта инвентаря (полная выгрузка или один контейнер)
/// </summary>
public sealed class ExportScope
{
    #region Конструкторы

    private ExportScope(bool isFullInventory, int? containerId)
    {
        IsFullInventory = isFullInventory;
        ContainerId = containerId;
    }

    #endregion Конструкторы

    #region Свойства

    /// <summary>
    /// Признак полной выгрузки всех активных предметов
    /// </summary>
    public bool IsFullInventory { get; }

    /// <summary>
    /// Идентификатор контейнера при выборочной выгрузке
    /// </summary>
    public int? ContainerId { get; }

    #endregion Свойства

    #region Методы

    /// <summary>
    /// Полная выгрузка инвентаря
    /// </summary>
    public static ExportScope FullInventory { get; } = new(true, null);

    /// <summary>
    /// Выгрузка предметов в указанном контейнере
    /// </summary>
    /// <param name="containerId">Идентификатор контейнера</param>
    public static ExportScope Container(int containerId) => new(false, containerId);

    #endregion Методы
}
