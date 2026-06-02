using InventoryApp.Application.Models.Graph;
using InventoryApp.Application.Services.Graph;
using InventoryApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryApp.Infrastructure.Services.Graph;

/// <inheritdoc cref="IGraphService" />
public sealed class GraphService : IGraphService
{
    #region Поля

    /// <summary>
    /// Контекст базы данных
    /// </summary>
    private readonly ApplicationContext _context;

    /// <summary>
    /// Журнал построения графа
    /// </summary>
    private readonly ILogger<GraphService> _logger;

    #endregion Поля

    #region Конструкторы

    /// <summary>
    /// Создаёт сервис графа инвентаря
    /// </summary>
    /// <param name="context">Контекст SQLite</param>
    /// <param name="logger">Журнал операций</param>
    public GraphService(ApplicationContext context, ILogger<GraphService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #endregion Конструкторы

    #region Публичные методы

    /// <inheritdoc />
    public async Task<GraphView> BuildInventoryGraphAsync(int? userId = null, string? search = null, CancellationToken cancellationToken = default)
    {
        var graph = new GraphView();

        try
        {
            var addedNodes = new HashSet<string>();
            var addedLinks = new HashSet<string>();

            var buildings = await _context.Buildings
                .Where(b => b.IsActive)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var building in buildings)
            {
                var buildingNodeId = $"building_{building.Id}";
                if (!addedNodes.Contains(buildingNodeId))
                {
                    graph.Nodes.Add(new GraphNode
                    {
                        Id = buildingNodeId,
                        Label = building.Name,
                        Type = "building",
                        Color = "#ef4444",
                        Data = new { building.Id, building.Name, building.Description },
                    });
                    addedNodes.Add(buildingNodeId);
                }
            }

            var floors = await _context.Floors
                .Include(f => f.Building)
                .Where(f => f.IsActive)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var floor in floors)
            {
                var floorNodeId = $"floor_{floor.Id}";
                if (!addedNodes.Contains(floorNodeId))
                {
                    graph.Nodes.Add(new GraphNode
                    {
                        Id = floorNodeId,
                        Label = $"Этаж {floor.Level}",
                        Type = "floor",
                        Color = "#8b5cf6",
                        Data = new { floor.Id, floor.Level, floor.Name, floor.Description },
                    });
                    addedNodes.Add(floorNodeId);
                }

                if (floor.Building != null)
                {
                    var buildingNodeId = $"building_{floor.Building.Id}";
                    var linkKey = $"{floorNodeId}-{buildingNodeId}";
                    if (!addedLinks.Contains(linkKey))
                    {
                        graph.Links.Add(new GraphLink
                        {
                            Source = floorNodeId,
                            Target = buildingNodeId,
                            Label = "in building",
                            Type = "hierarchy",
                        });
                        addedLinks.Add(linkKey);
                    }
                }
            }

            var rooms = await _context.Rooms
                .Include(r => r.Floor).ThenInclude(f => f!.Building)
                .Where(r => r.IsActive)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var room in rooms)
            {
                var roomNodeId = $"room_{room.Id}";
                if (!addedNodes.Contains(roomNodeId))
                {
                    graph.Nodes.Add(new GraphNode
                    {
                        Id = roomNodeId,
                        Label = room.Name,
                        Type = "room",
                        Color = "#f59e0b",
                        Data = new { room.Id, room.Name, room.Description },
                    });
                    addedNodes.Add(roomNodeId);
                }

                if (room.Floor != null)
                {
                    var floorNodeId = $"floor_{room.Floor.Id}";
                    var linkKey = $"{roomNodeId}-{floorNodeId}";
                    if (!addedLinks.Contains(linkKey))
                    {
                        graph.Links.Add(new GraphLink
                        {
                            Source = roomNodeId,
                            Target = floorNodeId,
                            Label = "on floor",
                            Type = "hierarchy",
                        });
                        addedLinks.Add(linkKey);
                    }
                }
            }

            var containers = await _context.Containers
                .Include(c => c.Room).ThenInclude(r => r!.Floor).ThenInclude(f => f!.Building)
                .Include(c => c.Floor).ThenInclude(f => f!.Building)
                .Where(c => c.IsActive)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var container in containers)
            {
                var containerNodeId = $"container_{container.Id}";
                if (!addedNodes.Contains(containerNodeId))
                {
                    graph.Nodes.Add(new GraphNode
                    {
                        Id = containerNodeId,
                        Label = container.Name,
                        Type = "container",
                        Color = "#06b6d4",
                        Data = new { container.Id, container.Name, container.Description },
                    });
                    addedNodes.Add(containerNodeId);
                }

                if (container.RoomId.HasValue && container.Room != null)
                {
                    var roomNodeId = $"room_{container.Room.Id}";
                    var linkKey = $"{containerNodeId}-{roomNodeId}";
                    if (!addedLinks.Contains(linkKey))
                    {
                        graph.Links.Add(new GraphLink
                        {
                            Source = containerNodeId,
                            Target = roomNodeId,
                            Label = "in room",
                            Type = "hierarchy",
                        });
                        addedLinks.Add(linkKey);
                    }
                }
                else if (container.FloorId.HasValue && container.Floor != null)
                {
                    var floorNodeId = $"floor_{container.Floor.Id}";
                    var linkKey = $"{containerNodeId}-{floorNodeId}";
                    if (!addedLinks.Contains(linkKey))
                    {
                        graph.Links.Add(new GraphLink
                        {
                            Source = containerNodeId,
                            Target = floorNodeId,
                            Label = "on floor",
                            Type = "hierarchy",
                        });
                        addedLinks.Add(linkKey);
                    }
                }
            }

            var sections = await _context.Sections
                .Include(s => s.Container).ThenInclude(c => c!.Room).ThenInclude(r => r!.Floor).ThenInclude(f => f!.Building)
                .Include(s => s.Container).ThenInclude(c => c!.Floor).ThenInclude(f => f!.Building)
                .Where(s => s.IsActive)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var section in sections)
            {
                var sectionNodeId = $"section_{section.Id}";
                if (!addedNodes.Contains(sectionNodeId))
                {
                    graph.Nodes.Add(new GraphNode
                    {
                        Id = sectionNodeId,
                        Label = section.Name,
                        Type = "section",
                        Color = "#d946ef",
                        Data = new { section.Id, section.Name, section.Description },
                    });
                    addedNodes.Add(sectionNodeId);
                }

                if (section.Container != null)
                {
                    var containerNodeId = $"container_{section.Container.Id}";
                    var linkKey = $"{sectionNodeId}-{containerNodeId}";
                    if (!addedLinks.Contains(linkKey))
                    {
                        graph.Links.Add(new GraphLink
                        {
                            Source = sectionNodeId,
                            Target = containerNodeId,
                            Label = "in container",
                            Type = "hierarchy",
                        });
                        addedLinks.Add(linkKey);
                    }
                }
            }

            var itemsQuery = _context.Items
                .Include(i => i.Category)
                .Include(i => i.Room).ThenInclude(r => r!.Floor).ThenInclude(f => f!.Building)
                .Include(i => i.Container)
                .Include(i => i.Section)
                .Include(i => i.ItemImages)
                .Where(i => i.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLowerInvariant();
                itemsQuery = itemsQuery.Where(i =>
                    i.Name.ToLower().Contains(term) ||
                    (i.Description != null && i.Description.ToLower().Contains(term)) ||
                    (i.LocationNotes != null && i.LocationNotes.ToLower().Contains(term)));
            }

            var items = await itemsQuery.ToListAsync(cancellationToken).ConfigureAwait(false);

            var categoryLocationMap = new Dictionary<int, (string? LocationType, int? LocationId)>();
            foreach (var item in items)
            {
                if (item.CategoryId.HasValue)
                {
                    string? locationType = null;
                    int? locationId = null;

                    if (item.SectionId.HasValue && item.Section != null)
                    {
                        locationType = "section";
                        locationId = item.Section.Id;
                    }
                    else if (item.ContainerId.HasValue && item.Container != null)
                    {
                        locationType = "container";
                        locationId = item.Container.Id;
                    }
                    else if (item.RoomId.HasValue && item.Room != null)
                    {
                        locationType = "room";
                        locationId = item.Room.Id;
                    }

                    if (locationType != null && locationId.HasValue &&
                        !categoryLocationMap.ContainsKey(item.CategoryId.Value))
                    {
                        categoryLocationMap[item.CategoryId.Value] = (locationType, locationId);
                    }
                }
            }

            var categoriesWithItems = items
                .Where(i => i.CategoryId.HasValue)
                .Select(i => i.CategoryId!.Value)
                .Distinct()
                .ToHashSet();

            var categories = await _context.Categories
                .Include(c => c.ParentCategory)
                .Where(c => c.IsActive && categoriesWithItems.Contains(c.Id))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var category in categories)
            {
                var categoryNodeId = $"category_{category.Id}";
                if (!addedNodes.Contains(categoryNodeId))
                {
                    graph.Nodes.Add(new GraphNode
                    {
                        Id = categoryNodeId,
                        Label = category.Name,
                        Type = "category",
                        Color = "#10b981",
                        Data = new { category.Id, category.Name, category.Description },
                    });
                    addedNodes.Add(categoryNodeId);
                }

                if (categoryLocationMap.TryGetValue(category.Id, out var locationInfo))
                {
                    string? locationNodeId = null;
                    if (locationInfo.LocationType == "section" && locationInfo.LocationId.HasValue)
                    {
                        locationNodeId = $"section_{locationInfo.LocationId.Value}";
                    }
                    else if (locationInfo.LocationType == "container" && locationInfo.LocationId.HasValue)
                    {
                        locationNodeId = $"container_{locationInfo.LocationId.Value}";
                    }
                    else if (locationInfo.LocationType == "room" && locationInfo.LocationId.HasValue)
                    {
                        locationNodeId = $"room_{locationInfo.LocationId.Value}";
                    }

                    if (locationNodeId != null)
                    {
                        var linkKey = $"{locationNodeId}-{categoryNodeId}";
                        if (!addedLinks.Contains(linkKey))
                        {
                            graph.Links.Add(new GraphLink
                            {
                                Source = locationNodeId,
                                Target = categoryNodeId,
                                Label = "has category",
                                Type = "hierarchy",
                            });
                            addedLinks.Add(linkKey);
                        }
                    }
                }

                if (category.ParentCategoryId.HasValue && category.ParentCategory != null &&
                    categoriesWithItems.Contains(category.ParentCategory.Id))
                {
                    var parentCategoryNodeId = $"category_{category.ParentCategory.Id}";
                    var linkKey = $"{categoryNodeId}-{parentCategoryNodeId}";
                    if (!addedLinks.Contains(linkKey))
                    {
                        graph.Links.Add(new GraphLink
                        {
                            Source = categoryNodeId,
                            Target = parentCategoryNodeId,
                            Label = "parent",
                            Type = "hierarchy",
                        });
                        addedLinks.Add(linkKey);
                    }
                }
            }

            foreach (var item in items)
            {
                var itemNodeId = $"item_{item.Id}";
                if (!addedNodes.Contains(itemNodeId))
                {
                    graph.Nodes.Add(new GraphNode
                    {
                        Id = itemNodeId,
                        Label = item.Name,
                        Type = "item",
                        Color = GetColorForCategory(),
                        Data = new
                        {
                            item.Id,
                            item.Name,
                            item.Description,
                            item.Quantity,
                            Category = item.Category?.Name,
                            Room = item.Room?.Name,
                            Container = item.Container?.Name,
                            Section = item.Section?.Name,
                            HasImage = item.ItemImages.Any(),
                        },
                    });
                    addedNodes.Add(itemNodeId);
                }

                if (item.CategoryId.HasValue && item.Category != null)
                {
                    var categoryNodeId = $"category_{item.Category.Id}";
                    var linkKey = $"{categoryNodeId}-{itemNodeId}";
                    if (!addedLinks.Contains(linkKey))
                    {
                        graph.Links.Add(new GraphLink
                        {
                            Source = categoryNodeId,
                            Target = itemNodeId,
                            Label = "contains",
                            Type = "hierarchy",
                        });
                        addedLinks.Add(linkKey);
                    }
                }
            }

            _logger.LogInformation(
                "Graph built with {NodeCount} nodes and {LinkCount} links",
                graph.Nodes.Count,
                graph.Links.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build inventory graph for user {UserId}", userId);
            throw;
        }

        return graph;
    }

    /// <inheritdoc />
    public async Task<GraphView> GetItemGraphAsync(int itemId, CancellationToken cancellationToken = default)
    {
        var graph = new GraphView();

        try
        {
            var item = await _context.Items
                .Include(i => i.Category)
                .Include(i => i.Room).ThenInclude(r => r!.Floor).ThenInclude(f => f!.Building)
                .Include(i => i.Container)
                .Include(i => i.Section)
                .Include(i => i.ItemImages)
                .FirstOrDefaultAsync(i => i.Id == itemId && i.IsActive, cancellationToken)
                .ConfigureAwait(false);

            if (item == null)
            {
                return graph;
            }

            graph.Nodes.Add(new GraphNode
            {
                Id = $"item_{item.Id}",
                Label = item.Name,
                Type = "item",
                    Color = GetColorForCategory(),
                Data = new
                {
                    item.Id,
                    item.Name,
                    item.Description,
                    item.Quantity,
                    Category = item.Category?.Name,
                    Room = item.Room?.Name,
                },
            });

            if (item.Room != null)
            {
                graph.Nodes.Add(new GraphNode
                {
                    Id = $"room_{item.Room.Id}",
                    Label = item.Room.Name,
                    Type = "room",
                    Color = "#f59e0b",
                });

                graph.Links.Add(new GraphLink
                {
                    Source = $"item_{item.Id}",
                    Target = $"room_{item.Room.Id}",
                    Label = "in room",
                    Type = "location",
                });
            }

            if (item.Category != null)
            {
                graph.Nodes.Add(new GraphNode
                {
                    Id = $"category_{item.Category.Id}",
                    Label = item.Category.Name,
                    Type = "category",
                    Color = "#10b981",
                });

                graph.Links.Add(new GraphLink
                {
                    Source = $"item_{item.Id}",
                    Target = $"category_{item.Category.Id}",
                    Label = "in category",
                    Type = "category",
                });
            }

            var similarItems = await _context.Items
                .Include(i => i.Category)
                .Include(i => i.Room)
                .Where(i => i.IsActive && i.Id != itemId &&
                            (i.RoomId == item.RoomId || i.CategoryId == item.CategoryId))
                .Take(10)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var similarItem in similarItems)
            {
                graph.Nodes.Add(new GraphNode
                {
                    Id = $"item_{similarItem.Id}",
                    Label = similarItem.Name,
                    Type = "item",
                        Color = GetColorForCategory(),
                });

                graph.Links.Add(new GraphLink
                {
                    Source = $"item_{item.Id}",
                    Target = $"item_{similarItem.Id}",
                    Label = "similar",
                    Type = "similar",
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build item graph for item {ItemId}", itemId);
            throw;
        }

        return graph;
    }

    #endregion Публичные методы

    #region Приватные методы

    /// <summary>
    /// Возвращает цвет узла категории (единообразный серый)
    /// </summary>
    private static string GetColorForCategory() => "#6b7280";

    #endregion Приватные методы
}
