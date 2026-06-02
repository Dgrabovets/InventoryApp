namespace InventoryApp.Domain.Entities
{
    /// <summary>
    /// Контейнер хранения (шкаф, полка, ящик)
    /// </summary>
    public class Container : BaseEntity
    {
        #region Свойства

        /// <summary>
        /// Идентификатор этажа
        /// </summary>
        public int? FloorId { get; set; }

        /// <summary>
        /// Этаж, на котором расположен контейнер
        /// </summary>
        public virtual Floor? Floor { get; set; }

        /// <summary>
        /// Идентификатор комнаты
        /// </summary>
        public int? RoomId { get; set; }

        /// <summary>
        /// Комната, в которой расположен контейнер
        /// </summary>
        public virtual Room? Room { get; set; }

        /// <summary>
        /// Идентификатор типа контейнера
        /// </summary>
        public int ContainerTypeId { get; set; }

        /// <summary>
        /// Тип контейнера (справочник).
        /// </summary>
        public virtual ContainerType? ContainerType { get; set; }

        /// <summary>
        /// Название контейнера
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Описание контейнера
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Текстовое описание расположения контейнера
        /// </summary>
        public string? LocationDescription { get; set; }

        /// <summary>
        /// Идентификатор пользователя, создавшего запись
        /// </summary>
        public int CreatedById { get; set; }

        /// <summary>
        /// Пользователь, создавший запись
        /// </summary>
        public virtual User CreatedBy { get; set; } = null!;

        /// <summary>
        /// Флаг активности контейнера
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Отделения в контейнере
        /// </summary>
        public virtual ICollection<Section> Sections { get; set; } = new List<Section>();

        /// <summary>
        /// Предметы, расположенные в контейнере
        /// </summary>
        public virtual ICollection<Item> Items { get; set; } = new List<Item>();

        #endregion
    }
}

