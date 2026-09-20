using Core.DataComponents.Models.Interfaces;
using Core.SNBT;

using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;

namespace Core.DataComponents.Models.Extensions;

public static class SnbtModelExtensions
{
    /// <summary>
    /// Adds a collection of serializable models to a compound builder under a given key.
    /// </summary>
    public static SnbtCompoundBuilder PutModels<T>(
        this SnbtCompoundBuilder builder,
        string key,
        IReadOnlyList<T>? models)
        where T : ISnbtSerializable
    {
        if (models == null || models.Count == 0)
            return builder;

        return builder.PutList(key, list =>
        {
            foreach (var model in models)
                list.Add(model.ToSnbt());
        });
    }

    /// <summary>
    /// Extracts a list of parsed compound models from an SNBT list node.
    /// </summary>
    public static List<T>? GetCompoundList<T>(this SnbtCompound compound, string key)
        where T : ICompoundModel<T>
    {
        if (compound.GetNode(key) is not SnbtList list || list.Items.Count == 0)
            return null;

        var result = new List<T>(list.Items.Count);
        foreach (var item in list.Items)
        {
            if (item is SnbtCompound comp)
                result.Add(T.Parse(comp));
        }

        return result;
    }

    /// <summary>
    /// Extracts a list of flexible models (accepting arbitrary ISnbtNode items) from an SNBT list node.
    /// </summary>
    public static List<T>? GetModelList<T>(this SnbtCompound compound, string key)
        where T : IFlexibleModel<T>
    {
        if (compound.GetNode(key) is not SnbtList list || list.Items.Count == 0)
            return null;

        var result = new List<T>(list.Items.Count);
        foreach (var item in list.Items)
            result.Add(T.Parse(item));

        return result;
    }

    /// <summary>
    /// Extracts a list of parsed models with explicit NBT node type filtering.
    /// </summary>
    public static List<TModel>? GetModelList<TModel, TNode>(this SnbtCompound compound, string key)
        where TModel : ISnbtModel<TModel, TNode>
        where TNode : class, ISnbtNode
    {
        if (compound.GetNode(key) is not SnbtList list || list.Items.Count == 0)
            return null;

        var result = new List<TModel>(list.Items.Count);
        foreach (var item in list.Items)
        {
            if (item is TNode typedNode)
                result.Add(TModel.Parse(typedNode));
        }

        return result;
    }
}