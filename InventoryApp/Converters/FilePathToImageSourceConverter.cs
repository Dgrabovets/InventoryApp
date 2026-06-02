using System.Globalization;

namespace InventoryApp.Converters;

/// <summary>
/// Локальный путь к файлу → <see cref="ImageSource.FromFile"/> или null
/// </summary>
public sealed class FilePathToImageSourceConverter : IValueConverter
{
    #region Публичные методы

    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string path || string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        try
        {
            return File.Exists(path) ? ImageSource.FromFile(path) : null;
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();

    #endregion Публичные методы
}
