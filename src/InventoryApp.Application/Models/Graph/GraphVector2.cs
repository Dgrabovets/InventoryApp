namespace InventoryApp.Application.Models.Graph;

/// <summary>
/// Двумерный вектор позиции узла на плоскости графа
/// </summary>
public struct GraphVector2
{
    #region Свойства

    /// <summary>
    /// Координата X
    /// </summary>
    public float X { get; set; }

    /// <summary>
    /// Координата Y
    /// </summary>
    public float Y { get; set; }

    #endregion Свойства

    #region Конструкторы

    /// <summary>
    /// Инициализирует вектор с заданными компонентами
    /// </summary>
    /// <param name="x">Компонента X</param>
    /// <param name="y">Компонента Y</param>
    public GraphVector2(float x, float y)
    {
        X = x;
        Y = y;
    }

    #endregion Конструкторы
}
