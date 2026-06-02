namespace InventoryApp.Application.Models.Graph;

/// <summary>
/// Полное описание графа для передачи в слой отображения
/// </summary>
public sealed class GraphView
{
    #region Свойства

    /// <summary>
    /// Узлы графа
    /// </summary>
    public List<GraphNode> Nodes { get; set; } = new();

    /// <summary>
    /// Связи между узлами
    /// </summary>
    public List<GraphLink> Links { get; set; } = new();

    #endregion Свойства
}
