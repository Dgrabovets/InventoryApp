using InventoryApp.Application.Models.Items;
using InventoryApp.Application.Services.Items;
using InventoryApp.Domain.Entities;
using InventoryApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Infrastructure.Services.Items;

/// <inheritdoc cref="IInventoryItemCatalogService" />
public sealed class InventoryItemCatalogService : IInventoryItemCatalogService
{
    #region Поля

    /// <summary>
    /// Контекст базы данных
    /// </summary>
    private readonly ApplicationContext _context;

    #endregion Поля

    #region Конструкторы

    /// <summary>
    /// Создаёт сервис каталога предметов
    /// </summary>
    /// <param name="context">Контекст базы данных</param>
    public InventoryItemCatalogService(ApplicationContext context)
    {
        _context = context;
    }

    #endregion Конструкторы

    #region Публичные методы

    /// <inheritdoc />
    public async Task<IReadOnlyList<ItemListRowDto>> ListItemsAsync(
        string? search = null,
        ItemExpiryListFilter expiryFilter = ItemExpiryListFilter.All,
        CancellationToken cancellationToken = default)
    {
        var q = _context.Items.AsNoTracking()
            .Include(i => i.Category)
            .Include(i => i.Building)
            .Include(i => i.Floor)
            .Include(i => i.Room)
            .Include(i => i.Container)
            .Include(i => i.Section)
            .Include(i => i.ItemImages)
            .Where(i => i.IsActive);

        if (expiryFilter != ItemExpiryListFilter.All)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            if (expiryFilter == ItemExpiryListFilter.ExpiringWithin30Days)
            {
                var in30 = today.AddDays(30);
                q = q.Where(i => i.ExpiryDate != null && i.ExpiryDate >= today && i.ExpiryDate <= in30);
            }
            else if (expiryFilter == ItemExpiryListFilter.Expired)
            {
                q = q.Where(i => i.ExpiryDate != null && i.ExpiryDate < today);
            }
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            q = q.Where(i =>
                i.Name.ToLower().Contains(term) ||
                (i.Description != null && i.Description.ToLower().Contains(term)) ||
                (i.LocationNotes != null && i.LocationNotes.ToLower().Contains(term)));
        }

        var list = await q
            .OrderBy(i => i.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return list.Select(i => new ItemListRowDto(
            i.Id,
            i.Name,
            i.Quantity,
            i.Category?.Name,
            BuildLocationSummary(i),
            i.ItemImages
                .OrderByDescending(im => im.IsPrimary)
                .ThenBy(im => im.Id)
                .Select(im => im.ImageUrl)
                .FirstOrDefault())).ToList();
    }

    /// <inheritdoc />
    public async Task<ItemDetailDto?> GetItemDetailAsync(int itemId, CancellationToken cancellationToken = default)
    {
        var i = await _context.Items.AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Building)
            .Include(x => x.Floor)
            .Include(x => x.Room)
            .Include(x => x.Container)
            .Include(x => x.Section)
            .Include(x => x.ItemImages)
            .FirstOrDefaultAsync(x => x.Id == itemId && x.IsActive, cancellationToken)
            .ConfigureAwait(false);

        if (i is null)
        {
            return null;
        }

        var images = i.ItemImages
            .OrderByDescending(im => im.IsPrimary)
            .ThenBy(im => im.Id)
            .Select(im => new ItemImageRowDto(im.Id, im.ImageUrl, im.IsPrimary))
            .ToList();

        return new ItemDetailDto(
            i.Id,
            i.Name,
            i.Description,
            i.Quantity,
            i.CategoryId,
            i.RoomId,
            BuildLocationSummary(i),
            i.LocationNotes,
            images);
    }

    #endregion Публичные методы

    #region Приватные методы

    /// <summary>
    /// Собирает краткое описание места предмета
    /// </summary>
    /// <param name="i">Сущность предмета с подгруженной иерархией</param>
    private static string BuildLocationSummary(Item i)
    {
        var parts = new List<string>();
        if (i.Building != null)
        {
            parts.Add(i.Building.Name);
        }

        if (i.Floor != null)
        {
            parts.Add($"этаж {i.Floor.Level}");
        }

        if (i.Room != null)
        {
            parts.Add(i.Room.Name);
        }

        if (i.Container != null)
        {
            parts.Add(i.Container.Name);
        }

        if (i.Section != null)
        {
            parts.Add(i.Section.Name);
        }

        return parts.Count == 0 ? "—" : string.Join(" · ", parts);
    }

    #endregion Приватные методы
}
