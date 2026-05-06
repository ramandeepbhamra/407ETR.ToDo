namespace ETR.ToDo.Services.Core.Common.Dto;

/// <summary>
/// Base DTO for entities with an Id
/// </summary>
/// <typeparam name="TKey">Type of the entity's primary key</typeparam>
public abstract class EntityDto<TKey> where TKey : IEquatable<TKey>
{
    public TKey Id { get; set; } = default!;
}

/// <summary>
/// Base DTO for entities with Guid Id
/// </summary>
public abstract class EntityDto : EntityDto<Guid>
{
}
