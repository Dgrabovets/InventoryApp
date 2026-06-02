using InventoryApp.Application.Models.Items;

namespace InventoryApp.Views.Pages;

/// <summary>
/// Главная страница: сводная статистика из SQLite и быстрые действия
/// </summary>
public partial class HomePage : ContentPage
{
    #region Конструкторы

    /// <summary>
    /// Создаёт главную страницу
    /// </summary>
    public HomePage()
    {
        InitializeComponent();
    }

    #endregion Конструкторы

    #region Публичные методы

    /// <inheritdoc />
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = LoadStatsAsync();
    }

    #endregion Публичные методы

    #region Обработчики

    /// <summary>
    /// Обновляет счётчики при жесте pull-to-refresh
    /// </summary>
    private async void OnRefreshing(object? sender, EventArgs e)
    {
        await LoadStatsAsync();
        Refresh.IsRefreshing = false;
    }

    /// <summary>
    /// Переход к форме добавления предмета
    /// </summary>
    private async void OnAddItemClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddItemPage(LoadStatsAsync)).ConfigureAwait(true);
    }

    /// <summary>
    /// Открывает список предметов с поиском
    /// </summary>
    private async void OnBrowseItemsClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new ItemsListPage()).ConfigureAwait(true);
    }

    /// <summary>
    /// Открывает каталог со сроком в ближайшие 30 дней
    /// </summary>
    private async void OnExpiringSoonTapped(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new ItemsListPage(ItemExpiryListFilter.ExpiringWithin30Days)).ConfigureAwait(true);
    }

    /// <summary>
    /// Открывает каталог просроченных позиций
    /// </summary>
    private async void OnExpiredTapped(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new ItemsListPage(ItemExpiryListFilter.Expired)).ConfigureAwait(true);
    }

    #endregion Обработчики

    #region Приватные методы

    /// <summary>
    /// Загружает агрегаты через <see cref="InventoryApp.Application.Services.Statistics.IInventoryStatisticsService"/>
    /// </summary>
    private async Task LoadStatsAsync()
    {
        Loading.IsVisible = Loading.IsRunning = true;
        try
        {
            var s = await AppServices.WithStatisticsAsync(x => x.GetAppSummaryAsync()).ConfigureAwait(true);
            ItemsCountLabel.Text = s.ItemsCount.ToString();
            RoomsCountLabel.Text = s.RoomsCount.ToString();
            CategoriesCountLabel.Text = s.CategoriesCount.ToString();
            UsersCountLabel.Text = s.UsersCount.ToString();
            ExpiringSoonLabel.Text = s.ItemsExpiringWithin30Days.ToString();
            ExpiredLabel.Text = s.ItemsExpired.ToString();
        }
        finally
        {
            Loading.IsRunning = false;
            Loading.IsVisible = false;
        }
    }

    #endregion Приватные методы
}
