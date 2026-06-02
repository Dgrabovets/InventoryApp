using System.Globalization;
using InventoryApp.Application.Models.Graph;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace InventoryApp.Controls;

/// <summary>
/// Нативная отрисовка <see cref="GraphView"/> через SkiaSharp: раскладка, пан/зум, выбор узла, подсветка связной компоненты, контекстное меню
/// </summary>
public sealed class InventoryGraphSkiaView : SKCanvasView
{
    #region Поля

    /// <summary>
    /// Порядок колонок раскладки по типу узла
    /// </summary>
    private static readonly string[] LayerTypeOrder =
    {
        "building", "floor", "room", "container", "section", "category", "item",
    };

    /// <summary>
    /// Ширина колонки слоя в раскладке
    /// </summary>
    private const float ColumnWidth = 200f;

    /// <summary>
    /// Высота строки узла в раскладке
    /// </summary>
    private const float RowHeight = 44f;

    /// <summary>
    /// Радиус круга узла на холсте
    /// </summary>
    private const float NodeRadius = 16f;

    /// <summary>
    /// Минимальный масштаб вида
    /// </summary>
    private const float MinScale = 0.25f;

    /// <summary>
    /// Максимальный масштаб вида
    /// </summary>
    private const float MaxScale = 4f;

    /// <summary>
    /// Длительность долгого нажатия в миллисекундах
    /// </summary>
    private const float LongPressMs = 480f;

    /// <summary>
    /// Смещение пальца, отменяющее долгое нажатие
    /// </summary>
    private const float MoveCancelLongPressPx = 10f;

    /// <summary>
    /// Кисть рёбер графа
    /// </summary>
    private readonly SKPaint _edgePaint = new()
    {
        Color = SKColors.White.WithAlpha(90),
        StrokeWidth = 1.5f,
        Style = SKPaintStyle.Stroke,
        IsAntialias = true,
    };

    /// <summary>
    /// Кисть затемнённых рёбер при подсветке компоненты
    /// </summary>
    private readonly SKPaint _edgeDimPaint = new()
    {
        Color = SKColors.White.WithAlpha(28),
        StrokeWidth = 1.25f,
        Style = SKPaintStyle.Stroke,
        IsAntialias = true,
    };

    /// <summary>
    /// Кисть подсвеченных рёбер связной компоненты
    /// </summary>
    private readonly SKPaint _edgeHighlightPaint = new()
    {
        Color = new SKColor(147, 197, 253),
        StrokeWidth = 2.25f,
        Style = SKPaintStyle.Stroke,
        IsAntialias = true,
    };

    /// <summary>
    /// Обводка обычного узла
    /// </summary>
    private readonly SKPaint _nodeStrokePaint = new()
    {
        Color = SKColors.White.WithAlpha(200),
        StrokeWidth = 1.25f,
        Style = SKPaintStyle.Stroke,
        IsAntialias = true,
    };

    /// <summary>
    /// Заливка круга узла
    /// </summary>
    private readonly SKPaint _nodeFillPaint = new()
    {
        Style = SKPaintStyle.Fill,
        IsAntialias = true,
    };

    /// <summary>
    /// Обводка узла в подсвеченной связной компоненте
    /// </summary>
    private readonly SKPaint _neighborStrokePaint = new()
    {
        Color = new SKColor(250, 204, 21),
        StrokeWidth = 2.5f,
        Style = SKPaintStyle.Stroke,
        IsAntialias = true,
    };

    /// <summary>
    /// Шрифт подписи рядом с узлом
    /// </summary>
    private readonly SKFont _labelFont = new(SKTypeface.Default, 12f);

    /// <summary>
    /// Кисть текста подписи узла
    /// </summary>
    private readonly SKPaint _textPaint = new()
    {
        Color = SKColors.WhiteSmoke,
        IsAntialias = true,
    };

    /// <summary>
    /// Кисть затемнённой подписи узла
    /// </summary>
    private readonly SKPaint _textDimPaint = new()
    {
        Color = SKColors.WhiteSmoke.WithAlpha(85),
        IsAntialias = true,
    };

    /// <summary>
    /// Обводка выбранного узла
    /// </summary>
    private readonly SKPaint _selStrokePaint = new()
    {
        Color = SKColors.Cyan,
        StrokeWidth = 3f,
        Style = SKPaintStyle.Stroke,
        IsAntialias = true,
    };

    /// <summary>
    /// Текущая модель графа для отрисовки
    /// </summary>
    private GraphView? _graph;

    /// <summary>
    /// Смещение вида по оси X в экранных координатах
    /// </summary>
    private float _panX;

    /// <summary>
    /// Смещение вида по оси Y в экранных координатах
    /// </summary>
    private float _panY;

    /// <summary>
    /// Текущий масштаб вида
    /// </summary>
    private float _scale = 1f;

    /// <summary>
    /// Центр ограничивающего прямоугольника графа по X (мировые координаты)
    /// </summary>
    private float _graphCenterX;

    /// <summary>
    /// Центр ограничивающего прямоугольника графа по Y (мировые координаты)
    /// </summary>
    private float _graphCenterY;

    /// <summary>
    /// При следующей отрисовке центрировать граф в видимой области
    /// </summary>
    private bool _needsViewCentering = true;

    /// <summary>
    /// Идентификаторы узлов подсвеченной связной компоненты от выбранного узла
    /// </summary>
    private HashSet<string>? _highlightIds;

    /// <summary>
    /// Активные точки касания (id касания → экранная позиция)
    /// </summary>
    private readonly Dictionary<long, SKPoint> _activeTouches = new();

    /// <summary>
    /// Экранная точка начала жеста панорамы
    /// </summary>
    private SKPoint _panStartScreen;

    /// <summary>
    /// Значение <see cref="_panX"/> на момент начала панорамы
    /// </summary>
    private float _panStartOffsetX;

    /// <summary>
    /// Значение <see cref="_panY"/> на момент начала панорамы
    /// </summary>
    private float _panStartOffsetY;

    /// <summary>
    /// Расстояние между двумя пальцами в начале щипка
    /// </summary>
    private float _pinchStartDistance;

    /// <summary>
    /// Масштаб в начале жеста щипка
    /// </summary>
    private float _pinchStartScale;

    /// <summary>
    /// Идёт перетаскивание вида одним пальцем
    /// </summary>
    private bool _isPanning;

    /// <summary>
    /// Экранная точка нажатия для тапа и долгого нажатия
    /// </summary>
    private SKPoint _pressScreen;

    /// <summary>
    /// Не обрабатывать короткий тап после панорамы, щипка или ПКМ
    /// </summary>
    private bool _suppressTap;

    /// <summary>
    /// Текущий жест начат правой кнопкой мыши
    /// </summary>
    private bool _rightButtonSession;

    /// <summary>
    /// Отмена таймера долгого нажатия
    /// </summary>
    private CancellationTokenSource? _longPressCts;

    /// <summary>
    /// Текущий выбранный узел
    /// </summary>
    private GraphNode? _selectedNode;

    #endregion Поля

    #region События

    /// <summary>
    /// Короткое касание: узел или null
    /// </summary>
    public event EventHandler<GraphNode?>? NodeTapped;

    /// <summary>
    /// Долгое нажатие или щелчок ПКМ по узлу/фону — контекстное меню на странице
    /// </summary>
    public event EventHandler<GraphNode?>? NodeContextRequested;

    #endregion События

    #region Конструкторы

    /// <summary>
    /// Создаёт холст графа и включает обработку касаний
    /// </summary>
    public InventoryGraphSkiaView()
    {
        EnableTouchEvents = true;
        Touch += OnTouch;
        PaintSurface += OnPaintSurface;
    }

    #endregion Конструкторы

    #region Свойства

    /// <summary>
    /// Текущий выделенный узел
    /// </summary>
    public GraphNode? SelectedNode
    {
        get => _selectedNode;
        private set
        {
            if (ReferenceEquals(_selectedNode, value))
            {
                return;
            }

            _selectedNode = value;
            RecomputeHighlights();
            InvalidateSurface();
        }
    }

    #endregion Свойства

    #region Публичные методы

    /// <summary>
    /// Устанавливает данные графа, выполняет раскладку и сбрасывает вид
    /// </summary>
    /// <param name="graph">Модель графа или null</param>
    public void SetGraph(GraphView? graph)
    {
        CancelLongPress();
        _graph = graph;
        _selectedNode = null;
        _highlightIds = null;
        if (graph != null)
        {
            ApplyLayeredLayout(graph);
            ComputeGraphBounds(graph, out _graphCenterX, out _graphCenterY);
        }

        _needsViewCentering = true;
        _panX = _panY = 0;
        _scale = 1f;
        InvalidateSurface();
    }

    /// <summary>
    /// Снимает выделение и подсветку связей
    /// </summary>
    public void ClearSelection()
    {
        CancelLongPress();
        if (_selectedNode == null && _highlightIds == null)
        {
            return;
        }

        _selectedNode = null;
        _highlightIds = null;
        InvalidateSurface();
        NodeTapped?.Invoke(this, null);
    }

    /// <summary>
    /// Центрирует вид на текущем выбранном узле
    /// </summary>
    public void CenterOnSelectedNode()
    {
        if (_selectedNode == null || _graph == null)
        {
            return;
        }

        var w = CanvasSize.Width;
        var h = CanvasSize.Height;
        if (w <= 0 || h <= 0)
        {
            return;
        }

        var wx = _selectedNode.Position.X;
        var wy = _selectedNode.Position.Y;
        _panX = w * 0.5f - wx * _scale;
        _panY = h * 0.5f - wy * _scale;
        _needsViewCentering = false;
        InvalidateSurface();
    }

    /// <summary>
    /// Увеличивает масштаб относительно центра видимой области
    /// </summary>
    public void ZoomIn() => ApplyZoomStep(1.15f);

    /// <summary>
    /// Уменьшает масштаб относительно центра видимой области
    /// </summary>
    public void ZoomOut() => ApplyZoomStep(1f / 1.15f);

    /// <summary>
    /// Сбрасывает панораму и масштаб и центрирует весь граф
    /// </summary>
    public void ResetView()
    {
        CancelLongPress();
        _scale = 1f;
        _panX = _panY = 0;
        _needsViewCentering = true;
        InvalidateSurface();
    }

    /// <summary>
    /// Масштабирование колёсиком мыши в точке экрана
    /// </summary>
    /// <param name="delta">Приращение масштаба</param>
    /// <param name="screenX">Координата X на экране</param>
    /// <param name="screenY">Координата Y на экране</param>
    public void ApplyWheelDelta(float delta, float screenX, float screenY)
    {
        if (delta == 0 || _graph == null)
        {
            return;
        }

        var oldScale = _scale;
        var factor = delta > 0 ? 1.1f : 1f / 1.1f;
        _scale = Math.Clamp(_scale * factor, MinScale, MaxScale);
        var ratio = _scale / oldScale;
        _panX = screenX - (screenX - _panX) * ratio;
        _panY = screenY - (screenY - _panY) * ratio;
        _needsViewCentering = false;
        InvalidateSurface();
    }

    #endregion Публичные методы

    #region Приватные методы

    /// <summary>
    /// Изменяет масштаб на множитель с ограничением Min/Max
    /// </summary>
    /// <param name="factor">Множитель масштаба</param>
    private void ApplyZoomStep(float factor)
    {
        if (_graph == null)
        {
            return;
        }

        var w = CanvasSize.Width;
        var h = CanvasSize.Height;
        if (w <= 0 || h <= 0)
        {
            return;
        }

        var cx = w * 0.5f;
        var cy = h * 0.5f;
        var oldScale = _scale;
        _scale = Math.Clamp(_scale * factor, MinScale, MaxScale);
        var ratio = _scale / oldScale;
        _panX = cx - (cx - _panX) * ratio;
        _panY = cy - (cy - _panY) * ratio;
        _needsViewCentering = false;
        InvalidateSurface();
    }

    /// <summary>
    /// Пересчитывает множество id узлов связной компоненты от <see cref="_selectedNode"/>
    /// </summary>
    private void RecomputeHighlights()
    {
        _highlightIds = null;
        if (_graph == null || _selectedNode == null)
        {
            return;
        }

        var adj = BuildAdjacencyUndirected(_graph);
        _highlightIds = BfsComponent(adj, _selectedNode.Id);
    }

    /// <summary>
    /// Строит неориентированный список смежности по рёбрам графа
    /// </summary>
    /// <param name="graph">Модель графа</param>
    /// <returns>Словарь «id узла → соседи»</returns>
    private static Dictionary<string, List<string>> BuildAdjacencyUndirected(GraphView graph)
    {
        var adj = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        foreach (var link in graph.Links)
        {
            AddEdge(adj, link.Source, link.Target);
            AddEdge(adj, link.Target, link.Source);
        }

        return adj;
    }

    /// <summary>
    /// Добавляет неориентированное ребро в список смежности
    /// </summary>
    /// <param name="adj">Списки смежности</param>
    /// <param name="a">Первый узел</param>
    /// <param name="b">Второй узел</param>
    private static void AddEdge(Dictionary<string, List<string>> adj, string a, string b)
    {
        if (!adj.TryGetValue(a, out var list))
        {
            list = new List<string>();
            adj[a] = list;
        }

        list.Add(b);
    }

    /// <summary>
    /// Обход в ширину: все узлы связной компоненты, содержащей <paramref name="start"/>
    /// </summary>
    /// <param name="adj">Списки смежности</param>
    /// <param name="start">Стартовый id узла</param>
    /// <returns>Множество id в одной компоненте</returns>
    private static HashSet<string> BfsComponent(Dictionary<string, List<string>> adj, string start)
    {
        var set = new HashSet<string>(StringComparer.Ordinal) { start };
        var q = new Queue<string>();
        q.Enqueue(start);
        while (q.Count > 0)
        {
            var id = q.Dequeue();
            if (!adj.TryGetValue(id, out var nbrs))
            {
                continue;
            }

            foreach (var n in nbrs)
            {
                if (set.Add(n))
                {
                    q.Enqueue(n);
                }
            }
        }

        return set;
    }

    /// <summary>
    /// Раскладывает узлы по колонкам типов (building → item) и строкам внутри колонки
    /// </summary>
    /// <param name="graph">Граф, позиции узлов записываются в <see cref="GraphNode.Position"/></param>
    private static void ApplyLayeredLayout(GraphView graph)
    {
        var layerIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < LayerTypeOrder.Length; i++)
        {
            layerIndex[LayerTypeOrder[i]] = i;
        }

        var counters = new int[LayerTypeOrder.Length];

        foreach (var node in graph.Nodes.OrderBy(n => GetLayer(n.Type, layerIndex)).ThenBy(n => n.Id, StringComparer.Ordinal))
        {
            var layer = GetLayer(node.Type, layerIndex);
            var row = counters[layer]++;
            var x = 80f + layer * ColumnWidth;
            var y = 60f + row * RowHeight;
            node.Position = new GraphVector2(x, y);
        }
    }

    /// <summary>
    /// Индекс колонки слоя для типа узла
    /// </summary>
    /// <param name="type">Тип узла</param>
    /// <param name="layerIndex">Соответствие типа и индекса колонки</param>
    /// <returns>Индекс колонки или значение после последнего известного типа</returns>
    private static int GetLayer(string type, Dictionary<string, int> layerIndex) =>
        layerIndex.TryGetValue(type, out var i) ? i : LayerTypeOrder.Length;

    /// <summary>
    /// Вычисляет центр ограничивающего прямоугольника всех узлов
    /// </summary>
    /// <param name="graph">Граф с заданными позициями</param>
    /// <param name="cx">Центр по X</param>
    /// <param name="cy">Центр по Y</param>
    private static void ComputeGraphBounds(GraphView graph, out float cx, out float cy)
    {
        if (graph.Nodes.Count == 0)
        {
            cx = cy = 0;
            return;
        }

        float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
        foreach (var n in graph.Nodes)
        {
            var x = n.Position.X;
            var y = n.Position.Y;
            minX = Math.Min(minX, x - NodeRadius);
            maxX = Math.Max(maxX, x + NodeRadius + 120f);
            minY = Math.Min(minY, y - NodeRadius);
            maxY = Math.Max(maxY, y + NodeRadius);
        }

        cx = (minX + maxX) * 0.5f;
        cy = (minY + maxY) * 0.5f;
    }

    #endregion Приватные методы

    #region Обработчики

    /// <summary>
    /// Отрисовка графа на Skia-холсте
    /// </summary>
    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(new SKColor(18, 18, 18));

        if (_graph == null || _graph.Nodes.Count == 0)
        {
            return;
        }

        var w = e.Info.Width;
        var h = e.Info.Height;

        if (_needsViewCentering && w > 0 && h > 0)
        {
            _panX = w * 0.5f - _graphCenterX * _scale;
            _panY = h * 0.5f - _graphCenterY * _scale;
            _needsViewCentering = false;
        }

        var positions = _graph.Nodes.ToDictionary(n => n.Id, n => new SKPoint(n.Position.X, n.Position.Y));
        var hasFocus = _highlightIds != null && _highlightIds.Count > 0;

        canvas.Save();
        canvas.Translate(_panX, _panY);
        canvas.Scale(_scale);

        foreach (var link in _graph.Links)
        {
            if (!positions.TryGetValue(link.Source, out var s) || !positions.TryGetValue(link.Target, out var t))
            {
                continue;
            }

            var hi = hasFocus &&
                     _highlightIds!.Contains(link.Source) &&
                     _highlightIds.Contains(link.Target);
            canvas.DrawLine(s, t, hi ? _edgeHighlightPaint : hasFocus ? _edgeDimPaint : _edgePaint);
        }

        foreach (var node in _graph.Nodes)
        {
            var center = positions[node.Id];
            var dim = hasFocus && !_highlightIds!.Contains(node.Id);
            var baseColor = ParseHexColor(node.Color);
            _nodeFillPaint.Color = dim ? baseColor.WithAlpha(70) : baseColor;

            canvas.DrawCircle(center, NodeRadius, _nodeFillPaint);

            if (ReferenceEquals(node, _selectedNode))
            {
                canvas.DrawCircle(center, NodeRadius, _selStrokePaint);
            }
            else if (hasFocus && _highlightIds!.Contains(node.Id))
            {
                canvas.DrawCircle(center, NodeRadius, _neighborStrokePaint);
            }
            else
            {
                canvas.DrawCircle(center, NodeRadius, dim ? _edgeDimPaint : _nodeStrokePaint);
            }

            var label = TruncateLabel(node.Label, 22);
            var tp = dim ? _textDimPaint : _textPaint;
            canvas.DrawText(label, center.X + NodeRadius + 6f, center.Y + 4f, SKTextAlign.Left, _labelFont, tp);
        }

        canvas.Restore();
    }

    /// <summary>
    /// Обработка касаний: панорама, щипок, тап, долгое нажатие, колёсико, ПКМ
    /// </summary>
    private void OnTouch(object? sender, SKTouchEventArgs e)
    {
        switch (e.ActionType)
        {
            case SKTouchAction.Pressed:
                _activeTouches[e.Id] = e.Location;
                if (e.MouseButton == SKMouseButton.Right)
                {
                    _rightButtonSession = true;
                    _pressScreen = e.Location;
                    _suppressTap = true;
                    CancelLongPress();
                    break;
                }

                if (_activeTouches.Count == 1)
                {
                    _panStartScreen = e.Location;
                    _panStartOffsetX = _panX;
                    _panStartOffsetY = _panY;
                    _pressScreen = e.Location;
                    _suppressTap = false;
                    _isPanning = false;
                    _rightButtonSession = false;
                    ScheduleLongPress(e.Location);
                }
                else if (_activeTouches.Count == 2)
                {
                    CancelLongPress();
                    _pinchStartDistance = GetPinchDistance(_activeTouches);
                    _pinchStartScale = _scale;
                    _suppressTap = true;
                }

                break;

            case SKTouchAction.WheelChanged:
                if (e.WheelDelta != 0)
                {
                    ApplyWheelDelta(e.WheelDelta, e.Location.X, e.Location.Y);
                }

                break;

            case SKTouchAction.Moved:
                if (_activeTouches.ContainsKey(e.Id))
                {
                    _activeTouches[e.Id] = e.Location;
                }

                if (_activeTouches.Count == 1)
                {
                    var cur = _activeTouches.Values.First();
                    var moved = Math.Abs(cur.X - _panStartScreen.X) + Math.Abs(cur.Y - _panStartScreen.Y);
                    if (moved > MoveCancelLongPressPx)
                    {
                        CancelLongPress();
                    }
                }

                if (_activeTouches.Count == 1 && _activeTouches.TryGetValue(e.Id, out var p1))
                {
                    var dx = p1.X - _panStartScreen.X;
                    var dy = p1.Y - _panStartScreen.Y;
                    if (Math.Abs(dx) + Math.Abs(dy) > 6f)
                    {
                        _isPanning = true;
                    }

                    _panX = _panStartOffsetX + dx;
                    _panY = _panStartOffsetY + dy;
                    _needsViewCentering = false;
                    InvalidateSurface();
                }
                else if (_activeTouches.Count == 2)
                {
                    var d = GetPinchDistance(_activeTouches);
                    if (d > 1f && _pinchStartDistance > 1f)
                    {
                        var newScale = Math.Clamp(_pinchStartScale * (d / _pinchStartDistance), MinScale, MaxScale);
                        var keys = _activeTouches.Keys.Take(2).ToArray();
                        var c = Midpoint(_activeTouches[keys[0]], _activeTouches[keys[1]]);
                        var oldScale = _scale;
                        _scale = newScale;
                        var ratio = _scale / oldScale;
                        _panX = c.X - (c.X - _panX) * ratio;
                        _panY = c.Y - (c.Y - _panY) * ratio;
                        _needsViewCentering = false;
                        InvalidateSurface();
                    }
                }

                break;

            case SKTouchAction.Released:
            {
                var countBefore = _activeTouches.Count;
                if (_rightButtonSession)
                {
                    _activeTouches.Remove(e.Id);
                    _rightButtonSession = false;
                    var hit = HitTestScreen(_pressScreen);
                    if (hit != null)
                    {
                        SelectedNode = hit;
                    }

                    NodeContextRequested?.Invoke(this, hit);
                    if (hit != null)
                    {
                        NodeTapped?.Invoke(this, hit);
                    }

                    _suppressTap = false;
                    if (_activeTouches.Count == 0)
                    {
                        _isPanning = false;
                    }

                    break;
                }

                _activeTouches.Remove(e.Id);
                CancelLongPress();
                if (countBefore == 1 && !_suppressTap && !_isPanning)
                {
                    TrySelectNodeAtScreen(_pressScreen);
                }

                if (_activeTouches.Count < 2)
                {
                    _pinchStartDistance = 0;
                }

                if (_activeTouches.Count == 0)
                {
                    _suppressTap = false;
                    _isPanning = false;
                }

                break;
            }

            case SKTouchAction.Cancelled:
                CancelLongPress();
                _rightButtonSession = false;
                _activeTouches.Remove(e.Id);
                if (_activeTouches.Count < 2)
                {
                    _pinchStartDistance = 0;
                }

                if (_activeTouches.Count == 0)
                {
                    _suppressTap = false;
                    _isPanning = false;
                }

                break;
        }

        e.Handled = true;
    }

    /// <summary>
    /// Запускает таймер долгого нажатия в экранной точке
    /// </summary>
    /// <param name="screen">Позиция нажатия на холсте</param>
    private void ScheduleLongPress(SKPoint screen)
    {
        CancelLongPress();
        _longPressCts = new CancellationTokenSource();
        var token = _longPressCts.Token;
        var loc = screen;
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(TimeSpan.FromMilliseconds(LongPressMs), token).ConfigureAwait(false);
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    if (token.IsCancellationRequested || _isPanning || _suppressTap || _rightButtonSession)
                    {
                        return;
                    }

                    var hit = HitTestScreen(loc);
                    if (hit != null)
                    {
                        SelectedNode = hit;
                    }

                    NodeContextRequested?.Invoke(this, hit);
                    if (hit != null)
                    {
                        NodeTapped?.Invoke(this, hit);
                    }
                }).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
        }, token);
    }

    /// <summary>
    /// Отменяет ожидающее долгое нажатие
    /// </summary>
    private void CancelLongPress()
    {
        if (_longPressCts == null)
        {
            return;
        }

        try
        {
            _longPressCts.Cancel();
            _longPressCts.Dispose();
        }
        catch
        {
            // ignored
        }

        _longPressCts = null;
    }

    /// <summary>
    /// Выбирает узел под точкой экрана и вызывает <see cref="NodeTapped"/>
    /// </summary>
    /// <param name="screen">Экранные координаты</param>
    private void TrySelectNodeAtScreen(SKPoint screen)
    {
        var hit = HitTestScreen(screen);
        SelectedNode = hit;
        NodeTapped?.Invoke(this, hit);
    }

    /// <summary>
    /// Ищет ближайший узел в радиусе попадания под экранной точкой
    /// </summary>
    /// <param name="screen">Экранные координаты</param>
    /// <returns>Узел или null</returns>
    private GraphNode? HitTestScreen(SKPoint screen)
    {
        if (_graph == null)
        {
            return null;
        }

        var wx = (screen.X - _panX) / _scale;
        var wy = (screen.Y - _panY) / _scale;
        const float hitPad = 4f;
        GraphNode? best = null;
        var bestD = float.MaxValue;

        foreach (var node in _graph.Nodes)
        {
            var dx = node.Position.X - wx;
            var dy = node.Position.Y - wy;
            var d = MathF.Sqrt(dx * dx + dy * dy);
            if (d <= NodeRadius + hitPad && d < bestD)
            {
                bestD = d;
                best = node;
            }
        }

        return best;
    }

    /// <summary>
    /// Расстояние между первыми двумя активными касаниями
    /// </summary>
    /// <param name="touches">Активные касания</param>
    /// <returns>Расстояние в пикселях или 0</returns>
    private static float GetPinchDistance(IReadOnlyDictionary<long, SKPoint> touches)
    {
        if (touches.Count < 2)
        {
            return 0;
        }

        var pts = touches.Values.Take(2).ToArray();
        var dx = pts[1].X - pts[0].X;
        var dy = pts[1].Y - pts[0].Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>
    /// Середина отрезка между двумя точками
    /// </summary>
    /// <param name="a">Первая точка</param>
    /// <param name="b">Вторая точка</param>
    /// <returns>Центр отрезка</returns>
    private static SKPoint Midpoint(SKPoint a, SKPoint b) => new((a.X + b.X) * 0.5f, (a.Y + b.Y) * 0.5f);

    /// <summary>
    /// Парсит цвет узла из строки #RRGGBB
    /// </summary>
    /// <param name="hex">Шестнадцатеричный цвет с # или без</param>
    /// <returns>Цвет Skia или серый при ошибке</returns>
    private static SKColor ParseHexColor(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            return SKColors.Gray;
        }

        var s = hex.Trim();
        if (s.StartsWith('#'))
        {
            s = s[1..];
        }

        try
        {
            if (s.Length == 6 &&
                uint.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var rgb))
            {
                return new SKColor((byte)((rgb >> 16) & 255), (byte)((rgb >> 8) & 255), (byte)(rgb & 255));
            }
        }
        catch
        {
            // ignored
        }

        return SKColors.Gray;
    }

    /// <summary>
    /// Обрезает подпись узла с многоточием
    /// </summary>
    /// <param name="label">Исходный текст</param>
    /// <param name="maxChars">Максимальная длина</param>
    /// <returns>Обрезанная строка</returns>
    private static string TruncateLabel(string label, int maxChars)
    {
        if (string.IsNullOrEmpty(label) || label.Length <= maxChars)
        {
            return label;
        }

        return string.Concat(label.AsSpan(0, maxChars - 1), "…");
    }

    #endregion Обработчики
}
