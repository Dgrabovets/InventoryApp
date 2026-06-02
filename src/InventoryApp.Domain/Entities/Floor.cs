namespace InventoryApp.Domain.Entities
{
    /// <summary>
    /// Этаж строения
    /// </summary>
    public class Floor : BaseEntity
    {
        #region Свойства

        /// <summary>
        /// Идентификатор строения
        /// </summary>
        public int BuildingId { get; set; }

        /// <summary>
        /// Строение, к которому принадлежит этаж
        /// </summary>
        public virtual Building Building { get; set; } = null!;

        /// <summary>
        /// Номер этажа (-1 для подвала, 0 для цоколя, 1+ для этажей)
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Название этажа
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Описание этажа
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
        /// Флаг активности этажа
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Комнаты на этаже
        /// </summary>
        public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();

        /// <summary>
        /// Контейнеры на этаже
        /// </summary>
        public virtual ICollection<Container> Containers { get; set; } = new List<Container>();

        /// <summary>
        /// Предметы, расположенные на этаже
        /// </summary>
        public virtual ICollection<Item> Items { get; set; } = new List<Item>();

        #endregion
    }
}

