using System.Globalization;
using InventoryApp.Application.Models.Export;
using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace InventoryApp.Views.Pages;

/// <summary>
/// Экспорт инвентаря в CSV и PDF с выбором полной выгрузки или контейнера
/// </summary>
public partial class ExportPage : ContentPage
{
    #region Поля

    /// <summary>
    /// Список контейнеров для режима «только контейнер»
    /// </summary>
    private readonly List<ExportContainerListItemDto> _containers = new();

    #endregion Поля

    #region Конструкторы

    /// <summary>
    /// Создаёт страницу экспорта
    /// </summary>
    public ExportPage()
    {
        InitializeComponent();
        ScopePicker.ItemsSource = new[]
        {
            "Весь инвентарь",
            "Только выбранный контейнер",
        };
        ScopePicker.SelectedIndex = 0;
    }

    #endregion Конструкторы

    #region Публичные методы

    /// <inheritdoc />
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadContainersAsync().ConfigureAwait(true);
    }

    #endregion Публичные методы

    #region Обработчики

    /// <summary>
    /// Показывает или скрывает выбор контейнера при смене области экспорта
    /// </summary>
    private async void OnScopeChanged(object? sender, EventArgs e)
    {
        var show = ScopePicker.SelectedIndex == 1;
        ContainerSectionTitle.IsVisible = show;
        ContainerPicker.IsVisible = show;
        if (show && _containers.Count == 0)
        {
            await LoadContainersAsync().ConfigureAwait(true);
        }
    }

    /// <summary>
    /// Запуск экспорта в CSV
    /// </summary>
    private async void OnExportCsvClicked(object? sender, EventArgs e)
    {
        await RunExportAsync(isPdf: false).ConfigureAwait(true);
    }

    /// <summary>
    /// Запуск экспорта в PDF
    /// </summary>
    private async void OnExportPdfClicked(object? sender, EventArgs e)
    {
        await RunExportAsync(isPdf: true).ConfigureAwait(true);
    }

    #endregion Обработчики

    #region Приватные методы

    /// <summary>
    /// Загружает список контейнеров для пикера
    /// </summary>
    private async Task LoadContainersAsync()
    {
        Busy.IsVisible = Busy.IsRunning = true;
        try
        {
            var list = await AppServices.WithExportAsync(s => s.ListContainersForExportAsync()).ConfigureAwait(true);
            _containers.Clear();
            _containers.AddRange(list);
            ContainerPicker.ItemsSource = _containers.Select(c => c.Label).ToList();
            if (_containers.Count > 0)
            {
                ContainerPicker.SelectedIndex = 0;
            }
        }
        finally
        {
            Busy.IsRunning = false;
            Busy.IsVisible = false;
        }
    }

    /// <summary>
    /// Формирует файл и открывает системный диалог «Поделиться»
    /// </summary>
    /// <param name="isPdf">true — PDF, false — CSV</param>
    private async Task RunExportAsync(bool isPdf)
    {
        ExportScope scope;
        try
        {
            scope = BuildScopeOrThrow();
        }
        catch (InvalidOperationException ex)
        {
            await DisplayAlertAsync("Экспорт", ex.Message, "Ок").ConfigureAwait(true);
            return;
        }

        Busy.IsVisible = Busy.IsRunning = true;
        try
        {
            var bytes = isPdf
                ? await AppServices.WithExportAsync(s => s.ExportPdfAsync(scope)).ConfigureAwait(true)
                : await AppServices.WithExportAsync(s => s.ExportCsvAsync(scope)).ConfigureAwait(true);

            var ext = isPdf ? "pdf" : "csv";
            var name = BuildFileName(scope, ext);
            var path = Path.Combine(FileSystem.CacheDirectory, name);
            await File.WriteAllBytesAsync(path, bytes).ConfigureAwait(true);

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = isPdf ? "Поделиться PDF" : "Поделиться CSV",
                File = new ShareFile(path),
            }).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Экспорт", ex.Message, "Ок").ConfigureAwait(true);
        }
        finally
        {
            Busy.IsRunning = false;
            Busy.IsVisible = false;
        }
    }

    /// <summary>
    /// Строит область экспорта из выбора на форме
    /// </summary>
    private ExportScope BuildScopeOrThrow()
    {
        if (ScopePicker.SelectedIndex <= 0)
        {
            return ExportScope.FullInventory;
        }

        if (_containers.Count == 0)
        {
            throw new InvalidOperationException("Нет активных контейнеров для выгрузки.");
        }

        var idx = ContainerPicker.SelectedIndex;
        if (idx < 0 || idx >= _containers.Count)
        {
            throw new InvalidOperationException("Выберите контейнер в списке.");
        }

        return ExportScope.Container(_containers[idx].Id);
    }

    /// <summary>
    /// Имя файла экспорта с меткой времени
    /// </summary>
    /// <param name="scope">Область выгрузки</param>
    /// <param name="ext">Расширение без точки</param>
    private static string BuildFileName(ExportScope scope, string ext)
    {
        var stamp = DateTime.Now.ToString("yyyyMMdd_HHmm", CultureInfo.InvariantCulture);
        return scope.IsFullInventory
            ? $"inventory_full_{stamp}.{ext}"
            : $"inventory_container_{scope.ContainerId}_{stamp}.{ext}";
    }

    #endregion Приватные методы
}
