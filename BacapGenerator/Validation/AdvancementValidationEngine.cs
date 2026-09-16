using BacapGenerator.Advancements.Models;
using BacapGenerator.Datapacks.Models;
using BacapGenerator.Validation.Interfaces;
using BacapGenerator.Validation.Models;

namespace BacapGenerator.Validation;

/// <summary>
/// Orchestrates and executes registered pack-level and single-advancement validation rules.
/// </summary>
public sealed class AdvancementValidationEngine
{
    private readonly List<ISingleAdvancementRule> _singleRules = [];
    private readonly List<IPackValidationRule> _packRules = [];

    /// <summary>
    /// Gets the collection of registered single advancement rules.
    /// </summary>
    public IReadOnlyList<ISingleAdvancementRule> SingleRules => _singleRules;

    /// <summary>
    /// Gets the collection of registered pack validation rules.
    /// </summary>
    public IReadOnlyList<IPackValidationRule> PackRules => _packRules;

    /// <summary>
    /// Registers a single-advancement rule into the execution pipeline.
    /// </summary>
    /// <param name="rule">The single advancement validation rule to register.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rule"/> is <see langword="null"/>.</exception>
    public void RegisterRule(ISingleAdvancementRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        _singleRules.Add(rule);
    }

    /// <summary>
    /// Registers a pack-level rule into the execution pipeline.
    /// </summary>
    /// <param name="rule">The pack-level validation rule to register.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rule"/> is <see langword="null"/>.</exception>
    public void RegisterRule(IPackValidationRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        _packRules.Add(rule);
    }

    /// <summary>
    /// Executes all registered rules across the specified datapack.
    /// </summary>
    /// <param name="datapack">The datapack instance to validate.</param>
    /// <returns>A read-only list containing all reported validation issues.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="datapack"/> is <see langword="null"/>.</exception>
    public IReadOnlyList<ValidationIssue> Validate(Datapack datapack)
    {
        ArgumentNullException.ThrowIfNull(datapack);

        var context = new ValidationContext
        {
            Datapack = datapack
        };

        // Run pack-wide topological and aggregate rules
        foreach (var packRule in _packRules)
            packRule.Validate(context);

        // Run per-item rules in a single traversal pass
        if (_singleRules.Count <= 0)
            return context.Diagnostics;

        foreach (var advancement in datapack.Advancements)
        {
            foreach (var singleRule in _singleRules)
            {
                singleRule.Validate(advancement, context);
            }
        }

        return context.Diagnostics;
    }

    /// <summary>
    /// Validates an isolated advancement without executing pack-wide topological rules.
    /// Useful for on-the-fly validation in editors.
    /// </summary>
    /// <param name="advancement">The target advancement to validate.</param>
    /// <param name="datapack">The parent datapack context.</param>
    /// <returns>A read-only list of validation issues specific to this advancement.</returns>
    /// <exception cref="ArgumentNullException">Thrown when any argument is <see langword="null"/>.</exception>
    public IReadOnlyList<ValidationIssue> ValidateSingle(ManagedAdvancement advancement, Datapack datapack)
    {
        ArgumentNullException.ThrowIfNull(advancement);
        ArgumentNullException.ThrowIfNull(datapack);

        var context = new ValidationContext
        {
            Datapack = datapack
        };

        foreach (var singleRule in _singleRules)
        {
            singleRule.Validate(advancement, context);
        }

        return context.Diagnostics;
    }
}