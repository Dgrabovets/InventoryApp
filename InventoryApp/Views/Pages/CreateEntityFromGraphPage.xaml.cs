using InventoryApp.Application.Models.Graph;
using InventoryApp.Application.Models.InventoryWrite;

namespace InventoryApp.Views.Pages;

/// <summary>
/// Модальная форма создания сущности относительно выбранного узла графа (или корневого строения).
/// </summary>
public partial class CreateEntityFromGraphPage : ContentPage
{
    #region Поля

    /// <summary>
    /// Узел-родитель на графе или null для корневого строения
    /// </summary>
    private readonly GraphNode? _parent;
    private readonly Func<Task>? _refreshAfterSave;
    private readonly List<KindOption> _kindOptions = new();
    private readonly List<int?> _containerTypeOptionIds = new();
    private bool _suppressKind;
    /// <summary>
    /// Справочник типов контейнеров уже загружен
    /// </summary>
    private bool _containerTypesLoaded;

    #endregion Поля

    #region Конструкторы

    /// <summary>
    /// Создаёт страницу; при parent null доступно только строение
    /// </summary>
    /// <param name="parent">Узел-родитель на графе</param>
    /// <param name="refreshAfterSave">Вызывается после успешного сохранения</param>
    public CreateEntityFromGraphPage(GraphNode? parent, Func<Task>? refreshAfterSave = null)
    {
        InitializeComponent();
        _parent = parent;
        _refreshAfterSave = refreshAfterSave;
        BuildContextLabel();
        RebuildKindPicker();
        _suppressKind = true;
        KindPicker.SelectedIndex = _kindOptions.Count > 0 ? 0 : -1;
        _suppressKind = false;
        ApplyKindUi();
    }

    #endregion Конструкторы

    #region Публичные методы

    /// <inheritdoc />
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_containerTypesLoaded)
        {
            return;
        }

        await LoadContainerTypesAsync().ConfigureAwait(true);
        _containerTypesLoaded = true;
    }

    #endregion Публичные методы

    #region Обработчики

    /// <summary>
    /// Смена типа создаваемой сущности
    /// </summary>
    private void OnKindChanged(object? sender, EventArgs e)
    {
        if (_suppressKind)
        {
            return;
        }

        ApplyKindUi();
    }

    /// <summary>
    /// Закрытие модальной формы без сохранения
    /// </summary>
    private async void OnCancelClicked(object? sender, EventArgs e) => await CloseModalAsync().ConfigureAwait(true);

    /// <summary>
    /// Создание сущности по выбранному типу
    /// </summary>
    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        var name = NameEntry.Text?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            await DisplayAlertAsync("Проверка", "Введите название.", "Ок").ConfigureAwait(true);
            return;
        }

        var desc = string.IsNullOrWhiteSpace(DescriptionEditor.Text) ? null : DescriptionEditor.Text.Trim();
        var key = CurrentKindKey();
        if (string.IsNullOrEmpty(key))
        {
            await DisplayAlertAsync("Нельзя сохранить", "Выберите тип записи.", "Ок").ConfigureAwait(true);
            return;
        }

        if (key != "building" && _parent is null)
        {
            await DisplayAlertAsync("Ошибка", "Выберите узел-родитель на графе.", "Ок").ConfigureAwait(true);
            return;
        }

        var pType = "";
        var pId = 0;
        if (_parent is not null && !GraphNodeIdParser.TryParse(_parent, out pType, out pId))
        {
            await DisplayAlertAsync("Ошибка", "Не удалось определить родительскую сущность.", "Ок").ConfigureAwait(true);
            return;
        }

        Busy.IsVisible = Busy.IsRunning = true;
        try
        {
            await AppServices.WithInventoryWriteAsync(async w =>
            {
                var qty = ParsePositiveInt(QuantityEntry.Text, 1);
                var level = ParseInt(LevelEntry.Text, 1);
                var containerTypeId = GetSelectedContainerTypeId();

                switch (key)
                {
                    case "building":
                        await w.CreateBuildingAsync(name, desc, CancellationToken.None).ConfigureAwait(false);
                        break;
                    case "floor":
                        await w.CreateFloorAsync(pId, level, name, desc, CancellationToken.None).ConfigureAwait(false);
                        break;
                    case "room":
                        await w.CreateRoomAsync(pId, name, desc, CancellationToken.None).ConfigureAwait(false);
                        break;
                    case "container_floor":
                        await w.CreateContainerOnFloorAsync(pId, name, desc, containerTypeId, CancellationToken.None).ConfigureAwait(false);
                        break;
                    case "container_room":
                        await w.CreateContainerInRoomAsync(pId, name, desc, containerTypeId, CancellationToken.None).ConfigureAwait(false);
                        break;
                    case "section":
                        await w.CreateSectionAsync(pId, name, desc, CancellationToken.None).ConfigureAwait(false);
                        break;
                    case "category_child":
                        await w.CreateCategoryAsync(pId, name, desc, CancellationToken.None).ConfigureAwait(false);
                        break;
                    case "item_category":
                        await w.CreateItemAsync(name, desc, qty, w.CategoryOnly(pId), CancellationToken.None).ConfigureAwait(false);
                        break;
                    case "item_again":
                        var snap = await w.GetItemLocationSnapshotAsync(pId, CancellationToken.None).ConfigureAwait(false);
                        await w.CreateItemAsync(name, desc, qty, snap, CancellationToken.None).ConfigureAwait(false);
                        break;
                    case "item":
                        var loc = pType switch
                        {
                            "room" => await w.BuildLocationForRoomAsync(pId, null, CancellationToken.None).ConfigureAwait(false),
                            "container" => await w.BuildLocationForContainerAsync(pId, null, CancellationToken.None).ConfigureAwait(false),
                            "section" => await w.BuildLocationForSectionAsync(pId, null, CancellationToken.None).ConfigureAwait(false),
                            _ => throw new InvalidOperationException("Предмет можно создать из комнаты, контейнера или отделения."),
                        };
                        await w.CreateItemAsync(name, desc, qty, loc, CancellationToken.None).ConfigureAwait(false);
                        break;
                    default:
                        throw new InvalidOperationException("Неизвестный тип записи.");
                }
            }).ConfigureAwait(true);

            if (_refreshAfterSave is not null)
            {
                await _refreshAfterSave().ConfigureAwait(true);
            }

            await DisplayAlertAsync("Готово", "Запись создана.", "Ок").ConfigureAwait(true);
            await CloseModalAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Ошибка сохранения", ex.Message, "Ок").ConfigureAwait(true);
        }
        finally
        {
            Busy.IsRunning = false;
            Busy.IsVisible = false;
        }
    }

    #endregion Обработчики

    #region Приватные методы

    /// <summary>
    /// Загружает типы контейнеров в пикер
    /// </summary>
    private async Task LoadContainerTypesAsync()
    {
        try
        {
            var types = await AppServices.WithLookupAsync(l => l.GetContainerTypesAsync()).ConfigureAwait(true);
            _containerTypeOptionIds.Clear();
            var labels = new List<string> { "Авто (первый тип в справочнике)" };
            _containerTypeOptionIds.Add(null);
            foreach (var t in types)
            {
                labels.Add(t.Label);
                _containerTypeOptionIds.Add(t.Id);
            }

            ContainerTypePicker.ItemsSource = labels;
            ContainerTypePicker.SelectedIndex = 0;
        }
        catch
        {
            _containerTypeOptionIds.Clear();
            _containerTypeOptionIds.Add(null);
            ContainerTypePicker.ItemsSource = new List<string> { "Авто" };
            ContainerTypePicker.SelectedIndex = 0;
        }
    }

    private void BuildContextLabel()
    {
        if (_parent is null)
        {
            ContextLabel.Text = "Без родителя: можно добавить только строение.";
            return;
        }

        if (!GraphNodeIdParser.TryParse(_parent, out var type, out var id))
        {
            ContextLabel.Text = $"Родитель: «{_parent.Label}» — нестандартный идентификатор узла.";
            return;
        }

        ContextLabel.Text = $"Родитель: {HumanType(type)} «{_parent.Label}» (внутренний id {id}).";
    }

    private void RebuildKindPicker()
    {
        _kindOptions.Clear();
        if (_parent is null)
        {
            _kindOptions.Add(new KindOption("Строение", "building"));
        }
        else if (GraphNodeIdParser.TryParse(_parent, out var type, out _))
        {
            switch (type)
            {
                case "building":
                    _kindOptions.Add(new KindOption("Этаж", "floor"));
                    break;
                case "floor":
                    _kindOptions.Add(new KindOption("Комната", "room"));
                    _kindOptions.Add(new KindOption("Контейнер на этаже", "container_floor"));
                    break;
                case "room":
                    _kindOptions.Add(new KindOption("Контейнер в комнате", "container_room"));
                    _kindOptions.Add(new KindOption("Предмет", "item"));
                    break;
                case "container":
                    _kindOptions.Add(new KindOption("Отделение", "section"));
                    _kindOptions.Add(new KindOption("Предмет", "item"));
                    break;
                case "section":
                    _kindOptions.Add(new KindOption("Предмет", "item"));
                    break;
                case "category":
                    _kindOptions.Add(new KindOption("Подкатегория", "category_child"));
                    _kindOptions.Add(new KindOption("Предмет в категории", "item_category"));
                    break;
                case "item":
                    _kindOptions.Add(new KindOption("Ещё предмет с тем же местом", "item_again"));
                    break;
            }
        }

        if (_parent is not null && _kindOptions.Count == 0)
        {
            _kindOptions.Add(new KindOption("Нет действий для этого узла", ""));
        }

        KindPicker.ItemsSource = _kindOptions.Select(k => k.Label).ToList();
    }

    private void ApplyKindUi()
    {
        var key = CurrentKindKey();
        LevelPanel.IsVisible = key == "floor";
        ContainerTypePanel.IsVisible = key is "container_floor" or "container_room";
        QuantityPanel.IsVisible = key is "item" or "item_category" or "item_again";
        if (key != "floor")
        {
            LevelEntry.Text = "1";
        }

        if (!QuantityPanel.IsVisible)
        {
            QuantityEntry.Text = "1";
        }
    }

    private string CurrentKindKey()
    {
        var idx = KindPicker.SelectedIndex;
        if (idx < 0 || idx >= _kindOptions.Count)
        {
            return "";
        }

        return _kindOptions[idx].Key;
    }

    private int? GetSelectedContainerTypeId()
    {
        var i = ContainerTypePicker.SelectedIndex;
        if (i < 0 || i >= _containerTypeOptionIds.Count)
        {
            return null;
        }

        return _containerTypeOptionIds[i];
    }

    private async Task CloseModalAsync()
    {
        if (Navigation.ModalStack.Count > 0)
        {
            await Navigation.PopModalAsync().ConfigureAwait(false);
            return;
        }

        var page = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page?.Navigation.ModalStack.Count > 0)
        {
            await page.Navigation.PopModalAsync().ConfigureAwait(false);
        }
    }

    private static int ParseInt(string? text, int fallback)
    {
        if (int.TryParse(text?.Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var v))
        {
            return v;
        }

        return fallback;
    }

    private static int ParsePositiveInt(string? text, int fallback)
    {
        var v = ParseInt(text, fallback);
        return v < 1 ? 1 : v;
    }

    private static string HumanType(string type) =>
        type switch
        {
            "building" => "Строение",
            "floor" => "Этаж",
            "room" => "Комната",
            "container" => "Контейнер",
            "section" => "Отделение",
            "category" => "Категория",
            "item" => "Предмет",
            _ => type,
        };

    /// <summary>
    /// Вариант типа создаваемой сущности в пикере
    /// </summary>
    /// <param name="Label">Подпись в UI</param>
    /// <param name="Key">Внутренний ключ операции</param>
    private sealed record KindOption(string Label, string Key);

    #endregion Приватные методы
}
