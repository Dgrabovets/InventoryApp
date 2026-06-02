using System.Globalization;
using System.Linq;
using InventoryApp.Application.Models.Items;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;

namespace InventoryApp.Views.Pages;

/// <summary>
/// Карточка предмета: просмотр места, редактирование, перенос в другую комнату, удаление из учёта.
/// </summary>
public partial class ItemDetailPage : ContentPage
{
    #region Поля

    /// <summary>
    /// Идентификатор предмета в базе
    /// </summary>
    private readonly int _itemId;
    private readonly Func<Task>? _refreshAfterChanges;
    private readonly List<int?> _categoryIds = new();
    /// <summary>
    /// Идентификаторы комнат для переноса в пикере
    /// </summary>
    private readonly List<int?> _moveRoomIds = new();

    #endregion Поля

    #region Конструкторы

    /// <summary>
    /// Создаёт карточку для предмета с указанным id
    /// </summary>
    /// <param name="itemId">Идентификатор в таблице предметов</param>
    /// <param name="refreshAfterChanges">Вызывается после сохранения или удаления</param>
    public ItemDetailPage(int itemId, Func<Task>? refreshAfterChanges = null)
    {
        InitializeComponent();
        _itemId = itemId;
        _refreshAfterChanges = refreshAfterChanges;
    }

    #endregion Конструкторы

    #region Публичные методы

    /// <inheritdoc />
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAsync().ConfigureAwait(true);
    }

    #endregion Публичные методы

    #region Обработчики

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        var name = NameEntry.Text?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            await DisplayAlertAsync("Проверка", "Введите название.", "Ок").ConfigureAwait(true);
            return;
        }

        var qty = ParsePositiveInt(QuantityEntry.Text, 1);
        var desc = string.IsNullOrWhiteSpace(DescriptionEditor.Text) ? null : DescriptionEditor.Text.Trim();
        var locNotes = string.IsNullOrWhiteSpace(LocationNotesEditor.Text) ? null : LocationNotesEditor.Text.Trim();

        var ci = CategoryPicker.SelectedIndex;
        int? categoryId = ci >= 0 && ci < _categoryIds.Count ? _categoryIds[ci] : null;

        int? moveToRoom = null;
        var ri = MoveRoomPicker.SelectedIndex;
        if (ri > 0 && ri < _moveRoomIds.Count && _moveRoomIds[ri] is { } rid)
        {
            moveToRoom = rid;
        }

        Busy.IsVisible = Busy.IsRunning = true;
        try
        {
            await AppServices.WithInventoryWriteAsync(async w =>
            {
                await w.UpdateItemAsync(_itemId, name, desc, qty, categoryId, locNotes, moveToRoom, CancellationToken.None)
                    .ConfigureAwait(false);
            }).ConfigureAwait(true);

            if (_refreshAfterChanges is not null)
            {
                await _refreshAfterChanges().ConfigureAwait(true);
            }

            await DisplayAlertAsync("Готово", "Изменения сохранены.", "Ок").ConfigureAwait(true);
            await Navigation.PopAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Ошибка", ex.Message, "Ок").ConfigureAwait(true);
        }
        finally
        {
            Busy.IsRunning = false;
            Busy.IsVisible = false;
        }
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        var ok = await DisplayAlertAsync(
                "Удалить из учёта",
                "Предмет будет скрыт из списков и графа. Продолжить?",
                "Удалить",
                "Отмена")
            .ConfigureAwait(true);
        if (!ok)
        {
            return;
        }

        Busy.IsVisible = Busy.IsRunning = true;
        try
        {
            await AppServices.WithInventoryWriteAsync(async w =>
            {
                await w.SoftDeleteItemAsync(_itemId, CancellationToken.None).ConfigureAwait(false);
            }).ConfigureAwait(true);

            if (_refreshAfterChanges is not null)
            {
                await _refreshAfterChanges().ConfigureAwait(true);
            }

            await Navigation.PopAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Ошибка", ex.Message, "Ок").ConfigureAwait(true);
        }
        finally
        {
            Busy.IsRunning = false;
            Busy.IsVisible = false;
        }
    }

    private async void OnAddPhotoClicked(object? sender, EventArgs e)
    {
        try
        {
            var picked = await MediaPicker.PickPhotosAsync(new MediaPickerOptions { Title = "Фото предмета" }).ConfigureAwait(true);
            if (picked is null)
            {
                return;
            }

            var materialized = new List<(string Path, bool DeleteAfter)>();
            foreach (var file in picked)
            {
                var one = await MaterializePickerFileAsync(file).ConfigureAwait(true);
                if (one is { } m)
                {
                    materialized.Add(m);
                }
            }

            if (materialized.Count == 0)
            {
                return;
            }

            Busy.IsVisible = Busy.IsRunning = true;
            try
            {
                await AppServices.WithInventoryWriteAsync(async w =>
                {
                    foreach (var (path, _) in materialized)
                    {
                        await w.AddItemImageFromFileAsync(_itemId, path, FileSystem.AppDataDirectory, false, CancellationToken.None)
                            .ConfigureAwait(false);
                    }
                }).ConfigureAwait(true);
            }
            finally
            {
                Busy.IsRunning = false;
                Busy.IsVisible = false;
            }

            foreach (var (path, deleteAfter) in materialized)
            {
                if (!deleteAfter || !File.Exists(path))
                {
                    continue;
                }

                try
                {
                    File.Delete(path);
                }
                catch
                {
                }
            }

            await LoadAsync().ConfigureAwait(true);
        }
        catch (PermissionException)
        {
            await DisplayAlertAsync("Доступ", "Нужно разрешение на доступ к фото.", "Ок").ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Фото", ex.Message, "Ок").ConfigureAwait(true);
        }
    }

    #endregion Обработчики

    #region Приватные методы

    /// <summary>
    /// Загружает карточку предмета и заполняет форму
    /// </summary>
    private async Task LoadAsync()
    {
        Busy.IsVisible = Busy.IsRunning = true;
        try
        {
            var detail = await AppServices.WithItemCatalogAsync(s => s.GetItemDetailAsync(_itemId)).ConfigureAwait(true);
            if (detail is null)
            {
                await DisplayAlertAsync("Нет данных", "Предмет не найден или снят с учёта.", "Ок").ConfigureAwait(true);
                await Navigation.PopAsync().ConfigureAwait(true);
                return;
            }

            var categories = await AppServices.WithLookupAsync(l => l.GetActiveCategoriesAsync()).ConfigureAwait(true);
            var rooms = await AppServices.WithLookupAsync(l => l.GetRoomsDisplayListAsync()).ConfigureAwait(true);

            _categoryIds.Clear();
            _categoryIds.Add(null);
            var catLabels = new List<string> { "Без категории" };
            foreach (var c in categories)
            {
                _categoryIds.Add(c.Id);
                catLabels.Add(c.Label);
            }

            CategoryPicker.ItemsSource = catLabels;
            var catIdx = detail.CategoryId.HasValue ? _categoryIds.FindIndex(x => x == detail.CategoryId) : 0;
            CategoryPicker.SelectedIndex = catIdx >= 0 ? catIdx : 0;

            _moveRoomIds.Clear();
            _moveRoomIds.Add(null);
            var roomLabels = new List<string> { "Не менять расположение" };
            foreach (var r in rooms)
            {
                _moveRoomIds.Add(r.Id);
                roomLabels.Add(r.Label);
            }

            MoveRoomPicker.ItemsSource = roomLabels;
            MoveRoomPicker.SelectedIndex = 0;

            LocationSummaryLabel.Text = detail.LocationSummary;
            NameEntry.Text = detail.Name;
            DescriptionEditor.Text = detail.Description ?? string.Empty;
            QuantityEntry.Text = detail.Quantity.ToString(CultureInfo.InvariantCulture);
            LocationNotesEditor.Text = detail.LocationNotes ?? string.Empty;

            RebuildPhotosUi(detail.Images);
        }
        finally
        {
            Busy.IsRunning = false;
            Busy.IsVisible = false;
        }
    }

    private static int ParsePositiveInt(string? text, int fallback)
    {
        if (int.TryParse(text?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) && v >= 1)
        {
            return v;
        }

        return fallback;
    }

    private void RebuildPhotosUi(IReadOnlyList<ItemImageRowDto> images)
    {
        PhotosHost.Children.Clear();
        foreach (var im in images)
        {
            var border = new Border
            {
                StrokeThickness = 1,
                Stroke = Colors.Gray,
                BackgroundColor = Colors.Transparent,
                Padding = new Thickness(8),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(10) },
            };

            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto),
                },
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition(GridLength.Auto),
                    new RowDefinition(GridLength.Auto),
                },
            };

            Microsoft.Maui.Controls.Image picture;
            try
            {
                picture = File.Exists(im.FilePath)
                    ? new Microsoft.Maui.Controls.Image
                    {
                        Source = ImageSource.FromFile(im.FilePath),
                        Aspect = Aspect.AspectFit,
                        MaximumHeightRequest = 200,
                        HorizontalOptions = LayoutOptions.Fill,
                    }
                    : new Microsoft.Maui.Controls.Image();
            }
            catch
            {
                picture = new Microsoft.Maui.Controls.Image();
            }

            grid.Children.Add(picture);
            Grid.SetRow(picture, 0);
            Grid.SetColumn(picture, 0);
            Grid.SetColumnSpan(picture, 2);

            var badge = new Label
            {
                Text = im.IsPrimary ? "Основное фото" : string.Empty,
                FontSize = 11,
                TextColor = Microsoft.Maui.Controls.Application.Current?.RequestedTheme == AppTheme.Dark
                    ? Colors.LightGray
                    : Colors.DimGray,
                Margin = new Thickness(0, 4, 0, 0),
            };
            grid.Children.Add(badge);
            Grid.SetRow(badge, 1);
            Grid.SetColumn(badge, 0);

            var buttons = new VerticalStackLayout { Spacing = 6 };
            if (!im.IsPrimary)
            {
                var primaryBtn = new Button { Text = "Сделать основным", FontSize = 12 };
                var imageId = im.Id;
                primaryBtn.Clicked += async (_, _) => await SetPrimaryPhotoAsync(imageId).ConfigureAwait(true);
                buttons.Children.Add(primaryBtn);
            }

            var delBtn = new Button { Text = "Удалить фото", FontSize = 12, BackgroundColor = Colors.DarkRed };
            var delId = im.Id;
            delBtn.Clicked += async (_, _) => await RemovePhotoAsync(delId).ConfigureAwait(true);
            buttons.Children.Add(delBtn);

            grid.Children.Add(buttons);
            Grid.SetRow(buttons, 1);
            Grid.SetColumn(buttons, 1);

            border.Content = grid;
            PhotosHost.Children.Add(border);
        }

        if (images.Count == 0)
        {
            PhotosHost.Children.Add(new Label
            {
                Text = "Пока нет фото. Нажмите «Добавить фото».",
                FontSize = 13,
                TextColor = Colors.Gray,
            });
        }
    }

    /// <summary>
    /// Возвращает путь к файлу для копирования в хранилище приложения
    /// </summary>
    private static async Task<(string Path, bool DeleteAfter)?> MaterializePickerFileAsync(FileResult file)
    {
        var path = file.FullPath;
        if (!string.IsNullOrEmpty(path))
        {
            return (path, false);
        }

        var ext = string.IsNullOrEmpty(file.FileName) ? ".jpg" : global::System.IO.Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(ext))
        {
            ext = ".jpg";
        }

        path = global::System.IO.Path.Combine(FileSystem.CacheDirectory, $"pick_{Guid.NewGuid():N}{ext}");
        await using (var src = await file.OpenReadAsync().ConfigureAwait(true))
        await using (var dst = File.Create(path))
        {
            await src.CopyToAsync(dst).ConfigureAwait(true);
        }

        return (path, true);
    }

    private async Task SetPrimaryPhotoAsync(int imageId)
    {
        Busy.IsVisible = Busy.IsRunning = true;
        try
        {
            await AppServices.WithInventoryWriteAsync(async w =>
            {
                await w.SetPrimaryItemImageAsync(imageId, _itemId, CancellationToken.None).ConfigureAwait(false);
            }).ConfigureAwait(true);
            await LoadAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Ошибка", ex.Message, "Ок").ConfigureAwait(true);
        }
        finally
        {
            Busy.IsRunning = false;
            Busy.IsVisible = false;
        }
    }

    private async Task RemovePhotoAsync(int imageId)
    {
        var ok = await DisplayAlertAsync("Удалить фото", "Файл будет удалён с устройства.", "Удалить", "Отмена").ConfigureAwait(true);
        if (!ok)
        {
            return;
        }

        Busy.IsVisible = Busy.IsRunning = true;
        try
        {
            await AppServices.WithInventoryWriteAsync(async w =>
            {
                await w.RemoveItemImageAsync(imageId, _itemId, CancellationToken.None).ConfigureAwait(false);
            }).ConfigureAwait(true);
            await LoadAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Ошибка", ex.Message, "Ок").ConfigureAwait(true);
        }
        finally
        {
            Busy.IsRunning = false;
            Busy.IsVisible = false;
        }
    }

    #endregion Приватные методы
}
