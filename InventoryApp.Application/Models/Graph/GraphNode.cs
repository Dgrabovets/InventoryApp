namespace InventoryApp.Application.Models.Graph;

/// <summary>
/// Узел графа инвентаря (строение, этаж, комната, контейнер, категория, предмет и т.д.)
/// </summary>
public sealed class GraphNode
{
    #region Свойства

    /// <summary>
    /// Уникальный идентификатор узла в формате «тип_id» (например, room_3)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Подпись узла для отображения пользователю
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Семантический тип узла: building, floor, room, container, section, category, item
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Позиция на холсте (заполняется средствами визуализации)
    /// </summary>
    public GraphVector2 Position { get; set; }

    /// <summary>
    /// Цвет заливки узла в формате HEX
    /// </summary>
    public string Color { get; set; } = "#666666";

    /// <summary>
    /// Произвольная полезная нагрузка для подсказок и детальных форм
    /// </summary>
    public object? Data { get; set; }

    #endregion Свойства
}
