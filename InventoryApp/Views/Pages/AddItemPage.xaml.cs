using System.Globalization;

namespace InventoryApp.Views.Pages;

/// <summary>
/// Быстрое добавление предмета с главной: комната, опциональная категория, количество
/// </summary>
public partial class AddItemPage : ContentPage
{
    #region Поля

    /// <summary>
    /// Колбэк обновления сводки на главной после сохранения
    /// </summary>
    private readonly Func<Task>? _refreshAfterSave;

    /// <summary>
    /// Идентификаторы комнат в ItemsSource пикера (индекс 0 — заглушка)
    /// </summary>
    private readonly List<int> _roomIds = new();

    /// <summary>
    /// Идентификаторы категорий в ItemsSource пикера
    /// </summary>
    private readonly List<int?> _categoryIds = new();

    /// <summary>
    /// Справочники комнат и категорий уже загружены
    /// </summary>
    private bool _listsLoaded;

    #endregion Поля

    #region Конструкторы

    /// <summary>
    /// Создаёт страницу добавления предмета
    /// </summary>
    /// <param name="refreshAfterSave">Обновление сводки на главной после сохранения</param>
    public AddItemPage(Func<Task>? refreshAfterSave = null)
    {
        InitializeComponent();
        _refreshAfterSave = refreshAfterSave;
    }

    #endregion Конструкторы

    #region Публичные методы

    /// <inheritdoc />
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_listsLoaded)
        {
            return;
        }

        await LoadPickersAsync().ConfigureAwait(true);
        _listsLoaded = true;
    }

    #endregion Публичные методы

    #region Обработчики

    /// <summary>
    /// Возврат на предыдущую страницу
    /// </summary>
    private async void OnBackClicked(object? sender, EventArgs e) =>
        await Navigation.PopAsync().ConfigureAwait(true);

    /// <summary>
    /// Сохранение нового предмета в базу
    /// </summary>
    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        var name = NameEntry.Text?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            await DisplayAlertAsync("Проверка", "Введите название предмета.", "Ок").ConfigureAwait(true);
            return;
        }

        var ri = RoomPicker.SelectedIndex;
        if (ri <= 0 || ri >= _roomIds.Count || _roomIds[ri] == 0)
        {
            await DisplayAlertAsync(
                    "Комната",
                    "Сначала добавьте строение, этаж и комнату через граф или выберите комнату в списке.",
                    "Ок")
                .ConfigureAwait(true);
            return;
        }

        var roomId = _roomIds[ri];
        var ci = CategoryPicker.SelectedIndex;
        int? categoryId = ci >= 0 && ci < _categoryIds.Count ? _categoryIds[ci] : null;

        var desc = string.IsNullOrWhiteSpace(DescriptionEditor.Text) ? null : DescriptionEditor.Text.Trim();
        var qty = ParsePositiveInt(QuantityEntry.Text, 1);

        Busy.IsVisible = Busy.IsRunning = true;
        try
        {
            await AppServices.WithInventoryWriteAsync(async w =>
            {
                var loc = await w.BuildLocationForRoomAsync(roomId, categoryId, CancellationToken.None).ConfigureAwait(false);
                await w.CreateItemAsync(name, desc, qty, loc, CancellationToken.None).ConfigureAwait(false);
            }).ConfigureAwait(true);

            if (_refreshAfterSave is not null)
            {
                await _refreshAfterSave().ConfigureAwait(true);
            }

            await DisplayAlertAsync("Готово", "Предмет добавлен.", "Ок").ConfigureAwait(true);
            await Navigation.PopAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Ошибка", ex.Message, "Ок").ConfigureAwait(true);
        }
        finally
        {
            Busy.IsRunning = false;
            Busy.IsVisible = false;
        }
    }

    #endregion Обработчики

    #region Приватные методы

    /// <summary>
    /// Заполняет пикеры комнат и категорий
    /// </summary>
    private async Task LoadPickersAsync()
    {
        Busy.IsVisible = Busy.IsRunning = true;
        try
        {
            var rooms = await AppServices.WithLookupAsync(l => l.GetRoomsDisplayListAsync()).ConfigureAwait(true);
            var categories = await AppServices.WithLookupAsync(l => l.GetActiveCategoriesAsync()).ConfigureAwait(true);

            _roomIds.Clear();
            _roomIds.Add(0);
            var roomLabels = new List<string> { "— Выберите комнату —" };
            foreach (var r in rooms)
            {
                _roomIds.Add(r.Id);
                roomLabels.Add(r.Label);
            }

            RoomPicker.ItemsSource = roomLabels;
            RoomPicker.SelectedIndex = rooms.Count == 0 ? 0 : 0;

            _categoryIds.Clear();
            _categoryIds.Add(null);
            var catLabels = new List<string> { "Без категории" };
            foreach (var c in categories)
            {
                _categoryIds.Add(c.Id);
                catLabels.Add(c.Label);
            }

            CategoryPicker.ItemsSource = catLabels;
            CategoryPicker.SelectedIndex = 0;
        }
        finally
        {
            Busy.IsRunning = false;
            Busy.IsVisible = false;
        }
    }

    /// <summary>
    /// Разбирает положительное целое из текста поля
    /// </summary>
    /// <param name="text">Введённый текст</param>
    /// <param name="fallback">Значение по умолчанию</param>
    private static int ParsePositiveInt(string? text, int fallback)
    {
        if (int.TryParse(text?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) && v >= 1)
        {
            return v;
        }

        return fallback;
    }

    #endregion Приватные методы
}
