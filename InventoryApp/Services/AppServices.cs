using InventoryApp.Application.Services.Backup;
using InventoryApp.Application.Services.Export;
using InventoryApp.Application.Services.Graph;
using InventoryApp.Application.Services.InventoryWrite;
using InventoryApp.Application.Services.Items;
using InventoryApp.Application.Services.Lookup;
using InventoryApp.Application.Services.Movements;
using InventoryApp.Application.Services.Statistics;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryApp.Services;

/// <summary>
/// Доступ к корневому DI после сборки <see cref="MauiApp"/> (страницы из Shell DataTemplate не получают конструкторный DI)
/// </summary>
public static class AppServices
{
    #region Свойства

    /// <summary>
    /// Корневой провайдер сервисов приложения
    /// </summary>
    public static IServiceProvider Services { get; private set; } = null!;

    #endregion Свойства

    #region Методы

    /// <summary>
    /// Сохраняет провайдер сервисов для использования на страницах
    /// </summary>
    /// <param name="services">Провайдер из собранного <see cref="MauiApp"/></param>
    public static void Init(IServiceProvider services)
    {
        Services = services;
    }

    /// <summary>
    /// Выполняет действие в scope с <see cref="IInventoryStatisticsService"/>
    /// </summary>
    /// <typeparam name="TResult">Тип результата</typeparam>
    /// <param name="action">Асинхронная функция с сервисом статистики</param>
    /// <param name="cancellationToken">Токен отмены</param>
    public static async Task<TResult> WithStatisticsAsync<TResult>(
        Func<IInventoryStatisticsService, Task<TResult>> action,
        CancellationToken cancellationToken = default)
    {
        using var scope = Services.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IInventoryStatisticsService>();
        return await action(svc).ConfigureAwait(false);
    }

    /// <summary>
    /// Выполняет действие в scope с <see cref="IGraphService"/>
    /// </summary>
    /// <typeparam name="TResult">Тип результата</typeparam>
    /// <param name="action">Асинхронная функция с сервисом графа</param>
    /// <param name="cancellationToken">Токен отмены</param>
    public static async Task<TResult> WithGraphAsync<TResult>(
        Func<IGraphService, Task<TResult>> action,
        CancellationToken cancellationToken = default)
    {
        using var scope = Services.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IGraphService>();
        return await action(svc).ConfigureAwait(false);
    }

    /// <summary>
    /// Выполняет действие в scope с <see cref="IInventoryWriteService"/> и возвращает результат
    /// </summary>
    /// <typeparam name="TResult">Тип результата</typeparam>
    /// <param name="action">Асинхронная функция с сервисом записи</param>
    /// <param name="cancellationToken">Токен отмены</param>
    public static async Task<TResult> WithInventoryWriteAsync<TResult>(
        Func<IInventoryWriteService, Task<TResult>> action,
        CancellationToken cancellationToken = default)
    {
        using var scope = Services.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IInventoryWriteService>();
        return await action(svc).ConfigureAwait(false);
    }

    /// <summary>
    /// Выполняет действие в scope с <see cref="IInventoryWriteService"/> без результата
    /// </summary>
    /// <param name="action">Асинхронная функция с сервисом записи</param>
    /// <param name="cancellationToken">Токен отмены</param>
    public static async Task WithInventoryWriteAsync(
        Func<IInventoryWriteService, Task> action,
        CancellationToken cancellationToken = default)
    {
        using var scope = Services.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IInventoryWriteService>();
        await action(svc).ConfigureAwait(false);
    }

    /// <summary>
    /// Выполняет действие в scope с <see cref="IInventoryItemCatalogService"/>
    /// </summary>
    /// <typeparam name="TResult">Тип результата</typeparam>
    /// <param name="action">Асинхронная функция с сервисом каталога</param>
    /// <param name="cancellationToken">Токен отмены</param>
    public static async Task<TResult> WithItemCatalogAsync<TResult>(
        Func<IInventoryItemCatalogService, Task<TResult>> action,
        CancellationToken cancellationToken = default)
    {
        using var scope = Services.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IInventoryItemCatalogService>();
        return await action(svc).ConfigureAwait(false);
    }

    /// <summary>
    /// Выполняет действие в scope с <see cref="IInventoryLookupService"/>
    /// </summary>
    /// <typeparam name="TResult">Тип результата</typeparam>
    /// <param name="action">Асинхронная функция со справочниками</param>
    /// <param name="cancellationToken">Токен отмены</param>
    public static async Task<TResult> WithLookupAsync<TResult>(
        Func<IInventoryLookupService, Task<TResult>> action,
        CancellationToken cancellationToken = default)
    {
        using var scope = Services.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IInventoryLookupService>();
        return await action(svc).ConfigureAwait(false);
    }

    /// <summary>
    /// Выполняет действие в scope с <see cref="IItemMovementHistoryService"/>
    /// </summary>
    /// <typeparam name="TResult">Тип результата</typeparam>
    /// <param name="action">Асинхронная функция с журналом перемещений</param>
    /// <param name="cancellationToken">Токен отмены</param>
    public static async Task<TResult> WithMovementHistoryAsync<TResult>(
        Func<IItemMovementHistoryService, Task<TResult>> action,
        CancellationToken cancellationToken = default)
    {
        using var scope = Services.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IItemMovementHistoryService>();
        return await action(svc).ConfigureAwait(false);
    }

    /// <summary>
    /// Выполняет действие в scope с <see cref="IInventoryExportService"/>
    /// </summary>
    /// <typeparam name="TResult">Тип результата</typeparam>
    /// <param name="action">Асинхронная функция с сервисом экспорта</param>
    /// <param name="cancellationToken">Токен отмены</param>
    public static async Task<TResult> WithExportAsync<TResult>(
        Func<IInventoryExportService, Task<TResult>> action,
        CancellationToken cancellationToken = default)
    {
        using var scope = Services.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IInventoryExportService>();
        return await action(svc).ConfigureAwait(false);
    }

    /// <summary>
    /// Выполняет действие в scope с <see cref="IInventoryBackupService"/> и возвращает результат
    /// </summary>
    /// <typeparam name="TResult">Тип результата</typeparam>
    /// <param name="action">Асинхронная функция с сервисом резервного копирования</param>
    /// <param name="cancellationToken">Токен отмены</param>
    public static async Task<TResult> WithBackupAsync<TResult>(
        Func<IInventoryBackupService, Task<TResult>> action,
        CancellationToken cancellationToken = default)
    {
        using var scope = Services.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IInventoryBackupService>();
        return await action(svc).ConfigureAwait(false);
    }

    /// <summary>
    /// Выполняет действие в scope с <see cref="IInventoryBackupService"/> без результата
    /// </summary>
    /// <param name="action">Асинхронная функция с сервисом резервного копирования</param>
    /// <param name="cancellationToken">Токен отмены</param>
    public static async Task WithBackupAsync(
        Func<IInventoryBackupService, Task> action,
        CancellationToken cancellationToken = default)
    {
        using var scope = Services.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IInventoryBackupService>();
        await action(svc).ConfigureAwait(false);
    }

    #endregion Методы
}
