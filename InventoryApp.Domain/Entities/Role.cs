namespace InventoryApp.Domain.Entities
{
    /// <summary>
    /// Роль пользователя в системе
    /// </summary>
    public class Role : BaseEntity
    {
        #region Свойства

        /// <summary>
        /// Название роли
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Описание роли и ее прав
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Пользователи, имеющие данную роль
        /// </summary>
        public virtual ICollection<User> Users { get; set; } = new List<User>();

        #endregion
    }
}

