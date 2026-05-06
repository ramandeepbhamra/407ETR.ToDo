namespace ETR.ToDo.Core.Entities.Base;

/// <summary>
/// Base interface for all entities with primary key
/// </summary>
/// <typeparam name="TKey">Type of the primary key</typeparam>
public interface IEntity<TKey> where TKey : IEquatable<TKey>
{
    TKey Id { get; set; }
}
