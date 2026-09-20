using Core.SNBT.Interfaces;

namespace Core.DataComponents.Interfaces;

public interface IFlexibleComponent<TSelf> : IParsableComponent<TSelf, ISnbtNode>
    where TSelf : IFlexibleComponent<TSelf>;