using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.SNBT;

public class SnbtListBuilder
{
    private readonly List<ISnbtNode> _list = [];

    [PublicAPI]
    public SnbtListBuilder Add(ISnbtNode node)
    {
        _list.Add(node);
        return this;
    }

    [PublicAPI]
    public SnbtListBuilder Add(sbyte value) => Add(new SnbtByte(value));
    [PublicAPI]
    public SnbtListBuilder Add(int value) => Add(new SnbtInt(value));
    [PublicAPI]
    public SnbtListBuilder Add(string value) => Add(new SnbtString(value));

    [PublicAPI]
    public SnbtListBuilder AddRange(IEnumerable<string> values)
    {
        foreach (var v in values)
            Add(new SnbtString(v));
        return this;
    }

    [PublicAPI]
    public SnbtListBuilder AddRange(IEnumerable<int> values)
    {
        foreach (var v in values) Add(new SnbtInt(v));
        return this;
    }

    [PublicAPI]
    public SnbtListBuilder AddRange(IEnumerable<double> values)
    {
        foreach (var v in values) Add(new SnbtDouble(v));
        return this;
    }

    [PublicAPI]
    public SnbtListBuilder AddCompound(Action<SnbtCompoundBuilder> buildAction)
    {
        var nestedBuilder = new SnbtCompoundBuilder();
        buildAction(nestedBuilder);
        return Add(nestedBuilder.Build());
    }

    [PublicAPI]
    public SnbtList Build() => new(_list);
}