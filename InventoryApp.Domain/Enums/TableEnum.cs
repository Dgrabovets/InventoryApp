namespace InventoryApp.Domain.Enums
{
    /// <summary>
    /// Перечисление названий таблиц для аудита
    /// </summary>
    public enum TableEnum
    {
        /// <summary>
        /// Таблица пользователей
        /// </summary>
        USERS,

        /// <summary>
        /// Таблица ролей
        /// </summary>
        ROLES,

        /// <summary>
        /// Таблица зданий
        /// </summary>
        BUILDINGS,

        /// <summary>
        /// Таблица этажей
        /// </summary>
        FLOORS,

        /// <summary>
        /// Таблица комнат
        /// </summary>
        ROOMS,

        /// <summary>
        /// Таблица контейнеров
        /// </summary>
        CONTAINERS,

        /// <summary>
        /// Таблица отделений
        /// </summary>
        SECTIONS,

        /// <summary>
        /// Таблица категорий
        /// </summary>
        CATEGORIES,

        /// <summary>
        /// Таблица предметов
        /// </summary>
        ITEMS,

        /// <summary>
        /// Таблица изображений предметов
        /// </summary>
        ITEM_IMAGES,

        /// <summary>
        /// Таблица типов контейнеров
        /// </summary>
        CONTAINER_TYPES,

        /// <summary>
        /// Таблица перемещений предметов
        /// </summary>
        ITEM_MOVEMENTS,

        /// <summary>
        /// Таблица токенов сброса пароля
        /// </summary>
        PASSWORD_RESET_TOKENS,

        /// <summary>
        /// Таблица аудита
        /// </summary>
        AUDIT_LOGS
    }
}

