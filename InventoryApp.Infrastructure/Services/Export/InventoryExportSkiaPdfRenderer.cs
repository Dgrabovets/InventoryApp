using System.Globalization;
using InventoryApp.Application.Models.Export;
using SkiaSharp;

namespace InventoryApp.Infrastructure.Services.Export;

/// <summary>
/// Сборка PDF-отчёта через SkiaSharp (поддерживается на Android и десктопе)
/// </summary>
internal static class InventoryExportSkiaPdfRenderer
{
    #region Константы

    /// <summary>
    /// Имя встроенного ресурса шрифта Noto Sans
    /// </summary>
    private const string NotoSansResource = "InventoryApp.Infrastructure.Fonts.NotoSans-Variable.ttf";

    /// <summary>
    /// Ширина страницы A4 альбомная (pt)
    /// </summary>
    private const float PageWidth = 842f;

    /// <summary>
    /// Высота страницы A4 альбомная (pt)
    /// </summary>
    private const float PageHeight = 595f;

    /// <summary>
    /// Поле страницы (pt)
    /// </summary>
    private const float Margin = 28f;

    #endregion Константы

    #region Публичные методы

    /// <summary>
    /// Формирует PDF (A4 альбомная ориентация)
    /// </summary>
    /// <param name="title">Заголовок отчёта</param>
    /// <param name="scopeSubtitle">Подзаголовок области выгрузки</param>
    /// <param name="generatedAtUtc">Момент формирования в UTC</param>
    /// <param name="rows">Строки таблицы</param>
    public static byte[] Render(string title, string scopeSubtitle, DateTime generatedAtUtc, IReadOnlyList<InventoryExportRowDto> rows)
    {
        var culture = CultureInfo.GetCultureInfo("ru-RU");
        var whenLocal = generatedAtUtc.Kind == DateTimeKind.Utc
            ? TimeZoneInfo.ConvertTimeFromUtc(generatedAtUtc, TimeZoneInfo.Local)
            : generatedAtUtc.ToLocalTime();
        var whenText = whenLocal.ToString("F", culture);

        using var stream = new MemoryStream();
        using var typeface = LoadTypeface();
        using var doc = SKDocument.CreatePdf(stream);

        var paintTitle = new SKPaint { Typeface = typeface, TextSize = 15, IsAntialias = true, Color = SKColors.Black };
        var paintSub = new SKPaint { Typeface = typeface, TextSize = 10, IsAntialias = true, Color = new SKColor(0x55, 0x55, 0x55) };
        var paintHeader = new SKPaint { Typeface = typeface, TextSize = 8.5f, IsAntialias = true, Color = SKColors.Black, FakeBoldText = true };
        var paintCell = new SKPaint { Typeface = typeface, TextSize = 8f, IsAntialias = true, Color = SKColors.Black };
        var paintFooter = new SKPaint { Typeface = typeface, TextSize = 8, IsAntialias = true, Color = new SKColor(0x88, 0x88, 0x88) };
        var paintLine = new SKPaint { Color = new SKColor(0xDD, 0xDD, 0xDD), StrokeWidth = 0.5f, IsStroke = true };
        var paintHeaderBg = new SKPaint { Color = new SKColor(0xF3, 0xF4, 0xF6), Style = SKPaintStyle.Fill };

        var columns = new[]
        {
            ("№", 24f),
            ("Название", 140f),
            ("Кол-во", 32f),
            ("Категория", 90f),
            ("Место", 120f),
            ("Описание", 100f),
            ("Заметки", 80f),
            ("Цена", 48f),
            ("Срок годн.", 56f),
        };

        var rowHeight = 16f;
        var y = Margin;
        var pageIndex = 1;
        var canvas = BeginPage(doc, pageIndex, paintFooter, culture);

        y = DrawText(canvas, title, Margin, y, paintTitle) + 4;
        y = DrawText(canvas, scopeSubtitle, Margin, y, paintSub) + 2;
        y = DrawText(canvas, $"Сформировано: {whenText}", Margin, y, paintSub) + 12;

        y = DrawTableHeader(canvas, columns, Margin, y, rowHeight, paintHeader, paintHeaderBg, paintLine);

        var n = 1;
        foreach (var r in rows)
        {
            if (y + rowHeight > PageHeight - Margin - 20)
            {
                doc.EndPage();
                pageIndex++;
                canvas = BeginPage(doc, pageIndex, paintFooter, culture);
                y = Margin;
                y = DrawTableHeader(canvas, columns, Margin, y, rowHeight, paintHeader, paintHeaderBg, paintLine);
            }

            var cells = new[]
            {
                n.ToString(culture),
                Truncate(r.Name, 80),
                r.Quantity.ToString(culture),
                Truncate(r.Category, 40),
                Truncate(r.Location, 50),
                Truncate(r.Description, 45),
                Truncate(r.LocationNotes, 35),
                r.Price,
                r.ExpiryDate,
            };

            y = DrawTableRow(canvas, columns, cells, Margin, y, rowHeight, paintCell, paintLine);
            n++;
        }

        doc.EndPage();
        doc.Close();

        return stream.ToArray();
    }

    #endregion Публичные методы

    #region Приватные методы

    /// <summary>
    /// Загружает встроенный шрифт Noto Sans
    /// </summary>
    private static SKTypeface LoadTypeface()
    {
        var asm = typeof(InventoryExportSkiaPdfRenderer).Assembly;
        var fontStream = asm.GetManifestResourceStream(NotoSansResource);
        if (fontStream is null)
        {
            var names = string.Join(", ", asm.GetManifestResourceNames());
            throw new InvalidOperationException(
                $"Не найден встроенный шрифт «{NotoSansResource}». Ресурсы: {names}");
        }

        return SKTypeface.FromStream(fontStream);
    }

    /// <summary>
    /// Открывает новую страницу PDF
    /// </summary>
    private static SKCanvas BeginPage(SKDocument doc, int pageIndex, SKPaint paintFooter, CultureInfo culture)
    {
        var canvas = doc.BeginPage(PageWidth, PageHeight);
        var footer = $"Страница {pageIndex}";
        canvas.DrawText(footer, PageWidth / 2 - paintFooter.MeasureText(footer) / 2, PageHeight - Margin / 2, paintFooter);
        return canvas;
    }

    /// <summary>
    /// Рисует строку текста и возвращает нижнюю координату Y
    /// </summary>
    private static float DrawText(SKCanvas canvas, string text, float x, float y, SKPaint paint)
    {
        canvas.DrawText(text, x, y + paint.TextSize, paint);
        return y + paint.TextSize + 2;
    }

    /// <summary>
    /// Рисует заголовок таблицы
    /// </summary>
    private static float DrawTableHeader(
        SKCanvas canvas,
        (string Label, float Width)[] columns,
        float x0,
        float y,
        float rowHeight,
        SKPaint paintHeader,
        SKPaint paintHeaderBg,
        SKPaint paintLine)
    {
        var totalWidth = columns.Sum(c => c.Width);
        canvas.DrawRect(x0, y, totalWidth, rowHeight, paintHeaderBg);
        var x = x0;
        foreach (var (label, width) in columns)
        {
            canvas.DrawText(label, x + 3, y + rowHeight - 4, paintHeader);
            canvas.DrawLine(x, y, x, y + rowHeight, paintLine);
            x += width;
        }

        canvas.DrawLine(x0 + totalWidth, y, x0 + totalWidth, y + rowHeight, paintLine);
        canvas.DrawLine(x0, y + rowHeight, x0 + totalWidth, y + rowHeight, paintLine);
        return y + rowHeight;
    }

    /// <summary>
    /// Рисует строку данных таблицы
    /// </summary>
    private static float DrawTableRow(
        SKCanvas canvas,
        (string Label, float Width)[] columns,
        string[] cells,
        float x0,
        float y,
        float rowHeight,
        SKPaint paintCell,
        SKPaint paintLine)
    {
        var totalWidth = columns.Sum(c => c.Width);
        var x = x0;
        for (var i = 0; i < columns.Length; i++)
        {
            var text = i < cells.Length ? cells[i] : string.Empty;
            canvas.DrawText(text, x + 3, y + rowHeight - 4, paintCell);
            canvas.DrawLine(x, y, x, y + rowHeight, paintLine);
            x += columns[i].Width;
        }

        canvas.DrawLine(x0 + totalWidth, y, x0 + totalWidth, y + rowHeight, paintLine);
        canvas.DrawLine(x0, y + rowHeight, x0 + totalWidth, y + rowHeight, paintLine);
        return y + rowHeight;
    }

    /// <summary>
    /// Обрезает строку до заданной длины с многоточием
    /// </summary>
    private static string Truncate(string? s, int max)
    {
        if (string.IsNullOrEmpty(s) || s.Length <= max)
        {
            return s ?? string.Empty;
        }

        return s[..(max - 1)] + "…";
    }

    #endregion Приватные методы
}
