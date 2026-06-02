namespace InventoryApp.Application.Models.Graph;

/// <summary>
/// Связь между узлами графа
/// </summary>
public sealed class GraphLink
{
    #region Свойства

    /// <summary>
    /// Идентификатор исходного узла
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Идентификатор целевого узла
    /// </summary>
    public string Target { get; set; } = string.Empty;

    /// <summary>
    /// Подпись связи
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Тип связи (например hierarchy)
    /// </summary>
    public string Type { get; set; } = "default";

    #endregion Свойства
}
