using InventoryApp.Application.Services.Backup;
using InventoryApp.Application.Services.Export;
using InventoryApp.Application.Services.Graph;
using InventoryApp.Application.Services.InventoryWrite;
using InventoryApp.Application.Services.Items;
using InventoryApp.Application.Services.Lookup;
using InventoryApp.Application.Services.Movements;
using InventoryApp.Application.Services.Statistics;
using InventoryApp.Domain.Entities;
using InventoryApp.Infrastructure.Services.Backup;
using InventoryApp.Infrastructure.Services.Export;
using InventoryApp.Infrastructure.Services.Graph;
using InventoryApp.Infrastructure.Services.InventoryWrite;
using InventoryApp.Infrastructure.Services.Items;
using InventoryApp.Infrastructure.Services.Lookup;
using InventoryApp.Infrastructure.Services.Movements;
using InventoryApp.Infrastructure.Services.Statistics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryApp.Infrastructure.Data;

/// <summary>
/// Инициализация локальной базы данных: миграции и обязательный сид (роль и пользователь для FK)
/// </summary>
public sealed class DatabaseInitializer
{
    #region Поля

    /// <summary>
    /// Контекст EF Core для миграций и сида
    /// </summary>
    private readonly ApplicationContext _context;

    #endregion Поля

    #region Конструкторы

    /// <summary>
    /// Создаёт инициализатор базы данных
    /// </summary>
    /// <param name="context">Контекст базы данных</param>
    public DatabaseInitializer(ApplicationContext context)
    {
        _context = context;
    }

    #endregion Конструкторы

    #region Публичные методы

    /// <summary>
    /// Применяет миграции и создаёт минимальные данные при первом запуске
    /// </summary>
    /// <param name="cancellationToken">Токен отмены</param>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);

        if (!await _context.Roles.AsNoTracking().AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            var role = new Role
            {
                Name = "Пользователь",
                Description = "Локальный режим без входа в систему",
            };
            _context.Roles.Add(role);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        if (!await _context.Users.AsNoTracking().AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            var role = await _context.Roles.AsNoTracking().FirstAsync(cancellationToken).ConfigureAwait(false);
            var user = new User
            {
                Username = "локальный",
                Email = null,
                PasswordHash = "-",
                RoleId = role.Id,
                IsActive = true,
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    #endregion Публичные методы
}

/// <summary>
/// Регистрация инфраструктуры в DI
/// </summary>
public static class ServiceCollectionExtensions
{
    #region Публичные методы

    /// <summary>
    /// Добавляет SQLite, контекст БД, сервисы чтения и запись <see cref="IInventoryWriteService"/>
    /// </summary>
    /// <param name="services">Коллекция сервисов</param>
    /// <param name="databasePath">Полный путь к файлу SQLite</param>
    /// <returns>Та же коллекция сервисов</returns>
    public static IServiceCollection AddInventoryData(this IServiceCollection services, string databasePath)
    {
        var dbFull = Path.GetFullPath(databasePath);
        var appDataDir = Path.GetDirectoryName(dbFull)
            ?? throw new ArgumentException("Некорректный путь к базе данных.", nameof(databasePath));
        services.AddSingleton(new InventoryStoragePaths(dbFull, appDataDir));

        services.AddDbContext<ApplicationContext>(options =>
            options.UseSqlite($"Data Source={dbFull}"));
        services.AddScoped<DatabaseInitializer>();
        services.AddScoped<IInventoryStatisticsService, InventoryStatisticsService>();
        services.AddScoped<IGraphService, GraphService>();
        services.AddScoped<IInventoryWriteService, InventoryWriteService>();
        services.AddScoped<IInventoryLookupService, InventoryLookupService>();
        services.AddScoped<IInventoryItemCatalogService, InventoryItemCatalogService>();
        services.AddScoped<IItemMovementHistoryService, ItemMovementHistoryService>();
        services.AddScoped<IInventoryExportService, InventoryExportService>();
        services.AddScoped<IInventoryBackupService, InventoryBackupService>();
        return services;
    }

    #endregion Публичные методы
}
