using InventoryApp.Application.Models.InventoryWrite;
using InventoryApp.Application.Services.InventoryWrite;
using InventoryApp.Domain.Entities;
using InventoryApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryApp.Infrastructure.Services.InventoryWrite;

/// <inheritdoc cref="IInventoryWriteService" />
public sealed class InventoryWriteService : IInventoryWriteService
{
    #region Поля

    /// <summary>
    /// Контекст базы данных
    /// </summary>
    private readonly ApplicationContext _context;

    /// <summary>
    /// Журнал операций записи
    /// </summary>
    private readonly ILogger<InventoryWriteService> _logger;

    #endregion Поля

    #region Конструкторы

    /// <summary>
    /// Создаёт сервис записи инвентаря
    /// </summary>
    /// <param name="context">Контекст базы данных</param>
    /// <param name="logger">Журнал операций</param>
    public InventoryWriteService(ApplicationContext context, ILogger<InventoryWriteService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #endregion Конструкторы

    #region Публичные методы

    /// <inheritdoc />
    public async Task<int> CreateBuildingAsync(string name, string? description, CancellationToken cancellationToken = default)
    {
        var userId = await GetLocalUserIdAsync(cancellationToken).ConfigureAwait(false);
        var now = DateTime.UtcNow;
        var entity = new Building
        {
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            CreatedById = userId,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        };
        _context.Buildings.Add(entity);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Created building {Id} {Name}", entity.Id, entity.Name);
        return entity.Id;
    }

    /// <inheritdoc />
    public async Task<int> CreateFloorAsync(int buildingId, int level, string name, string? description, CancellationToken cancellationToken = default)
    {
        var userId = await GetLocalUserIdAsync(cancellationToken).ConfigureAwait(false);
        var now = DateTime.UtcNow;
        var entity = new Floor
        {
            BuildingId = buildingId,
            Level = level,
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            CreatedById = userId,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        };
        _context.Floors.Add(entity);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Created floor {Id} in building {BuildingId}", entity.Id, buildingId);
        return entity.Id;
    }

    /// <inheritdoc />
    public async Task<int> CreateRoomAsync(int floorId, string name, string? description, CancellationToken cancellationToken = default)
    {
        var userId = await GetLocalUserIdAsync(cancellationToken).ConfigureAwait(false);
        var now = DateTime.UtcNow;
        var entity = new Room
        {
            FloorId = floorId,
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            CreatedById = userId,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        };
        _context.Rooms.Add(entity);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Created room {Id} on floor {FloorId}", entity.Id, floorId);
        return entity.Id;
    }

    /// <inheritdoc />
    public async Task<int> CreateContainerInRoomAsync(
        int roomId,
        string name,
        string? description,
        int? containerTypeId = null,
        CancellationToken cancellationToken = default)
    {
        var room = await _context.Rooms.AsNoTracking().FirstAsync(r => r.Id == roomId, cancellationToken).ConfigureAwait(false);
        var userId = await GetLocalUserIdAsync(cancellationToken).ConfigureAwait(false);
        var typeId = await ResolveContainerTypeIdAsync(containerTypeId, cancellationToken).ConfigureAwait(false);
        var now = DateTime.UtcNow;
        var entity = new Container
        {
            RoomId = roomId,
            FloorId = room.FloorId,
            ContainerTypeId = typeId,
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            CreatedById = userId,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        };
        _context.Containers.Add(entity);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Created container {Id} in room {RoomId}", entity.Id, roomId);
        return entity.Id;
    }

    /// <inheritdoc />
    public async Task<int> CreateContainerOnFloorAsync(
        int floorId,
        string name,
        string? description,
        int? containerTypeId = null,
        CancellationToken cancellationToken = default)
    {
        var userId = await GetLocalUserIdAsync(cancellationToken).ConfigureAwait(false);
        var typeId = await ResolveContainerTypeIdAsync(containerTypeId, cancellationToken).ConfigureAwait(false);
        var now = DateTime.UtcNow;
        var entity = new Container
        {
            FloorId = floorId,
            RoomId = null,
            ContainerTypeId = typeId,
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            CreatedById = userId,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        };
        _context.Containers.Add(entity);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Created container {Id} on floor {FloorId}", entity.Id, floorId);
        return entity.Id;
    }

    /// <inheritdoc />
    public async Task<int> CreateSectionAsync(int containerId, string name, string? description, CancellationToken cancellationToken = default)
    {
        var userId = await GetLocalUserIdAsync(cancellationToken).ConfigureAwait(false);
        var now = DateTime.UtcNow;
        var entity = new Section
        {
            ContainerId = containerId,
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            CreatedById = userId,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        };
        _context.Sections.Add(entity);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Created section {Id} in container {ContainerId}", entity.Id, containerId);
        return entity.Id;
    }

    /// <inheritdoc />
    public async Task<int> CreateCategoryAsync(int? parentCategoryId, string name, string? description, CancellationToken cancellationToken = default)
    {
        var userId = await GetLocalUserIdAsync(cancellationToken).ConfigureAwait(false);
        var now = DateTime.UtcNow;
        var entity = new Category
        {
            ParentCategoryId = parentCategoryId,
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            CreatedById = userId,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        };
        _context.Categories.Add(entity);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Created category {Id}", entity.Id);
        return entity.Id;
    }

    /// <inheritdoc />
    public async Task<int> CreateItemAsync(
        string name,
        string? description,
        int quantity,
        ItemLocationSnapshot location,
        CancellationToken cancellationToken = default)
    {
        var userId = await GetLocalUserIdAsync(cancellationToken).ConfigureAwait(false);
        var now = DateTime.UtcNow;
        var entity = new Item
        {
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Quantity = quantity < 1 ? 1 : quantity,
            CategoryId = location.CategoryId,
            BuildingId = location.BuildingId,
            FloorId = location.FloorId,
            RoomId = location.RoomId,
            ContainerId = location.ContainerId,
            SectionId = location.SectionId,
            CreatedById = userId,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        };
        _context.Items.Add(entity);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Created item {Id} {Name}", entity.Id, entity.Name);
        return entity.Id;
    }

    /// <inheritdoc />
    public async Task<ItemLocationSnapshot> GetItemLocationSnapshotAsync(int itemId, CancellationToken cancellationToken = default)
    {
        var i = await _context.Items.AsNoTracking().FirstAsync(x => x.Id == itemId, cancellationToken).ConfigureAwait(false);
        return new ItemLocationSnapshot(i.BuildingId, i.FloorId, i.RoomId, i.ContainerId, i.SectionId, i.CategoryId);
    }

    /// <inheritdoc />
    public async Task<ItemLocationSnapshot> BuildLocationForRoomAsync(int roomId, int? categoryId, CancellationToken cancellationToken = default)
    {
        var room = await _context.Rooms
            .Include(r => r.Floor)
            .ThenInclude(f => f!.Building)
            .AsNoTracking()
            .FirstAsync(r => r.Id == roomId, cancellationToken)
            .ConfigureAwait(false);
        var floor = room.Floor;
        var buildingId = floor?.BuildingId;
        return new ItemLocationSnapshot(buildingId, room.FloorId, roomId, null, null, categoryId);
    }

    /// <inheritdoc />
    public async Task<ItemLocationSnapshot> BuildLocationForContainerAsync(int containerId, int? categoryId, CancellationToken cancellationToken = default)
    {
        var c = await _context.Containers
            .Include(x => x.Room)
            .ThenInclude(r => r!.Floor)
            .ThenInclude(f => f!.Building)
            .Include(x => x.Floor)
            .ThenInclude(f => f!.Building)
            .AsNoTracking()
            .FirstAsync(x => x.Id == containerId, cancellationToken)
            .ConfigureAwait(false);

        if (c.RoomId.HasValue && c.Room != null)
        {
            var room = c.Room;
            var floor = room.Floor;
            return new ItemLocationSnapshot(floor?.BuildingId, room.FloorId, room.Id, containerId, null, categoryId);
        }

        if (c.Floor != null)
        {
            return new ItemLocationSnapshot(c.Floor.BuildingId, c.FloorId, null, containerId, null, categoryId);
        }

        return new ItemLocationSnapshot(null, c.FloorId, c.RoomId, containerId, null, categoryId);
    }

    /// <inheritdoc />
    public async Task<ItemLocationSnapshot> BuildLocationForSectionAsync(int sectionId, int? categoryId, CancellationToken cancellationToken = default)
    {
        var s = await _context.Sections
            .Include(x => x.Container)
            .ThenInclude(c => c!.Room)
            .ThenInclude(r => r!.Floor)
            .ThenInclude(f => f!.Building)
            .Include(x => x.Container)
            .ThenInclude(c => c!.Floor)
            .ThenInclude(f => f!.Building)
            .AsNoTracking()
            .FirstAsync(x => x.Id == sectionId, cancellationToken)
            .ConfigureAwait(false);

        var c = s.Container;
        if (c == null)
        {
            return new ItemLocationSnapshot(null, null, null, null, sectionId, categoryId);
        }

        if (c.RoomId.HasValue && c.Room != null)
        {
            var room = c.Room;
            var floor = room.Floor;
            return new ItemLocationSnapshot(floor?.BuildingId, room.FloorId, room.Id, c.Id, sectionId, categoryId);
        }

        if (c.Floor != null)
        {
            return new ItemLocationSnapshot(c.Floor.BuildingId, c.FloorId, null, c.Id, sectionId, categoryId);
        }

        return new ItemLocationSnapshot(null, null, null, c.Id, sectionId, categoryId);
    }

    /// <inheritdoc />
    public ItemLocationSnapshot CategoryOnly(int categoryId) =>
        new(null, null, null, null, null, categoryId);

    /// <inheritdoc />
    public async Task UpdateItemAsync(
        int itemId,
        string name,
        string? description,
        int quantity,
        int? categoryId,
        string? locationNotes,
        int? moveToRoomId,
        CancellationToken cancellationToken = default)
    {
        var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == itemId && i.IsActive, cancellationToken).ConfigureAwait(false);
        if (item is null)
        {
            throw new InvalidOperationException("Предмет не найден или уже удалён.");
        }

        item.Name = name.Trim();
        item.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        item.Quantity = quantity < 1 ? 1 : quantity;
        item.CategoryId = categoryId;
        item.LocationNotes = string.IsNullOrWhiteSpace(locationNotes) ? null : locationNotes.Trim();

        if (moveToRoomId is > 0)
        {
            var fromSnap = new ItemLocationSnapshot(
                item.BuildingId,
                item.FloorId,
                item.RoomId,
                item.ContainerId,
                item.SectionId,
                item.CategoryId);

            var loc = await BuildLocationForRoomAsync(moveToRoomId.Value, item.CategoryId, cancellationToken).ConfigureAwait(false);

            if (LocationFieldsDiffer(fromSnap, loc))
            {
                var userId = await GetLocalUserIdAsync(cancellationToken).ConfigureAwait(false);
                var movedAt = DateTime.UtcNow;
                _context.ItemMovements.Add(new ItemMovement
                {
                    ItemName = item.Name.Trim(),
                    FromBuildingId = fromSnap.BuildingId,
                    FromFloorId = fromSnap.FloorId,
                    FromRoomId = fromSnap.RoomId,
                    FromContainerId = fromSnap.ContainerId,
                    FromSectionId = fromSnap.SectionId,
                    ToBuildingId = loc.BuildingId,
                    ToFloorId = loc.FloorId,
                    ToRoomId = loc.RoomId,
                    ToContainerId = loc.ContainerId,
                    ToSectionId = loc.SectionId,
                    MovedById = userId,
                    MovedAt = movedAt,
                    CreatedAt = movedAt,
                    UpdatedAt = movedAt,
                });
                _logger.LogInformation("Recording item movement for item {ItemId} to room {RoomId}", itemId, loc.RoomId);
            }

            ApplySnapshotToItem(item, loc);
        }

        item.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Updated item {Id}", itemId);
    }

    /// <inheritdoc />
    public async Task SoftDeleteItemAsync(int itemId, CancellationToken cancellationToken = default)
    {
        var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == itemId && i.IsActive, cancellationToken).ConfigureAwait(false);
        if (item is null)
        {
            return;
        }

        var images = await _context.ItemImages.Where(im => im.ItemId == itemId).ToListAsync(cancellationToken).ConfigureAwait(false);
        foreach (var im in images)
        {
            TryDeleteLocalImageFile(im.ImageUrl);
            _context.ItemImages.Remove(im);
        }

        item.IsActive = false;
        item.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Soft-deleted item {Id}", itemId);
    }

    /// <inheritdoc />
    public async Task<int> AddItemImageFromFileAsync(
        int itemId,
        string sourceFileFullPath,
        string appDataRootDirectory,
        bool setAsPrimary = false,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sourceFileFullPath) || !File.Exists(sourceFileFullPath))
        {
            throw new InvalidOperationException("Файл изображения не найден.");
        }

        if (string.IsNullOrWhiteSpace(appDataRootDirectory))
        {
            throw new InvalidOperationException("Не задан каталог данных приложения.");
        }

        var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == itemId && i.IsActive, cancellationToken).ConfigureAwait(false);
        if (item is null)
        {
            throw new InvalidOperationException("Предмет не найден или снят с учёта.");
        }

        var ext = Path.GetExtension(sourceFileFullPath);
        if (string.IsNullOrEmpty(ext))
        {
            ext = ".jpg";
        }

        var destDir = Path.Combine(appDataRootDirectory, "item_media", itemId.ToString(System.Globalization.CultureInfo.InvariantCulture));
        Directory.CreateDirectory(destDir);
        var destPath = Path.Combine(destDir, $"{Guid.NewGuid():N}{ext}");
        File.Copy(sourceFileFullPath, destPath, overwrite: false);

        var hasAny = await _context.ItemImages.AnyAsync(im => im.ItemId == itemId, cancellationToken).ConfigureAwait(false);
        var makePrimary = setAsPrimary || !hasAny;

        if (makePrimary)
        {
            var existing = await _context.ItemImages.Where(im => im.ItemId == itemId).ToListAsync(cancellationToken).ConfigureAwait(false);
            foreach (var e in existing)
            {
                e.IsPrimary = false;
            }
        }

        var userId = await GetLocalUserIdAsync(cancellationToken).ConfigureAwait(false);
        var now = DateTime.UtcNow;
        var entity = new ItemImage
        {
            ItemId = itemId,
            ImageUrl = destPath,
            IsPrimary = makePrimary,
            UploadedById = userId,
            CreatedAt = now,
            UpdatedAt = now,
        };
        _context.ItemImages.Add(entity);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Added item image {ImageId} for item {ItemId}", entity.Id, itemId);
        return entity.Id;
    }

    /// <inheritdoc />
    public async Task RemoveItemImageAsync(int imageId, int itemId, CancellationToken cancellationToken = default)
    {
        var img = await _context.ItemImages.FirstOrDefaultAsync(
                im => im.Id == imageId && im.ItemId == itemId,
                cancellationToken)
            .ConfigureAwait(false);
        if (img is null)
        {
            return;
        }

        var path = img.ImageUrl;
        _context.ItemImages.Remove(img);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        TryDeleteLocalImageFile(path);

        var remaining = await _context.ItemImages.Where(im => im.ItemId == itemId).ToListAsync(cancellationToken).ConfigureAwait(false);
        if (remaining.Count > 0 && remaining.All(im => !im.IsPrimary))
        {
            remaining[0].IsPrimary = true;
            remaining[0].UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        _logger.LogInformation("Removed item image {ImageId}", imageId);
    }

    /// <inheritdoc />
    public async Task SetPrimaryItemImageAsync(int imageId, int itemId, CancellationToken cancellationToken = default)
    {
        var imgs = await _context.ItemImages.Where(im => im.ItemId == itemId).ToListAsync(cancellationToken).ConfigureAwait(false);
        var target = imgs.FirstOrDefault(im => im.Id == imageId);
        if (target is null)
        {
            return;
        }

        var now = DateTime.UtcNow;
        foreach (var im in imgs)
        {
            im.IsPrimary = im.Id == imageId;
            im.UpdatedAt = now;
        }

        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    #endregion Публичные методы

    #region Приватные методы

    /// <summary>
    /// Удаляет локальный файл изображения, если он существует
    /// </summary>
    /// <param name="path">Полный путь к файлу</param>
    private static void TryDeleteLocalImageFile(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // ignored
        }
    }

    /// <summary>
    /// Копирует поля снимка места в сущность предмета
    /// </summary>
    private static void ApplySnapshotToItem(Item item, ItemLocationSnapshot loc)
    {
        item.BuildingId = loc.BuildingId;
        item.FloorId = loc.FloorId;
        item.RoomId = loc.RoomId;
        item.ContainerId = loc.ContainerId;
        item.SectionId = loc.SectionId;
        item.CategoryId = loc.CategoryId;
    }

    /// <summary>
    /// Проверяет, отличаются ли поля места в двух снимках
    /// </summary>
    private static bool LocationFieldsDiffer(ItemLocationSnapshot from, ItemLocationSnapshot to) =>
        from.BuildingId != to.BuildingId
        || from.FloorId != to.FloorId
        || from.RoomId != to.RoomId
        || from.ContainerId != to.ContainerId
        || from.SectionId != to.SectionId;

    /// <summary>
    /// Возвращает идентификатор первого пользователя (локальный режим)
    /// </summary>
    private async Task<int> GetLocalUserIdAsync(CancellationToken cancellationToken)
    {
        return await _context.Users.AsNoTracking()
            .OrderBy(u => u.Id)
            .Select(u => u.Id)
            .FirstAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Определяет идентификатор типа контейнера (явный или первый активный)
    /// </summary>
    private async Task<int> ResolveContainerTypeIdAsync(int? explicitId, CancellationToken cancellationToken)
    {
        if (explicitId is > 0 &&
            await _context.ContainerTypes.AsNoTracking()
                .AnyAsync(t => t.Id == explicitId && t.IsActive, cancellationToken)
                .ConfigureAwait(false))
        {
            return explicitId.Value;
        }

        return await _context.ContainerTypes.AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.Id)
            .Select(t => t.Id)
            .FirstAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    #endregion Приватные методы
}
