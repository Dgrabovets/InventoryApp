namespace InventoryApp.Domain.Enums
{
    /// <summary>
    /// Тип действия для аудита
    /// </summary>
    public enum ActionType
    {
        /// <summary>
        /// Вставка новой записи
        /// </summary>
        INSERT,

        /// <summary>
        /// Обновление существующей записи
        /// </summary>
        UPDATE,

        /// <summary>
        /// Удаление записи
        /// </summary>
        DELETE
    }
}

