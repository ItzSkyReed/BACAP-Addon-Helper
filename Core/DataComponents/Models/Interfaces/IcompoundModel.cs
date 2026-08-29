using Core.SNBT.Nodes;

namespace Core.DataComponents.Models.Interfaces;

/// <summary>
/// Alias interface for models that only parse from an SnbtCompound.
/// </summary>
public interface ICompoundModel<TSelf> : ISnbtModel<TSelf, SnbtCompound>
    where TSelf : ICompoundModel<TSelf>;