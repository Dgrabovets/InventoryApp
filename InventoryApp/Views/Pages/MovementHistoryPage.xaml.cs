using InventoryApp.Application.Models.Movements;

namespace InventoryApp.Views.Pages;

/// <summary>
/// Журнал перемещений предметов из таблицы item_movements
/// </summary>
public partial class MovementHistoryPage : ContentPage
{
    #region Поля

    /// <summary>
    /// Строка поиска по названию предмета или пользователю
    /// </summary>
    private string? _search;

    /// <summary>
    /// Токен отмены debounce поиска
    /// </summary>
    private CancellationTokenSource? _searchDebounceCts;

    #endregion Поля

    #region Конструкторы

    /// <summary>
    /// Создаёт страницу журнала
    /// </summary>
    public MovementHistoryPage()
    {
        InitializeComponent();
    }

    #endregion Конструкторы

    #region Публичные методы

    /// <inheritdoc />
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = LoadAsync();
    }

    #endregion Публичные методы

    #region Обработчики

    /// <summary>
    /// Отложенное обновление при вводе в SearchBar
    /// </summary>
    private async void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        _searchDebounceCts?.Cancel();
        _searchDebounceCts?.Dispose();
        var cts = new CancellationTokenSource();
        _searchDebounceCts = cts;
        try
        {
            await Task.Delay(400, cts.Token).ConfigureAwait(true);
            _search = string.IsNullOrWhiteSpace(SearchBar.Text) ? null : SearchBar.Text;
            await LoadAsync(cts.Token).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
    }

    /// <summary>
    /// Немедленный поиск по кнопке SearchBar
    /// </summary>
    private void OnSearchPressed(object? sender, EventArgs e)
    {
        _search = string.IsNullOrWhiteSpace(SearchBar.Text) ? null : SearchBar.Text;
        _ = LoadAsync();
    }

    /// <summary>
    /// Обновление списка при pull-to-refresh
    /// </summary>
    private async void OnRefreshing(object? sender, EventArgs e)
    {
        await LoadAsync();
        Refresh.IsRefreshing = false;
    }

    #endregion Обработчики

    #region Приватные методы

    /// <summary>
    /// Загружает последние записи журнала с фильтром поиска
    /// </summary>
    /// <param name="cancellationToken">Токен отмены</param>
    private async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        Loading.IsVisible = Loading.IsRunning = true;
        try
        {
            var list = await AppServices.WithMovementHistoryAsync(s =>
                    s.ListRecentAsync(300, _search, cancellationToken))
                .ConfigureAwait(true);
            MovementsCollection.ItemsSource = list;
        }
        finally
        {
            Loading.IsRunning = false;
            Loading.IsVisible = false;
        }
    }

    #endregion Приватные методы
}
