namespace InventoryApp.Application.Models.Export;

/// <summary>
/// Элемент списка контейнеров для выбора области экспорта
/// </summary>
/// <param name="Id">Идентификатор контейнера</param>
/// <param name="Label">Подпись для отображения</param>
public sealed record ExportContainerListItemDto(int Id, string Label);
