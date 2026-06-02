namespace InventoryApp.Domain.Entities
{
    /// <summary>
    /// Комната внутри этажа строения
    /// </summary>
    public class Room : BaseEntity
    {
        #region Свойства

        /// <summary>
        /// Идентификатор этажа
        /// </summary>
        public int FloorId { get; set; }

        /// <summary>
        /// Этаж, к которому принадлежит комната
        /// </summary>
        public virtual Floor Floor { get; set; } = null!;

        /// <summary>
        /// Название комнаты
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Описание комнаты
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
        /// Флаг активности комнаты
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Контейнеры в комнате
        /// </summary>
        public virtual ICollection<Container> Containers { get; set; } = new List<Container>();

        /// <summary>
        /// Предметы, расположенные в комнате
        /// </summary>
        public virtual ICollection<Item> Items { get; set; } = new List<Item>();

        #endregion
    }
}

