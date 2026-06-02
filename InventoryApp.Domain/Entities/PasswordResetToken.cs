namespace InventoryApp.Domain.Entities
{
    /// <summary>
    /// Токен сброса пароля
    /// </summary>
    public class PasswordResetToken : BaseEntity
    {
        #region Свойства

        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Токен (GUID)
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Дата истечения срока действия токена (UTC)
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Флаг использования токена
        /// </summary>
        public bool IsUsed { get; set; }

        /// <summary>
        /// Дата использования токена (UTC)
        /// </summary>
        public DateTime? UsedAt { get; set; }

        /// <summary>
        /// Пользователь, которому принадлежит токен
        /// </summary>
        public virtual User? User { get; set; }

        #endregion
    }
}

