namespace ETR.ToDo.Core.Entities.Base;

/// <summary>
/// Generic base entity class with any primary key type and audit fields
/// Developers must explicitly specify the TKey type parameter (int, long, Guid, string, etc.)
/// </summary>
/// <typeparam name="TKey">The type of the primary key - must be explicitly declared</typeparam>
public abstract class BaseEntity<TKey> : IEntity<TKey>, IAuditableEntity, ISoftDeletable
    where TKey : IEquatable<TKey>
{
    public TKey Id { get; set; } = default!;
    public DateTime CreatedDate { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedDate { get; set; }
    public string? DeletedBy { get; set; }
}
