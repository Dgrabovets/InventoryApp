using InventoryApp.Application.Models.Lookup;
using InventoryApp.Application.Services.Lookup;
using InventoryApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Infrastructure.Services.Lookup;

/// <inheritdoc cref="IInventoryLookupService" />
public sealed class InventoryLookupService : IInventoryLookupService
{
    #region Поля

    /// <summary>
    /// Контекст базы данных
    /// </summary>
    private readonly ApplicationContext _context;

    #endregion Поля

    #region Конструкторы

    /// <summary>
    /// Создаёт сервис справочников
    /// </summary>
    /// <param name="context">Контекст базы данных</param>
    public InventoryLookupService(ApplicationContext context)
    {
        _context = context;
    }

    #endregion Конструкторы

    #region Публичные методы

    /// <inheritdoc />
    public async Task<IReadOnlyList<ListItemDto>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Categories.AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new ListItemDto(c.Id, c.Name))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ListItemDto>> GetRoomsDisplayListAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Rooms.AsNoTracking()
            .Where(r => r.IsActive && r.Floor != null && r.Floor.Building != null)
            .OrderBy(r => r.Floor!.Building!.Name)
            .ThenBy(r => r.Floor!.Level)
            .ThenBy(r => r.Name)
            .Select(r => new ListItemDto(
                r.Id,
                r.Floor!.Building!.Name + " · этаж " + r.Floor.Level + " · " + r.Name))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ListItemDto>> GetContainerTypesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ContainerTypes.AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .Select(t => new ListItemDto(t.Id, t.Name))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    #endregion Публичные методы
}
