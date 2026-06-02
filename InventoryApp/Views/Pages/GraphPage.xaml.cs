using System.Text;
using System.Text.Json;
using InventoryApp.Application.Models.Graph;

namespace InventoryApp.Views.Pages;

/// <summary>
/// Экран графа: загрузка <see cref="GraphView"/> через <see cref="IGraphService"/>,
/// фильтр по типу узла, отображение через <see cref="Controls.InventoryGraphSkiaView"/>, контекстное меню
/// </summary>
public partial class GraphPage : ContentPage
{
    #region Поля

    /// <summary>
    /// Строка поиска по графу
    /// </summary>
    private string? _searchFilter;

    /// <summary>
    /// Токен отмены debounce поиска
    /// </summary>
    private CancellationTokenSource? _searchDebounceCts;

    /// <summary>
    /// Полный граф с сервера до фильтра по типу узла
    /// </summary>
    private GraphView? _fullGraph;

    /// <summary>
    /// Ключ типа узла для фильтра или null для всех типов
    /// </summary>
    private string? _typeFilterKey;
    /// <summary>
    /// Подавление обработчика фильтра типов при инициализации Picker
    /// </summary>
    private bool _suppressTypeFilterEvent;

    #endregion Поля

    #region Конструкторы

    /// <summary>
    /// Создаёт страницу графа и подписывается на события холста
    /// </summary>
    public GraphPage()
    {
        InitializeComponent();
        TypeFilterPicker.ItemsSource = new[]
        {
            "Все типы",
            "Строения",
            "Этажи",
            "Комнаты",
            "Контейнеры",
            "Отделения",
            "Категории",
            "Предметы",
        };
        _suppressTypeFilterEvent = true;
        TypeFilterPicker.SelectedIndex = 0;
        _suppressTypeFilterEvent = false;
        GraphCanvas.NodeTapped += OnGraphNodeTapped;
        GraphCanvas.NodeContextRequested += OnGraphContextRequested;
    }

    #endregion Конструкторы

    #region Публичные методы

    /// <inheritdoc />
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = LoadGraphAsync();
    }

    #endregion Публичные методы

    #region Обработчики

    /// <summary>
    /// Обновление по pull-to-refresh
    /// </summary>
    private async void OnRefreshing(object? sender, EventArgs e)
    {
        await LoadGraphAsync();
        Refresh.IsRefreshing = false;
    }

    /// <summary>
    /// Фильтр отображаемых типов узлов (клон графа для холста, исходные данные не меняются).
    /// </summary>
    private void OnTypeFilterChanged(object? sender, EventArgs e)
    {
        if (_suppressTypeFilterEvent || TypeFilterPicker.SelectedItem is not string label)
        {
            return;
        }

        _typeFilterKey = MapPickerToTypeKey(label);
        ApplyGraphToCanvas();
    }

    /// <summary>
    /// Отложенное обновление при вводе в строку поиска (debounce ~400 мс).
    /// </summary>
    private async void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        _searchDebounceCts?.Cancel();
        _searchDebounceCts?.Dispose();
        var cts = new CancellationTokenSource();
        _searchDebounceCts = cts;
        try
        {
            await Task.Delay(400, cts.Token).ConfigureAwait(true);
            var text = SearchBar.Text;
            _searchFilter = string.IsNullOrWhiteSpace(text) ? null : text;
            await LoadGraphAsync(cts.Token).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
    }

    /// <summary>
    /// Немедленное обновление по кнопке поиска на клавиатуре.
    /// </summary>
    private void OnSearchPressed(object? sender, EventArgs e)
    {
        _searchFilter = string.IsNullOrWhiteSpace(SearchBar.Text) ? null : SearchBar.Text;
        _ = LoadGraphAsync();
    }

    /// <summary>
    /// Увеличивает масштаб графа.
    /// </summary>
    private void OnZoomInClicked(object? sender, EventArgs e) => GraphCanvas.ZoomIn();

    /// <summary>
    /// Уменьшает масштаб графа.
    /// </summary>
    private void OnZoomOutClicked(object? sender, EventArgs e) => GraphCanvas.ZoomOut();

    /// <summary>
    /// Центрирует граф и сбрасывает масштаб.
    /// </summary>
    private void OnResetViewClicked(object? sender, EventArgs e) => GraphCanvas.ResetView();

    /// <summary>
    /// Контекстное меню: долгое нажатие или щелчок ПКМ по холсту.
    /// </summary>
    private async void OnGraphContextRequested(object? sender, GraphNode? node)
    {
        string? choice;
        if (node is null)
        {
            choice = await DisplayActionSheetAsync(
                    "Граф",
                    "Отмена",
                    null,
                    FlowDirection.MatchParent,
                    "Создать строение…",
                    "Обновить граф",
                    "Сброс вида")
                .ConfigureAwait(true);
        }
        else
        {
            var isItem = GraphNodeIdParser.TryParse(node, out var nodeType, out _) && nodeType == "item";
            if (isItem)
            {
                choice = await DisplayActionSheetAsync(
                        "Узел",
                        "Отмена",
                        null,
                        FlowDirection.MatchParent,
                        "Открыть карточку предмета",
                        "Центрировать на узле",
                        "Снять выделение",
                        "Создать дочерний объект…",
                        "Обновить граф",
                        "Сброс вида")
                    .ConfigureAwait(true);
            }
            else
            {
                choice = await DisplayActionSheetAsync(
                        "Узел",
                        "Отмена",
                        null,
                        FlowDirection.MatchParent,
                        "Центрировать на узле",
                        "Снять выделение",
                        "Создать дочерний объект…",
                        "Обновить граф",
                        "Сброс вида")
                    .ConfigureAwait(true);
            }
        }

        if (string.IsNullOrEmpty(choice) || choice == "Отмена")
        {
            return;
        }

        switch (choice)
        {
            case "Центрировать на узле":
                GraphCanvas.CenterOnSelectedNode();
                break;
            case "Снять выделение":
                GraphCanvas.ClearSelection();
                break;
            case "Обновить граф":
                await LoadGraphAsync().ConfigureAwait(true);
                break;
            case "Сброс вида":
                GraphCanvas.ResetView();
                break;
            case "Создать строение…":
                await PushCreateEntityModalAsync(null).ConfigureAwait(true);
                break;
            case "Создать дочерний объект…":
                await PushCreateEntityModalAsync(node).ConfigureAwait(true);
                break;
            case "Открыть карточку предмета":
                if (node is not null && GraphNodeIdParser.TryParse(node, out _, out var itemEntityId))
                {
                    await Navigation.PushAsync(new ItemDetailPage(itemEntityId, () => LoadGraphAsync())).ConfigureAwait(true);
                }

                break;
        }
    }

    /// <summary>
    /// Открывает форму создания сущности относительно узла (или корневого строения).
    /// </summary>
    private async Task PushCreateEntityModalAsync(GraphNode? parentNode)
    {
        var page = new CreateEntityFromGraphPage(parentNode, () => LoadGraphAsync());
        await Navigation.PushModalAsync(page).ConfigureAwait(true);
    }

    /// <summary>
    /// Показывает краткую строку и многострочные сведения (включая JSON <see cref="GraphNode.Data"/>).
    /// </summary>
    private void OnGraphNodeTapped(object? sender, GraphNode? node)
    {
        if (node is null)
        {
            SelectionLabel.Text = "Узел не выбран. Коснитесь круга на графе.";
            NodeDetailEditor.Text = string.Empty;
            return;
        }

        SelectionLabel.Text = $"{node.Type}: {node.Label} · id {node.Id}";
        NodeDetailEditor.Text = BuildNodeDetail(node);
    }

    #endregion Обработчики

    #region Приватные методы

    /// <summary>
    /// Загружает полный граф с сервиса и применяет фильтр типа для холста
    /// </summary>
    /// <param name="cancellationToken">Токен отмены</param>
    private async Task LoadGraphAsync(CancellationToken cancellationToken = default)
    {
        Loading.IsVisible = Loading.IsRunning = true;
        try
        {
            var graph = await AppServices.WithGraphAsync(s => s.BuildInventoryGraphAsync(null, _searchFilter, cancellationToken))
                .ConfigureAwait(true);
            _fullGraph = graph;
            ApplyGraphToCanvas();
        }
        finally
        {
            Loading.IsRunning = false;
            Loading.IsVisible = false;
        }
    }

    /// <summary>
    /// Строит копию графа для отрисовки (раскладка меняет только копии узлов).
    /// </summary>
    private void ApplyGraphToCanvas()
    {
        if (_fullGraph is null)
        {
            NodesCountLabel.Text = "—";
            LinksCountLabel.Text = "—";
            GraphCanvas.SetGraph(null);
            SelectionLabel.Text = "Нет данных графа.";
            NodeDetailEditor.Text = string.Empty;
            return;
        }

        var view = BuildViewGraph(_fullGraph, _typeFilterKey);
        NodesCountLabel.Text = view.Nodes.Count.ToString();
        LinksCountLabel.Text = view.Links.Count.ToString();
        GraphCanvas.SetGraph(view);
        SelectionLabel.Text = view.Nodes.Count == 0
            ? "Нет узлов для отображения (проверьте фильтры или добавьте данные)."
            : "Узел не выбран. Коснитесь круга на графе.";
        NodeDetailEditor.Text = string.Empty;
    }

    private static GraphView BuildViewGraph(GraphView source, string? typeKey)
    {
        IEnumerable<GraphNode> nodes = source.Nodes;
        if (!string.IsNullOrEmpty(typeKey))
        {
            nodes = nodes.Where(n => string.Equals(n.Type, typeKey, StringComparison.OrdinalIgnoreCase));
        }

        var list = nodes
            .Select(n => new GraphNode
            {
                Id = n.Id,
                Label = n.Label,
                Type = n.Type,
                Color = n.Color,
                Data = n.Data,
                Position = default,
            })
            .ToList();

        var ids = list.Select(n => n.Id).ToHashSet(StringComparer.Ordinal);
        var links = source.Links
            .Where(l => ids.Contains(l.Source) && ids.Contains(l.Target))
            .Select(l => new GraphLink
            {
                Source = l.Source,
                Target = l.Target,
                Label = l.Label,
                Type = l.Type,
            })
            .ToList();

        return new GraphView { Nodes = list, Links = links };
    }

    private static string? MapPickerToTypeKey(string label) =>
        label switch
        {
            "Все типы" => null,
            "Строения" => "building",
            "Этажи" => "floor",
            "Комнаты" => "room",
            "Контейнеры" => "container",
            "Отделения" => "section",
            "Категории" => "category",
            "Предметы" => "item",
            _ => null,
        };

    private static string BuildNodeDetail(GraphNode node)
    {
        var sb = new StringBuilder();
        sb.AppendLine(CultureInv($"Тип: {node.Type}"));
        sb.AppendLine(CultureInv($"Подпись: {node.Label}"));
        sb.AppendLine(CultureInv($"Идентификатор: {node.Id}"));
        sb.AppendLine(CultureInv($"Цвет: {node.Color}"));
        if (node.Data is not null)
        {
            sb.AppendLine();
            sb.AppendLine("Дополнительные данные (JSON):");
            try
            {
                var json = JsonSerializer.Serialize(node.Data, new JsonSerializerOptions { WriteIndented = true });
                sb.Append(json);
            }
            catch
            {
                sb.Append(node.Data.ToString());
            }
        }

        return sb.ToString().TrimEnd();
    }

    private static string CultureInv(FormattableString s) => FormattableString.Invariant(s);

    #endregion Приватные методы
}
