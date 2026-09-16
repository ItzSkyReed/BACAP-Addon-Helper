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
    /// Executes the validation engine for the given datapack and renders a formatted report.
    /// </summary>
    /// <param name="datapack">The datapack to validate.</param>
    /// <returns><see langword="true"/> if validation passed (no Errors); otherwise, <see langword="false"/>.</returns>
    public bool ValidateAndRenderReport(Datapack datapack)
    {
        if (datapack.Settings.Type == DatapackType.Reference)
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

        // Render diagnostics as a structured tree
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

                foreach (var issue in ruleGroup)
                {
                    var target = issue.Advancement?.McPath ?? "Pack-Level";
                    var pathSuffix = issue.PropertyPath != null ? $" -> {issue.PropertyPath}" : string.Empty;

                    ruleNode.AddNode($"[white]{target}[/][grey]{pathSuffix}[/]\n[{severityColor}]{issue.Message}[/]");
                }
            }
        }

        TuiTheme.Space();
        TuiTheme.RenderElement(tree);
        TuiTheme.Space();

        // Check if there are any blocking errors
        var hasErrors = issues.Any(i => i.Severity == ValidationSeverity.Error);

        if (!hasErrors)
            return true;

        TuiTheme.ShowError($"'{datapack.ReleaseName}' failed validation with blocking errors.");
        return false;

    }

    private static string GetSeverityColor(ValidationSeverity severity) => severity switch
    {
        ValidationSeverity.Error => "bold red",
        ValidationSeverity.Warning => "bold yellow",
        ValidationSeverity.Info => "bold cyan",
        _ => "grey"
    };
}