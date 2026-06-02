namespace InventoryApp.Services;

/// <summary>
/// Настройки резервного копирования в <see cref="Preferences"/>
/// </summary>
public static class BackupUserPreferences
{
    #region Поля

    /// <summary>
    /// Ключ Preferences для пути к папке копий
    /// </summary>
    private const string KeyFolder = "backup_target_folder";

    /// <summary>
    /// Ключ Preferences для автокопии при выходе
    /// </summary>
    private const string KeyAuto = "backup_auto_on_exit";

    #endregion Поля

    #region Свойства

    /// <summary>
    /// Полный путь к каталогу, куда сохранять ZIP (выбор пользователя)
    /// </summary>
    public static string? BackupFolderPath
    {
        get
        {
            var s = Preferences.Get(KeyFolder, (string?)null);
            return string.IsNullOrWhiteSpace(s) ? null : s;
        }
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                Preferences.Remove(KeyFolder);
            }
            else
            {
                Preferences.Set(KeyFolder, value);
            }
        }
    }

    /// <summary>
    /// Создавать копию при завершении работы приложения (если задана папка)
    /// </summary>
    public static bool AutoBackupOnExit
    {
        get => Preferences.Get(KeyAuto, false);
        set => Preferences.Set(KeyAuto, value);
    }

    #endregion Свойства
}
