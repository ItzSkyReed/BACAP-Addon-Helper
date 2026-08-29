using Core.DataComponents.Models.Interfaces;
using Core.SNBT;

using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;

namespace Core.DataComponents.Models;

public record BlockPredicate(
    List<string>? Blocks = null,
    ISnbtNode? State = null,
    ISnbtNode? Nbt = null,
    ISnbtNode? Components = null,
    ISnbtNode? Predicates = null
) : ICompoundModel<BlockPredicate>
{
    public static BlockPredicate Parse(SnbtCompound compound)
    {

        List<string>? blocks = null;
        var blocksNode = compound.GetNode("blocks");
        switch (blocksNode)
        {
            case SnbtString singleBlock:
                blocks = [singleBlock.Value];
                break;
            case SnbtList blockList:
            {
                blocks = new List<string>(blockList.Items.Count);
                foreach (var item in blockList.Items)
                    if (item is SnbtString s) blocks.Add(s.Value);
                break;
            }
        }

        return new BlockPredicate(
            Blocks: blocks,
            State: compound.GetNode("state"),
            Nbt: compound.GetNode("nbt"),
            Components: compound.GetNode("components"),
            Predicates: compound.GetNode("predicates")
        );
    }

    public ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound();
        if (Blocks is { Count: > 0 })
        {
            if (Blocks.Count == 1)
                builder.Put("blocks", Blocks[0]);
            else
                builder.PutList("blocks", list => list.AddRange(Blocks));
        }

        if (State != null) builder.Put("state", State);
        if (Nbt != null) builder.Put("nbt", Nbt);
        if (Components != null) builder.Put("components", Components);
        if (Predicates != null) builder.Put("predicates", Predicates);

        return builder.Build();
    }
}