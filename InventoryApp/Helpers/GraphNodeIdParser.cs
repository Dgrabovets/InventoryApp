using InventoryApp.Application.Models.Graph;

namespace InventoryApp.Helpers;

/// <summary>
/// Разбор идентификатора узла вида type_id
/// </summary>
internal static class GraphNodeIdParser
{
    #region Публичные методы

    /// <summary>
    /// Извлекает тип сущности и числовой id из <see cref="GraphNode.Id"/>
    /// </summary>
    /// <param name="node">Узел графа</param>
    /// <param name="type">Тип сущности (building, room, item и т.д.)</param>
    /// <param name="entityId">Числовой идентификатор в БД</param>
    /// <returns>true, если формат id корректен</returns>
    public static bool TryParse(GraphNode node, out string type, out int entityId)
    {
        type = "";
        entityId = 0;
        var id = node.Id;
        if (string.IsNullOrEmpty(id))
        {
            return false;
        }

        var u = id.IndexOf('_');
        if (u <= 0 || u >= id.Length - 1)
        {
            return false;
        }

        type = id[..u];
        return int.TryParse(id[(u + 1)..], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out entityId);
    }

    #endregion Публичные методы
}
