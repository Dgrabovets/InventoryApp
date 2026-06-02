using InventoryApp.Application.Models.Movements;
using InventoryApp.Application.Services.Movements;
using InventoryApp.Domain.Entities;
using InventoryApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Infrastructure.Services.Movements;

/// <inheritdoc cref="IItemMovementHistoryService" />
public sealed class ItemMovementHistoryService : IItemMovementHistoryService
{
    #region Поля

    /// <summary>
    /// Контекст базы данных
    /// </summary>
    private readonly ApplicationContext _context;

    #endregion Поля

    #region Конструкторы

    /// <summary>
    /// Создаёт сервис журнала перемещений
    /// </summary>
    /// <param name="context">Контекст базы данных</param>
    public ItemMovementHistoryService(ApplicationContext context)
    {
        _context = context;
    }

    #endregion Конструкторы

    #region Публичные методы

    /// <inheritdoc />
    public async Task<IReadOnlyList<ItemMovementRowDto>> ListRecentAsync(
        int take = 300,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        if (take < 1)
        {
            take = 1;
        }

        if (take > 2000)
        {
            take = 2000;
        }

        var q = _context.ItemMovements.AsNoTracking()
            .Include(m => m.FromBuilding)
            .Include(m => m.FromFloor)
            .Include(m => m.FromRoom)
            .Include(m => m.FromContainer)
            .Include(m => m.FromSection)
            .Include(m => m.ToBuilding)
            .Include(m => m.ToFloor)
            .Include(m => m.ToRoom)
            .Include(m => m.ToContainer)
            .Include(m => m.ToSection)
            .Include(m => m.MovedBy)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            q = q.Where(m =>
                m.ItemName.ToLower().Contains(term) ||
                m.MovedBy.Username.ToLower().Contains(term));
        }

        var rows = await q
            .OrderByDescending(m => m.MovedAt)
            .ThenByDescending(m => m.Id)
            .Take(take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows.Select(m => new ItemMovementRowDto(
            m.ItemName,
            ToUtc(m.MovedAt),
            BuildEndSummary(m.FromBuilding, m.FromFloor, m.FromRoom, m.FromContainer, m.FromSection),
            BuildEndSummary(m.ToBuilding, m.ToFloor, m.ToRoom, m.ToContainer, m.ToSection),
            m.MovedBy.Username)).ToList();
    }

    #endregion Публичные методы

    #region Приватные методы

    /// <summary>
    /// Приводит дату перемещения к UTC
    /// </summary>
    /// <param name="value">Исходное значение</param>
    private static DateTime ToUtc(DateTime value) =>
        value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
        };

    /// <summary>
    /// Формирует краткое описание конца цепочки места (здание, этаж, комната, контейнер, секция)
    /// </summary>
    private static string BuildEndSummary(
        Building? building,
        Floor? floor,
        Room? room,
        Container? container,
        Section? section)
    {
        var parts = new List<string>();
        if (building != null)
        {
            parts.Add(building.Name);
        }

        if (floor != null)
        {
            parts.Add($"этаж {floor.Level}");
        }

        if (room != null)
        {
            parts.Add(room.Name);
        }

        if (container != null)
        {
            parts.Add(container.Name);
        }

        if (section != null)
        {
            parts.Add(section.Name);
        }

        return parts.Count == 0 ? "—" : string.Join(" · ", parts);
    }

    #endregion Приватные методы
}
