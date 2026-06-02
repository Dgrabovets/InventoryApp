using CommunityToolkit.Maui;
using InventoryApp.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace InventoryApp;

/// <summary>
/// Точка входа конфигурации MAUI: шрифты, SkiaSharp, логирование, SQLite и инициализация БД
/// </summary>
public static class MauiProgram
{
    #region Публичные методы

    /// <summary>
    /// Создаёт и настраивает экземпляр <see cref="MauiApp"/>: применяет миграции, сид и регистрирует <see cref="AppServices"/>
    /// </summary>
    /// <returns>Собранное приложение</returns>
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseSkiaSharp()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "inventory.db");
        builder.Services.AddInventoryData(dbPath);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
            initializer.InitializeAsync().GetAwaiter().GetResult();
        }

        AppServices.Init(app.Services);

        return app;
    }

    #endregion Публичные методы
}
