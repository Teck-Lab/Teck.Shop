namespace Teck.Shop.SharedKernel.Core.Domain
{
/// <summary>
/// Marker interface used to denote an Aggregate Root in the Domain layer.
/// </summary>
/// <remarks>
/// In Domain-Driven Design (DDD), an Aggregate Root is the entry point to an aggregate, 
/// which is a cluster of domain objects that should be treated as a single unit.
/// Only the aggregate root should be used to access or modify the internal state 
/// of the aggregate, ensuring consistency and encapsulation.
///
/// This interface is typically used in conjunction with the Repository and Unit of Work patterns
/// to enforce aggregate boundaries and consistency rules.
/// </remarks>
public interface IAggregateRoot
{
}
}