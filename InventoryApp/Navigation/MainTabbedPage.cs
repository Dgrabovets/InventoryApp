using InventoryApp.Views.Pages;
using Microsoft.Maui.Controls.PlatformConfiguration;
using AndroidTabbedPage = Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.TabbedPage;
using ToolbarPlacement = Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.ToolbarPlacement;

namespace InventoryApp.Navigation;

/// <summary>
/// Корневая навигация: три вкладки с отдельным стеком <see cref="NavigationPage"/> на каждую
/// </summary>
public sealed class MainTabbedPage : Microsoft.Maui.Controls.TabbedPage
{
    #region Конструкторы

    /// <summary>
    /// Создаёт вкладки «Главная», «Граф», «Профиль» и настраивает панель вкладок
    /// </summary>
    public MainTabbedPage()
    {
        var app = Microsoft.Maui.Controls.Application.Current!;
        BarBackgroundColor = (Color)app.Resources["AppSurface"];
        BarTextColor = (Color)app.Resources["AppText"];
        UnselectedTabColor = (Color)app.Resources["AppMuted"];
        SelectedTabColor = (Color)app.Resources["Primary"];
        BackgroundColor = (Color)app.Resources["AppBg"];

        AndroidTabbedPage.SetToolbarPlacement(this, ToolbarPlacement.Top);

        Children.Add(CreateTabNavigation(new HomePage(), "Главная"));
        Children.Add(CreateTabNavigation(new GraphPage(), "Граф"));
        Children.Add(CreateTabNavigation(new ProfilePage(), "Профиль"));
    }

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Оборачивает страницу вкладки в <see cref="NavigationPage"/> без заголовка на корневом экране
    /// </summary>
    /// <param name="rootPage">Корневая страница вкладки</param>
    /// <param name="tabTitle">Подпись на панели вкладок</param>
    /// <returns>Стек навигации вкладки</returns>
    private static NavigationPage CreateTabNavigation(ContentPage rootPage, string tabTitle)
    {
        NavigationPage.SetHasNavigationBar(rootPage, false);
        var app = Microsoft.Maui.Controls.Application.Current!;
        return new NavigationPage(rootPage)
        {
            Title = tabTitle,
            BarBackgroundColor = (Color)app.Resources["AppSurface"],
            BarTextColor = (Color)app.Resources["AppText"],
            BackgroundColor = (Color)app.Resources["AppBg"],
        };
    }

    #endregion
}
