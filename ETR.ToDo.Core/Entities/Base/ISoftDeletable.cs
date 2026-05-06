namespace ETR.ToDo.Core.Entities.Base;

/// <summary>
/// Interface for entities that support soft delete
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedDate { get; set; }
    string? DeletedBy { get; set; }
}
