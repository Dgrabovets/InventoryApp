namespace InventoryApp.Application.Models.InventoryWrite;

/// <summary>
/// Снимок привязки предмета к иерархии места и категории
/// </summary>
/// <param name="BuildingId">Идентификатор здания или null</param>
/// <param name="FloorId">Идентификатор этажа или null</param>
/// <param name="RoomId">Идентификатор комнаты или null</param>
/// <param name="ContainerId">Идентификатор контейнера или null</param>
/// <param name="SectionId">Идентификатор секции или null</param>
/// <param name="CategoryId">Идентификатор категории или null</param>
public sealed record ItemLocationSnapshot(int? BuildingId,
                                          int? FloorId,
                                          int? RoomId,
                                          int? ContainerId,
                                          int? SectionId,
                                          int? CategoryId);
