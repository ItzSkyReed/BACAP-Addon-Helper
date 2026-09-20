using BacapGenerator.Datapacks;
using BacapGenerator.Datapacks.Models;
using BacapGenerator.Datapacks.Models.Settings;
using BacapGenerator.Validation.Rules;

namespace BacapGenerator.Validation;


/// <summary>
/// Creates tailored validation engines configured specifically for a given datapack instance.
/// </summary>
public static class DatapackValidationEngineFactory
{
    /// <summary>
    /// Builds an engine pre-configured with rules according to the datapack's settings.
    /// </summary>
    /// <param name="datapack">The target datapack to construct rules for.</param>
    /// <param name="datapackRegistry">Registry with all datapacks</param>
    /// <returns>A configured <see cref="AdvancementValidationEngine"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="datapack"/> is null.</exception>
    public static AdvancementValidationEngine CreateForDatapack(Datapack datapack, DatapackRegistry datapackRegistry)
    {
        ArgumentNullException.ThrowIfNull(datapack);

        var engine = new AdvancementValidationEngine();
        var settings = datapack.Settings;

        if (!settings.IsValidationActive())
            return engine; // Returns empty engine

        var validationConfig = settings.Validation;

        //  Title format
        if (validationConfig.TitleCase.Enabled)
            engine.RegisterRule(new TitleCaseRule(validationConfig.TitleCase));

        //  Whitespace trimming rules in titles
        if (validationConfig.WhitespaceTrimming.Enabled)
            engine.RegisterRule(new WhitespaceTrimmingRule(validationConfig.WhitespaceTrimming));

        //  Plain text in advancements
        if (validationConfig.PlainTextUsage.Enabled)
            engine.RegisterRule(new PlainTextInAdvancementsRule(validationConfig.PlainTextUsage));

        //  Plain text in advancements
        if (validationConfig.ParentReferences.Enabled)
            engine.RegisterRule(new AdvancementParentRule(validationConfig.ParentReferences, datapackRegistry));

        if (datapack.Settings.ShouldValidateRewardFiles())
            engine.RegisterRule(new RewardFunctionPathRule(validationConfig.RewardPaths));

        return engine;
    }
}