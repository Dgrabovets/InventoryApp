using InventoryApp.Application.Models.Items;

namespace InventoryApp.Views.Pages;

/// <summary>
/// Список предметов с поиском и переходом к карточке
/// </summary>
public partial class ItemsListPage : ContentPage
{
    #region Поля

    /// <summary>
    /// Текст поиска или null, если строка пустая
    /// </summary>
    private string? _search;

    /// <summary>
    /// Токен отмены для debounce поиска
    /// </summary>
    private CancellationTokenSource? _searchDebounceCts;

    /// <summary>
    /// Фильтр по сроку годности для списка
    /// </summary>
    private readonly ItemExpiryListFilter _expiryFilter;

    #endregion Поля

    #region Конструкторы

    /// <summary>
    /// Создаёт страницу каталога предметов
    /// </summary>
    public ItemsListPage()
        : this(ItemExpiryListFilter.All)
    {
    }

    /// <summary>
    /// Создаёт страницу каталога с фильтром по сроку годности
    /// </summary>
    /// <param name="expiryFilter">Режим фильтрации по дате срока</param>
    public ItemsListPage(ItemExpiryListFilter expiryFilter)
    {
        InitializeComponent();
        _expiryFilter = expiryFilter;
        Title = expiryFilter switch
        {
            ItemExpiryListFilter.ExpiringWithin30Days => "Срок ≤ 30 дней",
            ItemExpiryListFilter.Expired => "Просрочено",
            _ => "Предметы",
        };

        if (expiryFilter == ItemExpiryListFilter.ExpiringWithin30Days)
        {
            ItemsCollection.EmptyView = "Нет предметов со сроком в ближайшие 30 дней.";
        }
        else if (expiryFilter == ItemExpiryListFilter.Expired)
        {
            ItemsCollection.EmptyView = "Нет просроченных позиций (с заполненным сроком).";
        }
    }

    #endregion Конструкторы

    #region Публичные методы

    /// <inheritdoc />
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = LoadItemsAsync();
    }

    #endregion Публичные методы

    #region Обработчики

    /// <summary>
    /// Обновляет список при pull-to-refresh
    /// </summary>
    private async void OnRefreshing(object? sender, EventArgs e)
    {
        await LoadItemsAsync();
        Refresh.IsRefreshing = false;
    }

    /// <summary>
    /// Отложенный поиск при вводе текста
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
            await LoadItemsAsync(cts.Token).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
    }

    /// <summary>
    /// Немедленный поиск по кнопке на клавиатуре
    /// </summary>
    private void OnSearchPressed(object? sender, EventArgs e)
    {
        _search = string.IsNullOrWhiteSpace(SearchBar.Text) ? null : SearchBar.Text;
        _ = LoadItemsAsync();
    }

    /// <summary>
    /// Переход к карточке выбранного предмета
    /// </summary>
    private async void OnItemSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not ItemListRowDto row)
        {
            return;
        }

        ItemsCollection.SelectedItem = null;
        await Navigation.PushAsync(new ItemDetailPage(row.Id, () => LoadItemsAsync())).ConfigureAwait(true);
    }

    #endregion Обработчики

    #region Приватные методы

    /// <summary>
    /// Загружает строки каталога с учётом поиска и фильтра срока
    /// </summary>
    /// <param name="cancellationToken">Токен отмены</param>
    private async Task LoadItemsAsync(CancellationToken cancellationToken = default)
    {
        Loading.IsVisible = Loading.IsRunning = true;
        try
        {
            var list = await AppServices.WithItemCatalogAsync(s =>
                    s.ListItemsAsync(_search, _expiryFilter, cancellationToken))
                .ConfigureAwait(true);
            ItemsCollection.ItemsSource = list;
        }
        finally
        {
            Loading.IsRunning = false;
            Loading.IsVisible = false;
        }
    }

    #endregion Приватные методы
}
