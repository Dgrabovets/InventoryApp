using InventoryApp.Application.Models.Statistics;
using InventoryApp.Application.Services.Statistics;
using InventoryApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Infrastructure.Services.Statistics;

/// <summary>
/// Реализация <see cref="IInventoryStatisticsService"/> поверх <see cref="ApplicationContext"/>
/// </summary>
public sealed class InventoryStatisticsService : IInventoryStatisticsService
{
    #region Поля

    /// <summary>
    /// Контекст базы данных
    /// </summary>
    private readonly ApplicationContext _context;

    #endregion Поля

    #region Конструкторы

    /// <summary>
    /// Создаёт сервис статистики
    /// </summary>
    /// <param name="context">Контекст базы данных</param>
    public InventoryStatisticsService(ApplicationContext context)
    {
        _context = context;
    }

    #endregion Конструкторы

    #region Публичные методы

    /// <inheritdoc />
    public async Task<InventoryAppSummary> GetAppSummaryAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var in30 = today.AddDays(30);

        return new InventoryAppSummary
        {
            ItemsCount = await _context.Items.AsNoTracking()
                .CountAsync(i => i.IsActive, cancellationToken)
                .ConfigureAwait(false),
            RoomsCount = await _context.Rooms.AsNoTracking().CountAsync(cancellationToken).ConfigureAwait(false),
            CategoriesCount = await _context.Categories.AsNoTracking().CountAsync(cancellationToken).ConfigureAwait(false),
            UsersCount = await _context.Users.AsNoTracking().CountAsync(cancellationToken).ConfigureAwait(false),
            ItemsExpiringWithin30Days = await _context.Items.AsNoTracking()
                .CountAsync(
                    i => i.IsActive && i.ExpiryDate != null && i.ExpiryDate >= today && i.ExpiryDate <= in30,
                    cancellationToken)
                .ConfigureAwait(false),
            ItemsExpired = await _context.Items.AsNoTracking()
                .CountAsync(i => i.IsActive && i.ExpiryDate != null && i.ExpiryDate < today, cancellationToken)
                .ConfigureAwait(false),
        };
    }

    /// <inheritdoc />
    public async Task<LocalUserProfileStats> GetLocalUserProfileStatsAsync(CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.AsNoTracking()
            .Include(u => u.Role)
            .OrderBy(u => u.Id)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
        {
            return new LocalUserProfileStats();
        }

        var userId = user.Id;

        var movements = await _context.ItemMovements.AsNoTracking()
            .Where(im => im.MovedById == userId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var auditLogs = await _context.AuditLogs.AsNoTracking()
            .Where(al => al.UserId == userId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var lastMovement = movements.Count > 0 ? movements.Max(m => (DateTime?)m.MovedAt) : null;
        var lastAudit = auditLogs.Count > 0 ? auditLogs.Max(al => (DateTime?)al.CreatedAt) : null;
        DateTime? lastActivity = lastMovement.HasValue && lastAudit.HasValue
            ? (lastMovement.Value > lastAudit.Value ? lastMovement.Value : lastAudit.Value)
            : lastMovement ?? lastAudit;

        return new LocalUserProfileStats
        {
            Username = user.Username,
            RoleName = user.Role?.Name ?? "—",
            ItemsAdded = movements.Count(ItemMovementKind.IsAddition),
            ItemsDeleted = movements.Count(ItemMovementKind.IsDeletion),
            ItemMovements = movements.Count(ItemMovementKind.IsRelocation),
            ItemsEdited = auditLogs.Count(al => al.ActionType == "UPDATE"),
            TotalActions = movements.Count + auditLogs.Count,
            LastActivity = lastActivity,
            CreatedItems = await _context.Items.AsNoTracking().CountAsync(i => i.CreatedById == userId, cancellationToken).ConfigureAwait(false),
            CreatedBuildings = await _context.Buildings.AsNoTracking().CountAsync(b => b.CreatedById == userId, cancellationToken).ConfigureAwait(false),
            CreatedRooms = await _context.Rooms.AsNoTracking().CountAsync(r => r.CreatedById == userId, cancellationToken).ConfigureAwait(false),
            CreatedContainers = await _context.Containers.AsNoTracking().CountAsync(c => c.CreatedById == userId, cancellationToken).ConfigureAwait(false),
            CreatedCategories = await _context.Categories.AsNoTracking().CountAsync(c => c.CreatedById == userId, cancellationToken).ConfigureAwait(false),
        };
    }

    #endregion Публичные методы
}
