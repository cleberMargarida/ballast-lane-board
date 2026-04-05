namespace BallastLaneBoard.Domain.Core;

public interface IEntity : IEntity<Guid>
{
}

public interface IEntity<TId> where TId : struct, IEquatable<TId>
{
    TId Id { get; }
}

public interface IAggregateRoot : IAggregateRoot<Guid>, IEntity
{
}

public interface IAggregateRoot<TId> : IEntity<TId> where TId : struct, IEquatable<TId>
{
}
