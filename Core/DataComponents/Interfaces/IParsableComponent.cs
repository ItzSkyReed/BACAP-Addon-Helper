using Core.DataComponents.Models.Interfaces;
using Core.SNBT.Interfaces;

namespace Core.DataComponents.Interfaces;

/// <summary>
/// Contract for data components with strongly-typed node parsing.
/// </summary>
public interface IParsableComponent<TSelf, in TNode> : ITypedComponent<TSelf>, ISnbtModel<TSelf, TNode>
    where TSelf : IParsableComponent<TSelf, TNode>
    where TNode : ISnbtNode;

/// <summary>
/// Convenience alias for components that parse from any arbitrary SNBT node.
/// </summary>
public interface IParsableComponent<TSelf> : IParsableComponent<TSelf, ISnbtNode>
    where TSelf : IParsableComponent<TSelf>;