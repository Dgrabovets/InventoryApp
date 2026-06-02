using InventoryApp.Application.Models.Statistics;

namespace InventoryApp.Application.Services.Statistics;

/// <summary>
/// Агрегированная статистика приложения и локального пользователя
/// </summary>
public interface IInventoryStatisticsService
{
    /// <summary>
    /// Возвращает сводку для главной страницы
    /// </summary>
    /// <param name="cancellationToken">Токен отмены</param>
    Task<InventoryAppSummary> GetAppSummaryAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает статистику локального пользователя для профиля
    /// </summary>
    /// <param name="cancellationToken">Токен отмены</param>
    Task<LocalUserProfileStats> GetLocalUserProfileStatsAsync(CancellationToken cancellationToken = default);
}
