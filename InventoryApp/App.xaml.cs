namespace InventoryApp;

/// <summary>
/// Корневой класс приложения MAUI: задаёт стартовое окно с <see cref="Navigation.MainTabbedPage"/>
/// </summary>
public partial class App : Microsoft.Maui.Controls.Application
{
    #region Конструкторы

    /// <summary>
    /// Инициализирует приложение и ресурсы из <c>App.xaml</c>
    /// </summary>
    public App()
    {
        InitializeComponent();
    }

    #endregion Конструкторы

    #region Публичные методы

    /// <inheritdoc />
    protected override void OnStart()
    {
        base.OnStart();
        ExitBackupHelper.ResetForSession();
    }

    /// <inheritdoc />
    protected override void OnSleep()
    {
        ExitBackupHelper.RunOnExit();
        base.OnSleep();
    }

    /// <summary>
    /// Создаёт главное окно с вкладками и навигацией по стекам
    /// </summary>
    /// <param name="activationState">Состояние активации платформы</param>
    /// <returns>Экземпляр окна</returns>
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new MainTabbedPage());
#if WINDOWS || MACCATALYST
        window.Destroying += (_, _) => ExitBackupHelper.RunOnExit();
#endif
        return window;
    }

    #endregion Публичные методы
}
