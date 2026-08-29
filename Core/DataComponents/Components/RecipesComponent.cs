using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Specifies the recipe identifiers unlocked when a knowledge book is consumed (<c>minecraft:recipes</c>).
/// </summary>
/// <param name="Recipes">The list of recipe resource location identifiers.</param>
[UsedImplicitly]
public record RecipesComponent(
    List<string> Recipes
) : IListComponent<RecipesComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:recipes";

    /// <summary>
    /// Initializes a new instance of the <see cref="RecipesComponent"/> record with an array of recipe identifiers.
    /// </summary>
    /// <param name="recipes">The recipe identifiers.</param>
    public RecipesComponent(params string[] recipes) : this(recipes.ToList())
    {
    }

    /// <summary>
    /// Parses a <see cref="RecipesComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="list">The SNBT node to parse, which must be an <see cref="SnbtList"/>.</param>
    /// <returns>A populated <see cref="RecipesComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("[\"minecraft:diamond\", \"minecraft:end_crystal\"]");
    /// var component = RecipesComponent.Parse(node);
    /// </code>
    /// </example>
    public static RecipesComponent Parse(SnbtList list)
    {

        var recipes = new List<string>(list.Items.Count);
        foreach (var item in list.Items)
        {
            if (item is SnbtString str)
                recipes.Add(str.Value);
        }

        return new RecipesComponent(recipes);
    }

    /// <summary>
    /// Serializes the component into an SNBT list node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> containing the list of recipe IDs.</returns>
    public ISnbtNode ToSnbt()
    {
        var items = new List<ISnbtNode>(Recipes.Count);
        foreach (var recipe in Recipes)
            items.Add(new SnbtString(recipe));

        return new SnbtList(items);
    }
}