using Core.SNBT.Interfaces;

namespace Core.DataComponents.Models.Interfaces;

/// <summary>
/// Alias interface for models/components that can accept any ISnbtNode (polymorphic NBT).
/// </summary>
public interface IFlexibleModel<out TSelf> : ISnbtModel<TSelf, ISnbtNode>
    where TSelf : IFlexibleModel<TSelf>;