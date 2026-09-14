using BacapGenerator.Validation.Context;
using BacapGenerator.Validation.Interfaces;
using BacapGenerator.Validation.Models;
using BacapGenerator.Models.Advancements;

namespace BacapGenerator.Validation.Extensions;


/// <summary>
/// Provides reporting extensions to decouple issue creation boilerplate from rules.
/// </summary>
public static class ValidationReportingExtensions
{
    /// <summary>
    /// Creates and records a validation issue using the rule's metadata.
    /// </summary>
    /// <param name="rule">The rule reporting the issue.</param>
    /// <param name="context">The target validation context.</param>
    /// <param name="message">The description of the encountered issue.</param>
    /// <param name="advancement">The affected advancement, if applicable.</param>
    /// <param name="propertyPath">The path to the invalid property.</param>
    public static void ReportIssue(
        this IValidationRule rule,
        ValidationContext context,
        string message,
        ManagedAdvancement? advancement = null,
        string? propertyPath = null)
    {
        if (rule.Severity == ValidationSeverity.Ignore)
            return;

        context.Report(new ValidationIssue(
            rule.RuleId,
            rule.Severity,
            message,
            advancement,
            propertyPath));
    }
}