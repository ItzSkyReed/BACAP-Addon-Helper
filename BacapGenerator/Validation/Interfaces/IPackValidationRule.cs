using BacapGenerator.Validation.Models;

namespace BacapGenerator.Validation.Interfaces;

/// <summary>
/// Defines a rule evaluated across the entire datapack hierarchy or asset graph.
/// </summary>
public interface IPackValidationRule : IValidationRule
{
    /// <summary>
    /// Evaluates the rule against the whole datapack graph.
    /// </summary>
    /// <param name="context">The execution validation context.</param>
    void Validate(ValidationContext context);
}