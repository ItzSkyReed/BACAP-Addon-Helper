using Core.SNBT.Interfaces;

namespace Core.DataComponents.Models.Interfaces;

/// <summary>
/// Enforces SNBT parsing for a specific node type (SnbtCompound, SnbtString, SnbtList, or ISnbtNode).
/// </summary>
public interface ISnbtModel<out TSelf, in TNode> : ISnbtSerializable
    where TSelf : ISnbtModel<TSelf, TNode>
    where TNode : ISnbtNode
{
    static abstract TSelf Parse(TNode node);
}