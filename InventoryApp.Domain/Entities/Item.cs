namespace InventoryApp.Domain.Entities
{
    /// <summary>
    /// Предмет инвентаря
    /// </summary>
    public class Item : BaseEntity
    {
        #region Свойства

        /// <summary>
        /// Название предмета
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Описание предмета
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Идентификатор категории
        /// </summary>
        public int? CategoryId { get; set; }

        /// <summary>
        /// Категория предмета
        /// </summary>
        public virtual Category? Category { get; set; }

        /// <summary>
        /// Количество единиц предмета
        /// </summary>
        public int Quantity { get; set; } = 1;

        /// <summary>
        /// Срок годности предмета
        /// </summary>
        public DateOnly? ExpiryDate { get; set; }

        /// <summary>
        /// Дата изготовления предмета
        /// </summary>
        public DateOnly? ManufactureDate { get; set; }

        /// <summary>
        /// Дата покупки предмета
        /// </summary>
        public DateOnly? PurchaseDate { get; set; }

        /// <summary>
        /// Цена предмета
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// Идентификатор строения
        /// </summary>
        public int? BuildingId { get; set; }

        /// <summary>
        /// Строение, в котором расположен предмет
        /// </summary>
        public virtual Building? Building { get; set; }

        /// <summary>
        /// Идентификатор этажа
        /// </summary>
        public int? FloorId { get; set; }

        /// <summary>
        /// Этаж, на котором расположен предмет
        /// </summary>
        public virtual Floor? Floor { get; set; }

        /// <summary>
        /// Идентификатор комнаты
        /// </summary>
        public int? RoomId { get; set; }

        /// <summary>
        /// Комната, в которой расположен предмет
        /// </summary>
        public virtual Room? Room { get; set; }

        /// <summary>
        /// Идентификатор контейнера
        /// </summary>
        public int? ContainerId { get; set; }

        /// <summary>
        /// Контейнер, в котором расположен предмет
        /// </summary>
        public virtual Container? Container { get; set; }

        /// <summary>
        /// Идентификатор отделения
        /// </summary>
        public int? SectionId { get; set; }

        /// <summary>
        /// Отделение, в котором расположен предмет
        /// </summary>
        public virtual Section? Section { get; set; }

        /// <summary>
        /// Детальное описание места расположения
        /// </summary>
        public string? LocationNotes { get; set; }

        /// <summary>
        /// Идентификатор пользователя, создавшего запись
        /// </summary>
        public int CreatedById { get; set; }

        /// <summary>
        /// Пользователь, создавший запись
        /// </summary>
        public virtual User CreatedBy { get; set; } = null!;

        /// <summary>
        /// Дата последней проверки наличия
        /// </summary>
        public DateOnly? LastCheckedDate { get; set; }

        /// <summary>
        /// Флаг активности предмета
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Изображения предмета
        /// </summary>
        public virtual ICollection<ItemImage> ItemImages { get; set; } = new List<ItemImage>();

        #endregion
    }
}

