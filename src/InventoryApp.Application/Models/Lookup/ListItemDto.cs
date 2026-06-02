namespace InventoryApp.Application.Models.Lookup;

/// <summary>
/// Элемент выпадающего списка (идентификатор и подпись)
/// </summary>
/// <param name="Id">Идентификатор</param>
/// <param name="Label">Подпись для отображения</param>
public sealed record ListItemDto(int Id, string Label);
