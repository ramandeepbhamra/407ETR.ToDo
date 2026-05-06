namespace ETR.ToDo.Core.Entities.Base;

/// <summary>
/// Interface for entities that require audit tracking
/// </summary>
public interface IAuditableEntity
{
    DateTime CreatedDate { get; set; }
    string? CreatedBy { get; set; }
    DateTime? ModifiedDate { get; set; }
    string? ModifiedBy { get; set; }
}
