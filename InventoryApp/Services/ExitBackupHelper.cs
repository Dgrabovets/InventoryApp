using InventoryApp.Application.Services.Backup;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryApp.Services;

/// <summary>
/// Автоматическая копия при выходе: один раз за завершение процесса, если включено в настройках
/// </summary>
public static class ExitBackupHelper
{
    #region Поля

    /// <summary>
    /// Флаг «копия при выходе уже запускалась» (0/1)
    /// </summary>
    private static int _oncePerExit;

    #endregion Поля

    #region Публичные методы

    /// <summary>
    /// Сброс флага при новом запуске приложения (<see cref="App.OnStart"/>)
    /// </summary>
    public static void ResetForSession()
    {
        Interlocked.Exchange(ref _oncePerExit, 0);
    }

    /// <summary>
    /// Синхронный запуск копии в выбранную папку (для обработчиков завершения без async)
    /// </summary>
    public static void RunOnExit()
    {
        if (Interlocked.Exchange(ref _oncePerExit, 1) != 0)
        {
            return;
        }

        try
        {
            if (!BackupUserPreferences.AutoBackupOnExit)
            {
                return;
            }

            var dir = BackupUserPreferences.BackupFolderPath;
            if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
            {
                return;
            }

            var services = AppServices.Services;
            if (services is null)
            {
                return;
            }

            var factory = services.GetService<IServiceScopeFactory>();
            if (factory is null)
            {
                return;
            }

            using var scope = factory.CreateScope();
            var backup = scope.ServiceProvider.GetRequiredService<IInventoryBackupService>();
            backup.CreateFullBackupAtDirectoryAsync(dir, "inventory_exit", CancellationToken.None).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Exit backup: {ex}");
        }
    }

    #endregion Публичные методы
}
