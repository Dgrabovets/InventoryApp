namespace InventoryApp.Domain.Entities
{
    /// <summary>
    /// Категория предметов инвентаря
    /// </summary>
    public class Category : BaseEntity
    {
        #region Свойства

        /// <summary>
        /// Название категории
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Описание категории
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Идентификатор родительской категории
        /// </summary>
        public int? ParentCategoryId { get; set; }

        /// <summary>
        /// Родительская категория
        /// </summary>
        public virtual Category? ParentCategory { get; set; }

        /// <summary>
        /// Дочерние категории
        /// </summary>
        public virtual ICollection<Category> ChildCategories { get; set; } = new List<Category>();

        /// <summary>
        /// Идентификатор пользователя, создавшего запись
        /// </summary>
        public int CreatedById { get; set; }

        /// <summary>
        /// Пользователь, создавший запись
        /// </summary>
        public virtual User CreatedBy { get; set; } = null!;

        /// <summary>
        /// Флаг активности категории
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Предметы в данной категории
        /// </summary>
        public virtual ICollection<Item> Items { get; set; } = new List<Item>();

        #endregion
    }
}

