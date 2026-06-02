namespace InventoryApp.Application.Models.Items;

/// <summary>
/// Строка изображения предмета для экрана деталей
/// </summary>
/// <param name="Id">Идентификатор записи изображения</param>
/// <param name="ImageUrl">URL или путь к файлу изображения</param>
/// <param name="IsPrimary">Признак основного изображения</param>
public sealed record ItemImageRowDto(int Id, string ImageUrl, bool IsPrimary)
{
    #region Свойства

    /// <summary>
    /// Путь к файлу для привязки в UI (синоним <see cref="ImageUrl"/>)
    /// </summary>
    public string FilePath => ImageUrl;

    #endregion Свойства
}
