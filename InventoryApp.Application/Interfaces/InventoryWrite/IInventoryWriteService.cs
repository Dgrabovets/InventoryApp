using InventoryApp.Application.Models.InventoryWrite;

namespace InventoryApp.Application.Services.InventoryWrite;

/// <summary>
/// Запись и изменение сущностей инвентаря
/// </summary>
public interface IInventoryWriteService
{
    /// <summary>
    /// Создаёт здание
    /// </summary>
    Task<int> CreateBuildingAsync(string name, string? description, CancellationToken cancellationToken = default);

    /// <summary>
    /// Создаёт этаж
    /// </summary>
    Task<int> CreateFloorAsync(
        int buildingId,
        int level,
        string name,
        string? description,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Создаёт комнату
    /// </summary>
    Task<int> CreateRoomAsync(int floorId, string name, string? description, CancellationToken cancellationToken = default);

    /// <summary>
    /// Создаёт контейнер в комнате
    /// </summary>
    Task<int> CreateContainerInRoomAsync(
        int roomId,
        string name,
        string? description,
        int? containerTypeId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Создаёт контейнер на этаже
    /// </summary>
    Task<int> CreateContainerOnFloorAsync(
        int floorId,
        string name,
        string? description,
        int? containerTypeId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Создаёт секцию в контейнере
    /// </summary>
    Task<int> CreateSectionAsync(int containerId, string name, string? description, CancellationToken cancellationToken = default);

    /// <summary>
    /// Создаёт категорию
    /// </summary>
    Task<int> CreateCategoryAsync(int? parentCategoryId, string name, string? description, CancellationToken cancellationToken = default);

    /// <summary>
    /// Создаёт предмет
    /// </summary>
    Task<int> CreateItemAsync(
        string name,
        string? description,
        int quantity,
        ItemLocationSnapshot location,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает текущее место предмета
    /// </summary>
    Task<ItemLocationSnapshot> GetItemLocationSnapshotAsync(int itemId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Строит снимок места для комнаты и категории
    /// </summary>
    Task<ItemLocationSnapshot> BuildLocationForRoomAsync(int roomId, int? categoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Строит снимок места для контейнера и категории
    /// </summary>
    Task<ItemLocationSnapshot> BuildLocationForContainerAsync(int containerId, int? categoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Строит снимок места для секции и категории
    /// </summary>
    Task<ItemLocationSnapshot> BuildLocationForSectionAsync(int sectionId, int? categoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Снимок только с категорией (без привязки к месту)
    /// </summary>
    ItemLocationSnapshot CategoryOnly(int categoryId);

    /// <summary>
    /// Обновляет предмет и при необходимости перемещает
    /// </summary>
    Task UpdateItemAsync(
        int itemId,
        string name,
        string? description,
        int quantity,
        int? categoryId,
        string? locationNotes,
        int? moveToRoomId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Мягко удаляет предмет
    /// </summary>
    Task SoftDeleteItemAsync(int itemId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавляет изображение предмета из файла
    /// </summary>
    Task<int> AddItemImageFromFileAsync(
        int itemId,
        string sourceFileFullPath,
        string appDataRootDirectory,
        bool setAsPrimary = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Удаляет изображение предмета
    /// </summary>
    Task RemoveItemImageAsync(int imageId, int itemId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Назначает основное изображение предмета
    /// </summary>
    Task SetPrimaryItemImageAsync(int imageId, int itemId, CancellationToken cancellationToken = default);
}
