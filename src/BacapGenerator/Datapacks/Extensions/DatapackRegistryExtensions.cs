using BacapGenerator.Datapacks.Models;
using BacapGenerator.Datapacks.Models.Settings;

namespace BacapGenerator.Datapacks.Extensions;

/// <summary>
/// Provides query extensions for <see cref="DatapackRegistry"/> to resolve related datapack groupings.
/// </summary>
public static class DatapackRegistryExtensions
{
    /// <summary>
    /// Represents a primary addon paired with its associated compatibility addons.
    /// </summary>
    /// <param name="Primary">The primary parent addon datapack.</param>
    /// <param name="CompatibilityAddons">The ordered list of compatibility addons extending the primary pack.</param>
    public record AddonGroup(Datapack Primary, IReadOnlyList<Datapack> CompatibilityAddons)
    {
        /// <summary>
        /// Returns all datapacks belonging to this group, starting with the primary addon.
        /// </summary>
        public IEnumerable<Datapack> All => [Primary, ..CompatibilityAddons];
    }

    /// <summary>
    /// Locates all primary addon datapacks that configure a language pack and resolves their compatibility addons.
    /// </summary>
    /// <param name="registry">The source datapack registry.</param>
    /// <returns>A collection of resolved addon groups.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="registry"/> is <see langword="null"/>.</exception>
    /// <example>
    /// <code>
    /// foreach (var group in registry.GetLanguagePackAddonGroups())
    /// {
    ///     Console.WriteLine($"{group.Primary.ReleaseName} has {group.CompatibilityAddons.Count} compat addons");
    /// }
    /// </code>
    /// </example>
    public static IReadOnlyList<AddonGroup> GetLanguagePackAddonGroups(this DatapackRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        var primaryAddons = registry.Values
            .Where(dp => dp.Settings is { DatapackType: DatapackType.Addon, LanguagePackSettigs: not null } &&
                         !string.IsNullOrWhiteSpace(dp.Settings.LanguagePackSettigs.Path))
            .ToList();

        var groups = new List<AddonGroup>(primaryAddons.Count);
        groups.AddRange(
            from primary in primaryAddons
            let compatAddons = registry.Values.Where(dp =>
                    dp.Settings.DatapackType == DatapackType.CompatibilityAddon
                    && string.Equals(dp.Settings.ParentDatapackId, primary.Id, StringComparison.OrdinalIgnoreCase))
                .ToList()
            select new AddonGroup(primary, compatAddons));

        return groups;
    }
}