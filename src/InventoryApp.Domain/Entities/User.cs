namespace InventoryApp.Domain.Entities
{
    /// <summary>
    /// Пользователь системы
    /// </summary>
    public class User : BaseEntity
    {
        #region Свойства

        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Электронная почта пользователя
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Хеш пароля пользователя
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Идентификатор роли пользователя
        /// </summary>
        public int RoleId { get; set; }

        /// <summary>
        /// Роль пользователя
        /// </summary>
        public virtual Role Role { get; set; } = null!;

        /// <summary>
        /// Флаг активности пользователя
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Дата и время последнего входа
        /// </summary>
        public DateTime? LastLogin { get; set; }

        /// <summary>
        /// Строения, созданные пользователем
        /// </summary>
        public virtual ICollection<Building> BuildingsCreated { get; set; } = new List<Building>();

        /// <summary>
        /// Предметы, созданные пользователем
        /// </summary>
        public virtual ICollection<Item> ItemsCreated { get; set; } = new List<Item>();

        #endregion
    }
}

