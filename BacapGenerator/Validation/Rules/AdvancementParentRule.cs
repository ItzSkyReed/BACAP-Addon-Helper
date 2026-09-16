using System.Diagnostics.CodeAnalysis;
using BacapGenerator.Advancements.Models;
using BacapGenerator.Datapacks;
using BacapGenerator.Datapacks.Models.Settings.Validation.Rules;
using BacapGenerator.Validation.Extensions;
using BacapGenerator.Validation.Models;

namespace BacapGenerator.Validation.Rules;

/// <summary>
/// Validates that an advancement's parent exists across all loaded datapacks in the registry
/// and detects circular parent references.
/// </summary>
/// <param name="options">The configuration options specifying rule status and diagnostic severity.</param>
/// <param name="datapackRegistry">The central registry of loaded datapacks used to resolve parent advancements.</param>
/// <exception cref="ArgumentNullException">
/// Thrown when <paramref name="options"/> or <paramref name="datapackRegistry"/> is <see langword="null"/>.
/// </exception>
public sealed class AdvancementParentRule(GenericRuleOptions options, DatapackRegistry datapackRegistry)
    : SingleAdvancementRuleBase<GenericRuleOptions>(options)
{
    private readonly DatapackRegistry _datapackRegistry = datapackRegistry ?? throw new ArgumentNullException(nameof(datapackRegistry));

    /// <inheritdoc/>
    public override string RuleId => "INVALID_PARENT";

    /// <inheritdoc/>
    public override string DisplayName => "Advancement Parent Validation";

    /// <summary>
    /// Validates that the target advancement specifies a valid, existing, and non-cyclic parent advancement.
    /// </summary>
    /// <param name="managedAdvancement">The advancement to inspect.</param>
    /// <param name="context">The shared validation execution context where issues are recorded.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <see langword="null"/>.</exception>
    public override void Validate(ManagedAdvancement managedAdvancement, ValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (managedAdvancement is not BacapAdvancement bacapAdvancement)
            return;

        // Skip root advancements
        if (IsRootAdvancement(bacapAdvancement))
            return;

        var parentMcPath = bacapAdvancement.Parent;

        // Non-root advancement without a specified parent
        if (string.IsNullOrWhiteSpace(parentMcPath))
        {
            this.ReportIssue(
                context,
                "Non-root advancement is missing a parent specification.",
                managedAdvancement,
                "Parent");
            return;
        }

        // Check if parent exists across any loaded datapack in the registry
        if (!TryFindAdvancement(parentMcPath, out var parentAdvancement))
        {
            this.ReportIssue(
                context,
                $"Invalid parent '{parentMcPath}': advancement was not found in any loaded datapack.",
                managedAdvancement,
                "Parent");
            return;
        }

        // Circular reference detection ( A -> B -> A)
        CheckForCycles(managedAdvancement, parentAdvancement, context);
    }

    /// <summary>
    /// Traverses parent references to ensure no cyclic hierarchy loops back to the starting advancement.
    /// </summary>
    /// <param name="origin">The advancement being evaluated.</param>
    /// <param name="parent">The resolved direct parent advancement.</param>
    /// <param name="context">The validation context used for reporting.</param>
    private void CheckForCycles(
        ManagedAdvancement origin,
        ManagedAdvancement parent,
        ValidationContext context)
    {
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            origin.McPath
        };

        var current = parent;

        while (current is not null)
        {
            if (!visited.Add(current.McPath))
            {
                this.ReportIssue(
                    context,
                    $"Circular parent reference detected: '{origin.McPath}' leads to a loop at '{current.McPath}'.",
                    origin,
                    "Parent");
                return;
            }

            if (current is not BacapAdvancement bacapCurrent ||
                IsRootAdvancement(bacapCurrent) ||
                string.IsNullOrWhiteSpace(bacapCurrent.Parent))
            {
                break;
            }

            TryFindAdvancement(bacapCurrent.Parent, out current);
        }
    }

    /// <summary>
    /// Searches for an advancement by its Minecraft resource path across all datapacks in the registry.
    /// </summary>
    /// <param name="mcPath">The advancement Minecraft resource path (e.g. <c>"bacap:adventure/root"</c>).</param>
    /// <param name="advancement">When found, contains the matching managed advancement instance; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if found in any datapack; otherwise, <see langword="false"/>.</returns>
    private bool TryFindAdvancement(string mcPath, [NotNullWhen(true)] out ManagedAdvancement? advancement)
    {
        foreach (var datapack in _datapackRegistry.Values)
        {
            if (datapack.TryGetAdvancement(mcPath, out advancement))
                return true;
        }

        advancement = null;
        return false;
    }

    /// <summary>
    /// Determines whether an advancement represents a root tab node.
    /// </summary>
    /// <param name="advancement">The advancement to check.</param>
    /// <returns><see langword="true"/> if the advancement is a root node; otherwise, <see langword="false"/>.</returns>
    private static bool IsRootAdvancement(BacapAdvancement advancement)
    {
        return advancement.Tier == BacapAdvancementTier.Root;
    }
}