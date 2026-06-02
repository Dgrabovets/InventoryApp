namespace InventoryApp.Domain.Entities
{
    /// <summary>
    /// Изображение предмета инвентаря
    /// </summary>
    public class ItemImage : BaseEntity
    {
        #region Свойства

        /// <summary>
        /// Идентификатор предмета
        /// </summary>
        public int ItemId { get; set; }

        /// <summary>
        /// Предмет, к которому относится изображение
        /// </summary>
        public virtual Item Item { get; set; } = null!;

        /// <summary>
        /// URL или путь к файлу изображения
        /// </summary>
        public string ImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// Флаг основного изображения предмета
        /// </summary>
        public bool IsPrimary { get; set; }

        /// <summary>
        /// Идентификатор пользователя, загрузившего изображение
        /// </summary>
        public int UploadedById { get; set; }

        /// <summary>
        /// Пользователь, загрузивший изображение
        /// </summary>
        public virtual User UploadedBy { get; set; } = null!;

        #endregion
    }
}

