using Core.SNBT.Nodes;

namespace Core.DataComponents.Interfaces;

/// <summary>
/// Convenience alias for components that parse strictly from an SNBT compound node.
/// </summary>
public interface IIntComponent<TSelf> : IParsableComponent<TSelf, SnbtInt>
    where TSelf : IIntComponent<TSelf>;