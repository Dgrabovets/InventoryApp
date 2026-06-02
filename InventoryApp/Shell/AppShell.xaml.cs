namespace InventoryApp.Navigation;

/// <summary>
/// Запасной Shell (не используется при старте; корень — <see cref="MainTabbedPage"/>)
/// </summary>
public partial class AppShell : Microsoft.Maui.Controls.Shell
{
    #region Конструкторы

    /// <summary>
    /// Инициализирует Shell и маршруты из <c>AppShell.xaml</c>
    /// </summary>
    public AppShell()
    {
        InitializeComponent();
    }

    #endregion Конструкторы
}
