namespace InventoryApp.Application.Models.Items;

/// <summary>
/// Детальная карточка предмета для экрана редактирования
/// </summary>
/// <param name="Id">Идентификатор предмета</param>
/// <param name="Name">Название</param>
/// <param name="Description">Описание</param>
/// <param name="Quantity">Количество</param>
/// <param name="CategoryId">Идентификатор категории или null</param>
/// <param name="RoomId">Идентификатор комнаты или null</param>
/// <param name="LocationSummary">Краткое описание места</param>
/// <param name="LocationNotes">Заметки о месте</param>
/// <param name="Images">Список изображений</param>
public sealed record ItemDetailDto(int Id,
                                   string Name,
                                   string? Description,
                                   int Quantity,
                                   int? CategoryId,
                                   int? RoomId,
                                   string LocationSummary,
                                   string? LocationNotes,
                                   IReadOnlyList<ItemImageRowDto> Images);
