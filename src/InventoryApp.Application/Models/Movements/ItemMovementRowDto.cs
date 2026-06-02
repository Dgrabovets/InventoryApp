namespace InventoryApp.Application.Models.Movements;

/// <summary>
/// Строка истории перемещения предмета для списка
/// </summary>
/// <param name="ItemName">Название предмета</param>
/// <param name="MovedAtUtc">Момент перемещения в UTC</param>
/// <param name="FromSummary">Откуда (кратко)</param>
/// <param name="ToSummary">Куда (кратко)</param>
/// <param name="MovedByUsername">Кто переместил</param>
public sealed record ItemMovementRowDto(string ItemName,
                                        DateTime MovedAtUtc,
                                        string FromSummary,
                                        string ToSummary,
                                        string MovedByUsername)
{
    #region Свойства

    /// <summary>
    /// Дата и время для отображения в локальной зоне
    /// </summary>
    public string WhenLocalText =>
        MovedAtUtc.ToLocalTime().ToString("dd.MM.yyyy HH:mm");

    /// <summary>
    /// Строка «откуда» для UI
    /// </summary>
    public string FromLine => string.IsNullOrWhiteSpace(FromSummary) ? "—" : FromSummary;

    /// <summary>
    /// Строка «куда» для UI
    /// </summary>
    public string ToLine => string.IsNullOrWhiteSpace(ToSummary) ? "—" : ToSummary;

    /// <summary>
    /// Строка «кто» для UI
    /// </summary>
    public string ByLine => string.IsNullOrWhiteSpace(MovedByUsername) ? "—" : MovedByUsername;

    #endregion Свойства
}
