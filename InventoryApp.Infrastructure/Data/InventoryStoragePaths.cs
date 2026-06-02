namespace InventoryApp.Infrastructure.Data;

/// <summary>
/// Пути к файлу SQLite и каталогу данных приложения (медиа предметов и т.д.)
/// </summary>
/// <param name="DatabaseFilePath">Полный путь к <c>inventory.db</c></param>
/// <param name="AppDataDirectoryPath">Каталог данных приложения (родитель файла БД)</param>
public sealed record InventoryStoragePaths(string DatabaseFilePath, string AppDataDirectoryPath)
{
    #region Свойства

    /// <summary>
    /// Каталог с фото предметов: <c>item_media</c>
    /// </summary>
    public string ItemMediaDirectoryPath => Path.Combine(AppDataDirectoryPath, "item_media");

    #endregion Свойства
}
