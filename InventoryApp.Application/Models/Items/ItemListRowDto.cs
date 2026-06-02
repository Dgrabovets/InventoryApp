namespace InventoryApp.Application.Models.Items;

/// <summary>
/// Строка списка предметов для экрана каталога
/// </summary>
/// <param name="Id">Идентификатор предмета</param>
/// <param name="Name">Название</param>
/// <param name="Quantity">Количество</param>
/// <param name="CategoryName">Название категории или null</param>
/// <param name="LocationSummary">Краткое описание места</param>
/// <param name="ListThumbnailFilePath">Путь к миниатюре для списка или null</param>
public sealed record ItemListRowDto(int Id,
                                    string Name,
                                    int Quantity,
                                    string? CategoryName,
                                    string LocationSummary,
                                    string? ListThumbnailFilePath)
{
    #region Свойства

    /// <summary>
    /// Вторая строка списка: категория и место
    /// </summary>
    public string SubtitleLine =>
        string.IsNullOrEmpty(CategoryName) ? LocationSummary : $"{CategoryName} · {LocationSummary}";

    #endregion Свойства
}
