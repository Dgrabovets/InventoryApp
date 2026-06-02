namespace InventoryApp.Application.Models.Export;

/// <summary>
/// Строка экспорта инвентаря в CSV или PDF
/// </summary>
/// <param name="Name">Название предмета</param>
/// <param name="Quantity">Количество</param>
/// <param name="CategoryName">Категория</param>
/// <param name="LocationSummary">Место</param>
/// <param name="Description">Описание</param>
/// <param name="LocationNotes">Заметки о месте</param>
/// <param name="Price">Цена (текст)</param>
/// <param name="ExpiryDate">Срок годности (текст)</param>
public sealed record InventoryExportRowDto(
    string Name,
    int Quantity,
    string CategoryName,
    string LocationSummary,
    string Description,
    string LocationNotes,
    string Price,
    string ExpiryDate)
{
    #region Свойства

    /// <summary>
    /// Категория для экспорта (синоним <see cref="CategoryName"/>)
    /// </summary>
    public string Category => CategoryName;

    /// <summary>
    /// Место для экспорта (синоним <see cref="LocationSummary"/>)
    /// </summary>
    public string Location => LocationSummary;

    #endregion Свойства
}
