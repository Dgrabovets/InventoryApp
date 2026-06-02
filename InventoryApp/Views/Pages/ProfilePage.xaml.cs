using System.Globalization;
using CommunityToolkit.Maui.Storage;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Devices;

namespace InventoryApp.Views.Pages;

/// <summary>
/// Страница профиля: только просмотр статистики локального пользователя (без редактирования).
/// </summary>
public partial class ProfilePage : ContentPage
{
    #region Поля

    /// <summary>
    /// Подавление реакции на программное изменение переключателя автокопии
    /// </summary>
    private bool _muteBackupSwitch;

    #endregion Поля

    #region Конструкторы

    /// <summary>
    /// Создаёт страницу профиля
    /// </summary>
    public ProfilePage()
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

    private async void OnMovementHistoryClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new MovementHistoryPage()).ConfigureAwait(true);
    }

    private async void OnExportClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new ExportPage()).ConfigureAwait(true);
    }

    private async void OnPickBackupFolderClicked(object? sender, EventArgs e)
    {
        try
        {
            var result = await FolderPicker.Default.PickAsync("Папка для резервных копий", CancellationToken.None)
                .ConfigureAwait(true);
            if (result.IsCancelled)
            {
                return;
            }

            if (!result.IsSuccessful || result.Folder is null)
            {
                if (result.Exception is not null)
                {
                    await DisplayAlertAsync("Папка", result.Exception.Message, "Ок").ConfigureAwait(true);
                }

                return;
            }

            var path = result.Folder.Path;
            if (string.IsNullOrWhiteSpace(path))
            {
                await DisplayAlertAsync("Папка", "Не удалось получить путь к папке на этом устройстве.", "Ок")
                    .ConfigureAwait(true);
                return;
            }

            BackupUserPreferences.BackupFolderPath = path;
            RefreshBackupUi();
            await DisplayAlertAsync("Готово", $"Копии будут сохраняться в:\n{path}", "Ок").ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Папка", ex.Message, "Ок").ConfigureAwait(true);
        }
    }

    private void OnBackupAutoToggled(object? sender, ToggledEventArgs e)
    {
        if (_muteBackupSwitch)
        {
            return;
        }

        BackupUserPreferences.AutoBackupOnExit = e.Value;
    }

    private async void OnBackupNowClicked(object? sender, EventArgs e)
    {
        var dir = BackupUserPreferences.BackupFolderPath;
        if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
        {
            await DisplayAlertAsync(
                    "Копия",
                    "Сначала нажмите «Выбрать папку для копий».",
                    "Ок")
                .ConfigureAwait(true);
            return;
        }

        Loading.IsVisible = Loading.IsRunning = true;
        try
        {
            var path = await AppServices.WithBackupAsync(s =>
                    s.CreateFullBackupAtDirectoryAsync(dir, "inventory_manual", CancellationToken.None))
                .ConfigureAwait(true);
            await DisplayAlertAsync("Готово", $"Создан файл:\n{path}", "Ок").ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Копия", ex.Message, "Ок").ConfigureAwait(true);
        }
        finally
        {
            Loading.IsRunning = false;
            Loading.IsVisible = false;
        }
    }

    private async void OnShareBackupClicked(object? sender, EventArgs e)
    {
        Loading.IsVisible = Loading.IsRunning = true;
        try
        {
            var zipPath = await AppServices.WithBackupAsync(s => s.CreateFullBackupArchiveInTempAsync())
                .ConfigureAwait(true);
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Сохранить резервную копию",
                File = new ShareFile(zipPath),
            }).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Копия", ex.Message, "Ок").ConfigureAwait(true);
        }
        finally
        {
            Loading.IsRunning = false;
            Loading.IsVisible = false;
        }
    }

    private async void OnRestoreFromZipClicked(object? sender, EventArgs e)
    {
        var confirm = await DisplayAlertAsync(
            "Восстановление",
            "Текущая база и все фото предметов будут заменены содержимым архива. Продолжить?",
            "Восстановить",
            "Отмена").ConfigureAwait(true);
        if (!confirm)
        {
            return;
        }

        try
        {
            var pick = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Архив резервной копии (.zip)",
                FileTypes = ZipFilePickerFileType.Zip,
            }).ConfigureAwait(true);
            if (pick is null)
            {
                return;
            }

            var path = pick.FullPath;
            if (string.IsNullOrEmpty(path))
            {
                await DisplayAlertAsync("Восстановление", "Не удалось получить путь к файлу.", "Ок").ConfigureAwait(true);
                return;
            }

            Loading.IsVisible = Loading.IsRunning = true;
            try
            {
                await AppServices.WithBackupAsync(s => s.RestoreFromZipAsync(path, CancellationToken.None))
                    .ConfigureAwait(false);
            }
            finally
            {
                Loading.IsRunning = false;
                Loading.IsVisible = false;
            }

            await DisplayAlertAsync(
                    "Готово",
                    "Данные восстановлены. Приложение будет закрыто — откройте его снова.",
                    "Ок")
                .ConfigureAwait(true);
            Microsoft.Maui.Controls.Application.Current?.Quit();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Восстановление", ex.Message, "Ок").ConfigureAwait(true);
        }
    }

    /// <summary>
    /// Обновляет данные при pull-to-refresh
    /// </summary>
    private async void OnRefreshing(object? sender, EventArgs e)
    {
        await LoadAsync();
        Refresh.IsRefreshing = false;
    }

    #endregion Обработчики

    #region Приватные методы

    /// <summary>
    /// Загружает статистику профиля и заполняет поля на форме
    /// </summary>
    private async Task LoadAsync()
    {
        Loading.IsVisible = Loading.IsRunning = true;
        try
        {
            var p = await AppServices.WithStatisticsAsync(x => x.GetLocalUserProfileStatsAsync()).ConfigureAwait(true);
            UsernameLabel.Text = p.Username;
            RoleLabel.Text = p.RoleName;
            LastActivityLabel.Text = p.LastActivity is { } dt
                ? dt.ToLocalTime().ToString("g", CultureInfo.CurrentCulture)
                : "—";

            TotalActionsLabel.Text = p.TotalActions.ToString();
            MovementsLabel.Text = p.ItemMovements.ToString();
            AddedLabel.Text = p.ItemsAdded.ToString();
            DeletedLabel.Text = p.ItemsDeleted.ToString();
            EditedLabel.Text = p.ItemsEdited.ToString();

            CreatedItemsLabel.Text = p.CreatedItems.ToString();
            CreatedBuildingsLabel.Text = p.CreatedBuildings.ToString();
            CreatedRoomsLabel.Text = p.CreatedRooms.ToString();
            CreatedContainersLabel.Text = p.CreatedContainers.ToString();
            CreatedCategoriesLabel.Text = p.CreatedCategories.ToString();

            RefreshBackupUi();
        }
        finally
        {
            Loading.IsRunning = false;
            Loading.IsVisible = false;
        }
    }

    private void RefreshBackupUi()
    {
        var path = BackupUserPreferences.BackupFolderPath;
        BackupFolderLabel.Text = string.IsNullOrWhiteSpace(path)
            ? "Папка не выбрана."
            : path;

        _muteBackupSwitch = true;
        BackupAutoSwitch.IsToggled = BackupUserPreferences.AutoBackupOnExit;
        _muteBackupSwitch = false;
    }

    private static class ZipFilePickerFileType
    {
        public static FilePickerFileType Zip { get; } = new(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.WinUI, new[] { ".zip" } },
            { DevicePlatform.Android, new[] { "application/zip" } },
            { DevicePlatform.iOS, new[] { "public.zip-archive", "com.pkware.zip-archive" } },
            { DevicePlatform.MacCatalyst, new[] { "public.zip-archive", "com.pkware.zip-archive" } },
            { DevicePlatform.macOS, new[] { "zip", "public.zip-archive" } },
        });
    }

    #endregion Приватные методы
}
