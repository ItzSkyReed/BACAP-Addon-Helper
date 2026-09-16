using BacapGenerator.Models.Advancements;
using BacapGenerator.Validation.Models;

namespace BacapGenerator.Validation.Interfaces;

/// <summary>
/// Defines a rule evaluated against individual advancements in isolation.
/// </summary>
public interface ISingleAdvancementRule : IValidationRule
{
    /// <summary>
    /// Evaluates the rule against a specific managed advancement instance.
    /// </summary>
    /// <param name="advancement">The advancement to inspect.</param>
    /// <param name="context">The execution validation context.</param>
    void Validate(ManagedAdvancement advancement, ValidationContext context);
}