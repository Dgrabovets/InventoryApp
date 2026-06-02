using InventoryApp.Domain.Entities;

namespace InventoryApp.Infrastructure.Services.Statistics;

/// <summary>
/// Классификация записей <see cref="ItemMovement"/> для статистики профиля
/// </summary>
internal static class ItemMovementKind
{
    #region Публичные методы

    /// <summary>
    /// Определяет, что у перемещения не задан исходный адрес — трактуется как добавление предмета
    /// </summary>
    /// <param name="movement">Запись перемещения</param>
    /// <returns><see langword="true"/>, если все поля «откуда» пусты</returns>
    public static bool IsAddition(ItemMovement movement) =>
        movement.FromBuildingId == null && movement.FromFloorId == null &&
        movement.FromRoomId == null && movement.FromContainerId == null && movement.FromSectionId == null;

    /// <summary>
    /// Определяет, что у перемещения не задан адрес назначения — трактуется как удаление предмета
    /// </summary>
    /// <param name="movement">Запись перемещения</param>
    /// <returns><see langword="true"/>, если все поля «куда» пусты</returns>
    public static bool IsDeletion(ItemMovement movement) =>
        movement.ToBuildingId == null && movement.ToFloorId == null &&
        movement.ToRoomId == null && movement.ToContainerId == null && movement.ToSectionId == null;

    /// <summary>
    /// Обычное перемещение между локациями (не добавление и не удаление)
    /// </summary>
    /// <param name="movement">Запись перемещения</param>
    /// <returns><see langword="true"/>, если заданы и источник, и назначение в терминах локаций</returns>
    public static bool IsRelocation(ItemMovement movement) =>
        !IsAddition(movement) && !IsDeletion(movement);

    #endregion Публичные методы
}
