using InventoryApp.Application.Models.Export;

namespace InventoryApp.Application.Services.Export;

/// <summary>
/// Экспорт инвентаря в CSV и PDF
/// </summary>
public interface IInventoryExportService
{
    /// <summary>
    /// Список контейнеров для выбора области экспорта
    /// </summary>
    /// <param name="cancellationToken">Токен отмены</param>
    Task<IReadOnlyList<ExportContainerListItemDto>> ListContainersForExportAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Экспорт в CSV
    /// </summary>
    /// <param name="scope">Область выгрузки</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task<byte[]> ExportCsvAsync(ExportScope scope, CancellationToken cancellationToken = default);

    /// <summary>
    /// Экспорт в PDF
    /// </summary>
    /// <param name="scope">Область выгрузки</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task<byte[]> ExportPdfAsync(ExportScope scope, CancellationToken cancellationToken = default);
}
