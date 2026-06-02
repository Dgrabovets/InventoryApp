namespace InventoryApp.Domain.Entities
{
    /// <summary>
    /// Запись о перемещении предмета
    /// </summary>
    public class ItemMovement : BaseEntity
    {
        #region Свойства

        /// <summary>
        /// Название предмета (сохраняется для истории, даже если предмет удален)
        /// </summary>
        public string ItemName { get; set; } = string.Empty;

        /// <summary>
        /// Идентификатор исходного строения
        /// </summary>
        public int? FromBuildingId { get; set; }

        /// <summary>
        /// Исходное строение
        /// </summary>
        public virtual Building? FromBuilding { get; set; }

        /// <summary>
        /// Идентификатор исходного этажа
        /// </summary>
        public int? FromFloorId { get; set; }

        /// <summary>
        /// Исходный этаж
        /// </summary>
        public virtual Floor? FromFloor { get; set; }

        /// <summary>
        /// Идентификатор исходной комнаты
        /// </summary>
        public int? FromRoomId { get; set; }

        /// <summary>
        /// Исходная комната
        /// </summary>
        public virtual Room? FromRoom { get; set; }

        /// <summary>
        /// Идентификатор исходного контейнера
        /// </summary>
        public int? FromContainerId { get; set; }

        /// <summary>
        /// Исходный контейнер
        /// </summary>
        public virtual Container? FromContainer { get; set; }

        /// <summary>
        /// Идентификатор исходного отделения
        /// </summary>
        public int? FromSectionId { get; set; }

        /// <summary>
        /// Исходное отделение
        /// </summary>
        public virtual Section? FromSection { get; set; }

        /// <summary>
        /// Идентификатор целевого строения
        /// </summary>
        public int? ToBuildingId { get; set; }

        /// <summary>
        /// Целевое строение
        /// </summary>
        public virtual Building? ToBuilding { get; set; }

        /// <summary>
        /// Идентификатор целевого этажа
        /// </summary>
        public int? ToFloorId { get; set; }

        /// <summary>
        /// Целевой этаж
        /// </summary>
        public virtual Floor? ToFloor { get; set; }

        /// <summary>
        /// Идентификатор целевой комнаты
        /// </summary>
        public int? ToRoomId { get; set; }

        /// <summary>
        /// Целевая комната
        /// </summary>
        public virtual Room? ToRoom { get; set; }

        /// <summary>
        /// Идентификатор целевого контейнера
        /// </summary>
        public int? ToContainerId { get; set; }

        /// <summary>
        /// Целевой контейнер
        /// </summary>
        public virtual Container? ToContainer { get; set; }

        /// <summary>
        /// Идентификатор целевого отделения
        /// </summary>
        public int? ToSectionId { get; set; }

        /// <summary>
        /// Целевое отделение
        /// </summary>
        public virtual Section? ToSection { get; set; }

        /// <summary>
        /// Идентификатор пользователя, выполнившего перемещение
        /// </summary>
        public int MovedById { get; set; }

        /// <summary>
        /// Пользователь, выполнивший перемещение
        /// </summary>
        public virtual User MovedBy { get; set; } = null!;

        /// <summary>
        /// Дата и время перемещения
        /// </summary>
        public DateTime MovedAt { get; set; }

        /// <summary>
        /// Дополнительные заметки о перемещении
        /// </summary>
        public string? Notes { get; set; }

        #endregion
    }
}

