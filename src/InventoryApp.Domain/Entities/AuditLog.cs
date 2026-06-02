namespace InventoryApp.Domain.Entities
{
    /// <summary>
    /// Запись аудита действий в системе
    /// </summary>
    public class AuditLog : BaseEntity
    {
        #region Свойства

        /// <summary>
        /// Название таблицы, где произошло изменение
        /// </summary>
        public string TableName { get; set; } = string.Empty;

        /// <summary>
        /// Идентификатор измененной записи
        /// </summary>
        public int RecordId { get; set; }

        /// <summary>
        /// Тип действия: INSERT, UPDATE, DELETE
        /// </summary>
        public string ActionType { get; set; } = string.Empty;

        /// <summary>
        /// Старые значения в формате JSON
        /// </summary>
        public string? OldValues { get; set; }

        /// <summary>
        /// Новые значения в формате JSON
        /// </summary>
        public string? NewValues { get; set; }

        /// <summary>
        /// Идентификатор пользователя, выполнившего действие
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Пользователь, выполнивший действие
        /// </summary>
        public virtual User? User { get; set; }

        #endregion
    }
}

