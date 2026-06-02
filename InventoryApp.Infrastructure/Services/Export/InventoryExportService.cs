using System.Globalization;
using System.Text;
using InventoryApp.Application.Models.Export;
using InventoryApp.Application.Services.Export;
using InventoryApp.Domain.Entities;
using InventoryApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryApp.Infrastructure.Services.Export;

/// <inheritdoc cref="IInventoryExportService" />
public sealed class InventoryExportService : IInventoryExportService
{
    #region Поля

    /// <summary>
    /// Контекст базы данных
    /// </summary>
    private readonly ApplicationContext _context;

    /// <summary>
    /// Журнал операций экспорта
    /// </summary>
    private readonly ILogger<InventoryExportService> _logger;

    #endregion Поля

    #region Конструкторы

    /// <summary>
    /// Создаёт сервис экспорта
    /// </summary>
    /// <param name="context">Контекст базы данных</param>
    /// <param name="logger">Журнал операций</param>
    public InventoryExportService(ApplicationContext context, ILogger<InventoryExportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #endregion Конструкторы

    #region Публичные методы

    /// <inheritdoc />
    public async Task<IReadOnlyList<ExportContainerListItemDto>> ListContainersForExportAsync(
        CancellationToken cancellationToken = default)
    {
        var list = await _context.Containers.AsNoTracking()
            .Where(c => c.IsActive)
            .Include(c => c.ContainerType)
            .Include(c => c.Room)
            .ThenInclude(r => r!.Floor)
            .ThenInclude(f => f!.Building)
            .Include(c => c.Floor)
            .ThenInclude(f => f!.Building)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return list
            .Select(c => new ExportContainerListItemDto(c.Id, BuildContainerPickerLabel(c)))
            .OrderBy(x => x.Label, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<byte[]> ExportCsvAsync(ExportScope scope, CancellationToken cancellationToken = default)
    {
        var (title, subtitle, rows) = await LoadExportPayloadAsync(scope, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("CSV export: {Title}, rows={Count}", title, rows.Count);

        var sb = new StringBuilder();
        sb.AppendLine($"# {title}");
        sb.AppendLine($"# {subtitle}");
        sb.AppendLine("Название;Количество;Категория;Место;Описание;Заметки места;Цена;Срок годности");

        foreach (var r in rows)
        {
            sb.AppendLine(string.Join(';', new[]
            {
                CsvEscape(r.Name),
                CsvEscape(r.Quantity.ToString(CultureInfo.InvariantCulture)),
                CsvEscape(r.Category),
                CsvEscape(r.Location),
                CsvEscape(r.Description),
                CsvEscape(r.LocationNotes),
                CsvEscape(r.Price),
                CsvEscape(r.ExpiryDate),
            }));
        }

        return new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetBytes(sb.ToString());
    }

    /// <inheritdoc />
    public async Task<byte[]> ExportPdfAsync(ExportScope scope, CancellationToken cancellationToken = default)
    {
        var (title, subtitle, rows) = await LoadExportPayloadAsync(scope, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("PDF export: {Title}, rows={Count}", title, rows.Count);
        return InventoryExportSkiaPdfRenderer.Render(title, subtitle, DateTime.UtcNow, rows);
    }

    #endregion Публичные методы

    #region Приватные методы

    /// <summary>
    /// Загружает заголовок и строки для выбранной области экспорта
    /// </summary>
    private async Task<(string Title, string Subtitle, IReadOnlyList<InventoryExportRowDto> Rows)> LoadExportPayloadAsync(
        ExportScope scope,
        CancellationToken cancellationToken)
    {
        if (!scope.IsFullInventory && (scope.ContainerId is null or < 1))
        {
            throw new InvalidOperationException("Выберите контейнер для выгрузки.");
        }

        string title;
        string subtitle;

        if (scope.IsFullInventory)
        {
            title = "Полная выгрузка инвентаря";
            subtitle = "Все активные предметы (включая без привязки к контейнеру).";
        }
        else
        {
            var containerId = scope.ContainerId!.Value;
            var c = await _context.Containers.AsNoTracking()
                .Include(x => x.ContainerType)
                .Include(x => x.Room)
                .ThenInclude(r => r!.Floor)
                .ThenInclude(f => f!.Building)
                .Include(x => x.Floor)
                .ThenInclude(f => f!.Building)
                .FirstOrDefaultAsync(x => x.Id == containerId && x.IsActive, cancellationToken)
                .ConfigureAwait(false);
            if (c is null)
            {
                throw new InvalidOperationException("Контейнер не найден или отключён.");
            }

            title = $"Контейнер: {c.Name}";
            subtitle = BuildContainerPlacementSubtitle(c);
        }

        var items = await QueryItemsForScope(scope, cancellationToken).ConfigureAwait(false);
        var rows = items.Select(MapRow).ToList();
        return (title, subtitle, rows);
    }

    /// <summary>
    /// Выбирает предметы по области экспорта
    /// </summary>
    private async Task<List<Item>> QueryItemsForScope(ExportScope scope, CancellationToken cancellationToken)
    {
        var q = _context.Items.AsNoTracking()
            .Where(i => i.IsActive)
            .Include(i => i.Category)
            .Include(i => i.Building)
            .Include(i => i.Floor)
            .Include(i => i.Room)
            .Include(i => i.Container)
            .Include(i => i.Section)
            .ThenInclude(s => s!.Container)
            .AsQueryable();

        if (!scope.IsFullInventory)
        {
            var cid = scope.ContainerId!.Value;
            q = q.Where(i => i.ContainerId == cid || (i.Section != null && i.Section.ContainerId == cid));
        }

        return await q
            .OrderBy(i => i.Name)
            .ThenBy(i => i.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Преобразует сущность предмета в строку экспорта
    /// </summary>
    private static InventoryExportRowDto MapRow(Item i)
    {
        var culture = CultureInfo.GetCultureInfo("ru-RU");
        var price = i.Price is { } p ? p.ToString("0.##", culture) : string.Empty;
        var expiry = i.ExpiryDate is { } e ? e.ToString("d", culture) : string.Empty;
        return new InventoryExportRowDto(
            i.Name,
            i.Quantity < 1 ? 1 : i.Quantity,
            i.Category?.Name ?? string.Empty,
            BuildItemLocationSummary(i),
            i.Description ?? string.Empty,
            i.LocationNotes ?? string.Empty,
            price,
            expiry);
    }

    /// <summary>
    /// Формирует краткое описание места предмета
    /// </summary>
    private static string BuildItemLocationSummary(Item i)
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

    /// <summary>
    /// Подпись контейнера для списка выбора
    /// </summary>
    private static string BuildContainerPickerLabel(Container c)
    {
        var place = BuildContainerPlacementSubtitle(c);
        var type = c.ContainerType?.Name;
        return string.IsNullOrEmpty(type) ? $"{place} — {c.Name}" : $"{place} — {c.Name} ({type})";
    }

    /// <summary>
    /// Описание расположения контейнера в иерархии
    /// </summary>
    private static string BuildContainerPlacementSubtitle(Container c)
    {
        if (c.Room != null)
        {
            var parts = new List<string>();
            var b = c.Room.Floor?.Building?.Name;
            if (!string.IsNullOrEmpty(b))
            {
                parts.Add(b!);
            }

            if (c.Room.Floor != null)
            {
                parts.Add($"этаж {c.Room.Floor.Level}");
            }

            parts.Add(c.Room.Name);
            return string.Join(" · ", parts);
        }

        if (c.Floor != null)
        {
            var parts = new List<string>();
            var b = c.Floor.Building?.Name;
            if (!string.IsNullOrEmpty(b))
            {
                parts.Add(b!);
            }

            parts.Add($"этаж {c.Floor.Level}");
            return string.Join(" · ", parts);
        }

        return "Место не указано";
    }

    /// <summary>
    /// Экранирует значение для CSV
    /// </summary>
    private static string CsvEscape(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        if (value.Contains(';', StringComparison.Ordinal) ||
            value.Contains('"', StringComparison.Ordinal) ||
            value.Contains('\r', StringComparison.Ordinal) ||
            value.Contains('\n', StringComparison.Ordinal))
        {
            return "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
        }

        return value;
    }

    #endregion Приватные методы
}
