using Core.SNBT.Nodes;

namespace Core.DataComponents.Interfaces;

/// <summary>
/// Convenience alias for components that parse strictly from an SNBT compound node.
/// </summary>
public interface IListComponent<TSelf> : IParsableComponent<TSelf, SnbtList>
    where TSelf : IListComponent<TSelf>;