namespace InventoryApp.Domain.Entities
{
    /// <summary>
    /// Отделение внутри контейнера (полка, отдел)
    /// </summary>
    public class Section : BaseEntity
    {
        #region Свойства

        /// <summary>
        /// Идентификатор контейнера
        /// </summary>
        public int ContainerId { get; set; }

        /// <summary>
        /// Контейнер, к которому принадлежит отделение
        /// </summary>
        public virtual Container Container { get; set; } = null!;

        /// <summary>
        /// Название отделения
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Порядковый номер для сортировки
        /// </summary>
        public int? PositionIndex { get; set; }

        /// <summary>
        /// Описание отделения
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
        /// Флаг активности отделения
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Предметы, расположенные в отделении
        /// </summary>
        public virtual ICollection<Item> Items { get; set; } = new List<Item>();

        #endregion
    }
}

