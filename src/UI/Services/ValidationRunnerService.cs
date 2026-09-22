using BacapGenerator.Datapacks;
using BacapGenerator.Datapacks.Models;
using BacapGenerator.Datapacks.Models.Settings;
using BacapGenerator.Validation;
using BacapGenerator.Validation.Models;
using Spectre.Console;
using UI.Styling;

namespace UI.Services;

/// <summary>
/// A shared UI service responsible for running datapack validations and rendering diagnostic reports.
/// </summary>
public sealed class ValidationRunnerService(DatapackRegistry datapackRegistry)
{
    /// <summary>
    /// Executes the validation engine for the given datapack and renders a formatted report tree.
    /// </summary>
    /// <param name="datapack">The datapack to validate.</param>
    /// <returns><see langword="true"/> if validation passed with no blocking errors; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="datapack"/> is null.</exception>
    /// <example>
    /// <code>
    /// var runner = new ValidationRunnerService(registry);
    /// bool isValid = runner.ValidateAndRenderReport(datapack);
    /// </code>
    /// </example>
    public bool ValidateAndRenderReport(Datapack datapack)
    {
        ArgumentNullException.ThrowIfNull(datapack);

        if (datapack.Settings.DatapackType == DatapackType.Reference)
            return true;

        if (!datapack.Settings.Validation.Enabled)
        {
            TuiTheme.ShowInfo($"Validation is disabled for '{datapack.ReleaseName}'. Skipping.", "darkorange");
            return true;
        }

        var engine = DatapackValidationEngineFactory.CreateForDatapack(datapack, datapackRegistry);
        var issues = engine.Validate(datapack);

        if (issues.Count == 0)
        {
            TuiTheme.ShowSuccess($"Validation passed for '{datapack.ReleaseName}'!");
            return true;
        }

        var rootNodeMarkup = $"[bold cyan]Validation Report:[/] {datapack.ReleaseName}";
        var tree = TuiTheme.CreateTree(rootNodeMarkup);

        var severityGroups = issues
            .GroupBy(i => i.Severity)
            .OrderByDescending(g => g.Key);

        foreach (var group in severityGroups)
        {
            var severityColor = GetSeverityColor(group.Key);
            var severityNode = tree.AddNode($"[{severityColor}]{group.Key.ToString().ToUpperInvariant()}[/] ({group.Count()} issues)");

            var ruleGroups = group.GroupBy(i => i.RuleId);
            foreach (var ruleGroup in ruleGroups)
            {
                var ruleNode = severityNode.AddNode($"[grey]Rule:[/] {ruleGroup.Key}");

                // Grouping by target advancement and property
                var targetGroups = ruleGroup.GroupBy(i => new
                {
                    Target = i.Advancement?.McPath ?? "Pack-Level",
                    i.PropertyPath
                });

                foreach (var targetGroup in targetGroups)
                {
                    var pathSuffix = targetGroup.Key.PropertyPath is not null
                        ? $" -> {targetGroup.Key.PropertyPath}"
                        : string.Empty;

                    var targetNode = ruleNode.AddNode($"[white]{targetGroup.Key.Target}[/][grey]{pathSuffix}[/]");

                    foreach (var issue in targetGroup)
                    {
                        targetNode.AddNode($"[{severityColor}]{Markup.Escape(issue.Message)}[/]");
                    }
                }
            }
        }

        TuiTheme.Space();
        TuiTheme.RenderElement(tree);
        TuiTheme.Space();

        var hasErrors = issues.Any(i => i.Severity == ValidationSeverity.Error);
        if (!hasErrors)
            return true;

        TuiTheme.ShowError($"'{datapack.ReleaseName}' failed validation with blocking errors.");
        return false;
    }

    /// <summary>
    /// Maps a <see cref="ValidationSeverity"/> value to its corresponding Spectre.Console markup style.
    /// </summary>
    /// <param name="severity">The severity level to map.</param>
    /// <returns>A string representing Spectre.Console color tag markup.</returns>
    private static string GetSeverityColor(ValidationSeverity severity) => severity switch
    {
        ValidationSeverity.Error => "bold red",
        ValidationSeverity.Warning => "bold yellow",
        ValidationSeverity.Info => "bold cyan",
        _ => "grey"
    };
}