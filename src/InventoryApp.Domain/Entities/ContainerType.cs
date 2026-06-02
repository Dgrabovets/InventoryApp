namespace InventoryApp.Domain.Entities;

/// <summary>
/// Справочник типов контейнера (шкаф, ящик и т.д.).
/// </summary>
public class ContainerType : BaseEntity
{
    /// <summary>
    /// Название типа.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Описание.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Активен ли тип в выборках.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Контейнеры данного типа.
    /// </summary>
    public virtual ICollection<Container> Containers { get; set; } = new List<Container>();
}
