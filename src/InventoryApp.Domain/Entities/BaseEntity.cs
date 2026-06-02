namespace InventoryApp.Domain.Entities
{
    /// <summary>
    /// Базовый класс для всех сущностей базы данных
    /// </summary>
    public abstract class BaseEntity
    {
        #region Свойства

        /// <summary>
        /// Уникальный идентификатор записи
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Дата и время создания записи (UTC)
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Дата и время последнего обновления записи (UTC)
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        #endregion
    }
}

