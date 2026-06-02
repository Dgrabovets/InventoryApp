using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InventoryApp.Infrastructure.Data;

/// <summary>
/// Фабрика контекста для команд <c>dotnet ef</c> (design-time)
/// </summary>
public sealed class ApplicationContextFactory : IDesignTimeDbContextFactory<ApplicationContext>
{
    #region Публичные методы

    /// <inheritdoc />
    /// <param name="args">Аргументы CLI EF Core (не используются; строка подключения фиксирована)</param>
    public ApplicationContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ApplicationContext>()
            .UseSqlite("Data Source=design_time_inventory.db")
            .Options;

        return new ApplicationContext(options);
    }

    #endregion Публичные методы
}
