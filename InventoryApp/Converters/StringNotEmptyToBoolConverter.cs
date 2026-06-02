using System.Globalization;

namespace InventoryApp.Converters;

/// <summary>
/// Непустая строка → true (для видимости превью в списке)
/// </summary>
public sealed class StringNotEmptyToBoolConverter : IValueConverter
{
    #region Публичные методы

    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string s && !string.IsNullOrWhiteSpace(s);

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();

    #endregion Публичные методы
}
