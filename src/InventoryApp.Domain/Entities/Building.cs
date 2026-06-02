namespace InventoryApp.Domain.Entities
{
    /// <summary>
    /// Строение (здание) где хранится инвентарь
    /// </summary>
    public class Building : BaseEntity
    {
        #region Свойства

        /// <summary>
        /// Название строения
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Описание строения
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Идентификатор пользователя, создавшего запись
        /// </summary>
        public int CreatedById { get; set; }

        /// <summary>
        /// Пользователь, создавший запись
        /// </summary>
        public virtual User CreatedBy { get; set; } = null!;

        /// <summary>
        /// Флаг активности строения
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Этажи в строении
        /// </summary>
        public virtual ICollection<Floor> Floors { get; set; } = new List<Floor>();

        /// <summary>
        /// Предметы, расположенные в строении
        /// </summary>
        public virtual ICollection<Item> Items { get; set; } = new List<Item>();

        #endregion
    }
}

