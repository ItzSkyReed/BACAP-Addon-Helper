using BacapGenerator.Configuration.Validation.Rules;
using BacapGenerator.Models.Advancements;
using BacapGenerator.Models.Datapacks.Settings;
using BacapGenerator.Validation.Extensions;
using BacapGenerator.Utils;
using BacapGenerator.Validation.Models;

namespace BacapGenerator.Validation.Rules;

/// <summary>
/// Validates reward function configuration paths, namespace alignment, and physical file existence on disk.
/// </summary>
/// <param name="options">The configuration options controlling severity and rule status.</param>
/// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <see langword="null"/>.</exception>
public sealed class RewardFunctionPathRule(GenericRuleOptions options)
    : SingleAdvancementRuleBase<GenericRuleOptions>(options)
{
    /// <inheritdoc/>
    public override string RuleId => "REWARD_FUNCTION_VALIDATION";

    /// <inheritdoc/>
    public override string DisplayName => "Reward Function Path & Files";

    /// <inheritdoc/>
    public override void Validate(ManagedAdvancement managedAdvancement, ValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (managedAdvancement is not BacapAdvancement bacapAdvancement)
            return;


        var datapack = context.Datapack;
        var settings = datapack.Settings;

        // Skip if rewards are not supported or required for this pack type / override state
        if (!settings.IsRewardModifiableAddon())
            return;

        var rewards = bacapAdvancement.Advancement.Rewards;

        // Check if reward function is missing entirely
        if (rewards is null || string.IsNullOrWhiteSpace(rewards.Function))
        {
            this.ReportIssue(
                context,
                "Reward function doesn't set.",
                bacapAdvancement,
                "Rewards.Function");
            return;
        }

        var actualRewardPath = rewards.Function;
        var expectedRewardNamespace = settings.RewardNamespace;
        var relativeMcPath = MinecraftUtils.StripNamespace(bacapAdvancement.McPath);
        var expectedRewardPath = $"{expectedRewardNamespace}:{relativeMcPath}";

        // Check namespace and path convention match
        if (!string.Equals(actualRewardPath, expectedRewardPath, StringComparison.OrdinalIgnoreCase))
        {
            this.ReportIssue(
                context,
                $"Reward function doesn't match:\nExpected '{expectedRewardPath}'\nGot '{actualRewardPath}'",
                bacapAdvancement,
                "Rewards.Function");
            return;
        }

        // Refresh and check physical files existence on disk
        bacapAdvancement.ExpRewardFunction.File.Refresh();
        bacapAdvancement.ItemRewardFunction.File.Refresh();
        bacapAdvancement.TrophyRewardFunction.File.Refresh();
        bacapAdvancement.MsgFunction.File.Refresh();
        bacapAdvancement.MacroFunction.File.Refresh();

        if (settings.RequiresExpReward(bacapAdvancement) && !bacapAdvancement.ExpRewardFunction.File.Exists)
            ReportMissingFile(bacapAdvancement, "Experience reward function file does not exist on disk.", context);

        if (settings.RequiresItemReward(bacapAdvancement) && !bacapAdvancement.ItemRewardFunction.File.Exists)
            ReportMissingFile(bacapAdvancement, "Item reward function file does not exist on disk.", context);

        if (settings.RequiresTrophyReward(bacapAdvancement) && !bacapAdvancement.TrophyRewardFunction.File.Exists)
            ReportMissingFile(bacapAdvancement, "Trophy reward function file does not exist on disk.", context);

        if (settings.RequiresExpReward(bacapAdvancement) && !bacapAdvancement.ExpRewardFunction.File.Exists)
            ReportMissingFile(bacapAdvancement, "Experience reward function file does not exist on disk.", context);

        if (!bacapAdvancement.MsgFunction.File.Exists)
            ReportMissingFile(bacapAdvancement, "Message function file does not exist on disk.", context);

        if (!bacapAdvancement.MacroFunction.File.Exists)
            ReportMissingFile(bacapAdvancement, "Macro function file does not exist on disk.", context);
    }

    /// <summary>
    /// Helper method to report a missing reward function file diagnostic.
    /// </summary>
    private void ReportMissingFile(BacapAdvancement advancement, string message, ValidationContext context)
    {
        this.ReportIssue(
            context,
            message,
            advancement,
            "Rewards.Function");
    }
}