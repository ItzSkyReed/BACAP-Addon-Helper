using System.Diagnostics.CodeAnalysis;
using BacapGenerator.Models.Advancements.Functions;
using BacapGenerator.Models.Advancements.Functions.Trophy;
using BacapGenerator.Models.Datapacks;
using BacapGenerator.Models.Interfaces;
using BacapGenerator.Services;
using BacapGenerator.Utils;
using Core.Advancements.Models;
using Core.TextComponents.Components;
using Core.TextComponents.Extensions;
using Core.TextComponents.Models;
using JetBrains.Annotations;

namespace BacapGenerator.Models.Advancements;

/// <summary>
/// Represents a valid, playable BACAP advancement with UI elements and rewards.
/// Contains purely in-memory state.
/// </summary>
public class BacapAdvancement : ValidAdvancement
{
    private BacapAdvancementTab _tab;
    private BacapAdvancementTier _tier;

    public MacroFunction MacroFunction { get; internal set; } = null!;
    public ExpRewardFunction ExpRewardFunction { get; internal set; } = null!;
    public MsgFunction MsgFunction { get; internal set; } = null!;
    public ItemRewardFunction ItemRewardFunction { get; internal set; } = null!;
    public TrophyRewardFunction TrophyRewardFunction { get; internal set; } = null!;

    private BacapAdvancement(FileInfo file, Advancement advancement, IReadOnlyDatapack datapack, BacapAdvancementTab tab, BacapAdvancementTier tier)
        : base(file, advancement, datapack)
    {
        ArgumentNullException.ThrowIfNull(advancement);
        _tab = tab;
        _tier = tier;
    }

    /// <summary>
    /// Attempts to validate and create a new BacapAdvancement.
    /// Used by factories to avoid throwing exceptions on invalid JSON files.
    /// </summary>
    public static bool TryCreate(
        FileInfo file,
        Advancement advancement,
        IReadOnlyDatapack datapack,
        [NotNullWhen(true)] out BacapAdvancement? result,
        out AdvancementValidationError error)
    {
        result = null;

        if (!TryValidate(file, advancement, out var tab, out var tier, out error))
            return false;

        result = new BacapAdvancement(file, advancement, datapack, tab, tier);


        return true;
    }

    /// <summary>
    /// Core validation logic shared between factory creation and property mutation.
    /// </summary>
    private static bool TryValidate(
        FileInfo file,
        Advancement advancement,
        out BacapAdvancementTab tab,
        out BacapAdvancementTier tier,
        out AdvancementValidationError errorMessage)
    {
        tab = null!;
        tier = default;
        errorMessage = AdvancementValidationError.Unknown;

        if (advancement.Display?.Title is null)
        {
            errorMessage = AdvancementValidationError.MissingDisplay;
            return false;
        }

        if (string.IsNullOrWhiteSpace(advancement.Rewards?.Function))
        {
            errorMessage = AdvancementValidationError.MissingRewardFunction;
            return false;
        }

        if (!BacapUtils.TryExtractTab(advancement.Rewards.Function, out var tabName) ||
            !BacapAdvancementTab.TryFromFolderName(tabName, out tab!))
        {
            errorMessage = AdvancementValidationError.NotParsableTier;
            return false;
        }

        var color = advancement.Display.Description?.Style?.Color
                    ?? advancement.Display.Description?.GetTag<string>("color");

        if (BacapTierResolver.TryResolve(
                filename: Path.GetFileNameWithoutExtension(file.Name),
                tab: tab,
                hidden: advancement.Display.Hidden,
                frame: advancement.Display.Frame,
                descriptionColor: color,
                out tier))
            return true;
        errorMessage = AdvancementValidationError.NotParsableTier;
        return false;
    }

    public override string ToString() => $"{GetType().Name}({File}): {McPath} {Tier}";


    /// <summary>
    /// Gets or sets the underlying core <see cref="Advancement"/> data model.
    /// Mutating this property validates the advancement state and synchronizes attached function files.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when setting a <see langword="null"/> value.</exception>
    /// <exception cref="ArgumentException">Thrown when the new state fails BACAP tier validation.</exception>
    /// <exception cref="InvalidOperationException">Thrown when modifying a read-only instance.</exception>
    public override Advancement Advancement
    {
        get => base.Advancement;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (!TryValidate(File, value, out var newTab, out var newTier, out var error))
            {
                throw new ArgumentException($"Cannot update advancement: {error}", nameof(value));
            }

            _tab = newTab;
            _tier = newTier;
            base.Advancement = value;

            Sync();
            UpdateFunctionFilePaths();
        }
    }

/// <summary>
    /// Gets the display configuration guaranteed to exist for playable BACAP advancements.
    /// </summary>
    [PublicAPI]
    public AdvancementDisplay Display => Advancement.Display!;

    /// <summary>
    /// Gets the complete extracted plain text of the description.
    /// </summary>
    [PublicAPI]
    public string FullDescriptionText => DescriptionComponent.ExtractPlainText();

    /// <summary>
    /// Gets the extracted plain text of the first paragraph of the description.
    /// </summary>
    [PublicAPI]
    public string CleanDescriptionText => DescriptionComponent.ExtractFirstParagraph();

    /// <summary>
    /// Gets or sets the plain text or translation key of the advancement title, preserving any applied styles.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is <see langword="null"/> or empty.</exception>
    /// <example>
    /// <code>
    /// advancement.TitleText = "advancements.adventure.root.title";
    /// </code>
    /// </example>
    [PublicAPI]
    public string TitleText
    {
        get => (TitleComponent as TranslatableComponent)?.Translate
               ?? (TitleComponent as PlainTextComponent)?.Text
               ?? TitleComponent.ToString()!;
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);

            TitleComponent = TitleComponent switch
            {
                TranslatableComponent translatable => translatable with { Translate = value },
                PlainTextComponent plainText => plainText with { Text = value },
                _ => throw new InvalidOperationException(
                    $"Cannot change text directly on '{TitleComponent.GetType().Name}'. Expected TranslatableComponent or PlainTextComponent.")
            };
        }
    }

    /// <summary>
    /// Gets or sets the display title text component.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when setting a <see langword="null"/> value.</exception>
    [PublicAPI]
    public TextComponent TitleComponent
    {
        get => Display.Title!;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            // Reconstructs Display and triggers Advancement setter (Sync, validation, paths)
            Advancement = Advancement with
            {
                Display = Display with { Title = value }
            };
        }
    }

    /// <summary>
    /// Gets or sets the text content or translation key of the advancement description,
    /// preserving the component type (Translatable or PlainText), styles, and tier colors.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when attempting to update text on an unsupported component type.
    /// </exception>
    /// <example>
    /// <code>
    /// advancement.DescriptionText = "advancements.adventure.kill_a_mob.description";
    /// </code>
    /// </example>
    [PublicAPI]
    public string DescriptionText
    {
        get => (DescriptionComponent as TranslatableComponent)?.Translate
               ?? (DescriptionComponent as PlainTextComponent)?.Text
               ?? DescriptionComponent.ToString()!;
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);

            DescriptionComponent = DescriptionComponent switch
            {
                TranslatableComponent translatable => translatable with { Translate = value },
                PlainTextComponent plainText => plainText with { Text = value },
                _ => throw new InvalidOperationException(
                    $"Cannot change text directly on '{DescriptionComponent.GetType().Name}'. Expected TranslatableComponent or PlainTextComponent.")
            };
        }
    }

    /// <summary>
    /// Gets or sets the display description text component.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when setting a <see langword="null"/> value.</exception>
    [PublicAPI]
    public TextComponent DescriptionComponent
    {
        get => Display.Description!;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            // Reconstructs Display and triggers Advancement setter (Sync, validation, paths)
            Advancement = Advancement with
            {
                Display = Display with { Description = value }
            };
        }
    }

    [PublicAPI]
    public BacapAdvancementTier Tier
    {
        get => _tier;
        set
        {
            if (value == _tier)
                return;

            if (value == BacapAdvancementTier.Root)
                throw new ArgumentException("Creating Root advancements is not allowed.", nameof(value));

            var tierProfile = value.GetTierProfile();

            // Resolve rewards function path if the new tier mandates a specific tab
            var targetTab = tierProfile.RequiredTab ?? Tab;
            var updatedRewards = Advancement.Rewards;

            if (targetTab != Tab && !string.IsNullOrWhiteSpace(updatedRewards?.Function))
            {
                var updatedFunction = MinecraftUtils.ReplaceFirstPathSegment(updatedRewards.Function, targetTab.FolderName);
                updatedRewards = updatedRewards with { Function = updatedFunction };
            }

            var currentDescription = Display.Description ?? new PlainTextComponent(string.Empty);
            var updatedDescription = currentDescription with
            {
                Style = (currentDescription.Style ?? new TextStyle()) with
                {
                    Color = tierProfile.DescriptionColor
                }
            };

            // Atomically update state via Display accessor
            Advancement = Advancement with
            {
                Display = Display with
                {
                    Hidden = tierProfile.IsHidden,
                    Frame = tierProfile.Frame ?? Display.Frame,
                    Description = updatedDescription
                },
                Rewards = updatedRewards
            };
        }
    }

    /// <summary>
    /// Gets or sets the Minecraft path (McPath) of the BACAP advancement.
    /// </summary>
    /// <remarks>
    /// Overrides the base implementation to keep the rewards function path in sync with the advancement path,
    /// re-evaluating tabs, tiers, and attached function file locations.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when attempting to modify a read-only instance.</exception>
    /// <exception cref="ArgumentException">Thrown when the new path results in an invalid BACAP advancement state.</exception>
    /// <example>
    /// <code>
    /// bacapAdvancement.McPath = "bacap:adventure/kill_a_mob";
    /// </code>
    /// </example>
    [PublicAPI]
    public override string McPath
    {
        get => base.McPath;
        set
        {
            if (base.McPath == value)
                return;

            // Update base path and underlying File first so TryValidate sees the new filename
            base.McPath = value;

            // Synchronize rewards function path to match the new McPath
            var rewardNamespace = Datapack.Settings.RewardNamespace;
            var relativePath = MinecraftUtils.StripNamespace(value);
            var updatedFunction = $"{rewardNamespace}:{relativePath}";

            // Atomically update Advancement.
            // This triggers EnsureMutable(), runs TryValidate() with the new File and Tab,
            // updates _tab and _tier, and calls Sync() and UpdateFunctionFilePaths() with the new path.
            Advancement = Advancement with
            {
                Rewards = (Advancement.Rewards ?? new AdvancementRewards()) with
                {
                    Function = updatedFunction
                }
            };
        }
    }

    /// <summary>
    /// Gets or sets the BACAP tab.
    /// Changing the tab updates the rewards function namespace segment and synchronizes attached functions.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the resulting advancement state fails tier validation.</exception>
    [PublicAPI]
    public BacapAdvancementTab Tab
    {
        get => _tab;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (_tab == value)
                return;

            var currentFunc = Advancement.Rewards!.Function!;
            var updatedFunction = MinecraftUtils.ReplaceFirstPathSegment(currentFunc, value.FolderName);

            // Invoking this setter automatically triggers EnsureMutable(), TryValidate(),
            // updates _tab and _tier, and executes Sync() and UpdateFunctionFilePaths().
            Advancement = Advancement with
            {
                Rewards = Advancement.Rewards with
                {
                    Function = updatedFunction
                }
            };
        }
    }

    /// <summary>
    /// Gets or sets the parent advancement identifier (McPath).
    /// Mutating this property automatically extracts the tab from the parent path,
    /// updates the rewards function namespace segment, and recalculates both <see cref="Tab"/> and <see cref="Tier"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when attempting to modify a read-only instance or when the advancement tier cannot be resolved for the target tab.
    /// </exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/> for non-root advancements.</exception>
    /// <exception cref="ArgumentException">Thrown when the parent McPath contains an invalid BACAP tab identifier.</exception>
    /// <example>
    /// <code>
    /// advancement.Parent = "minecraft:adventure/root";
    /// </code>
    /// </example>
    [PublicAPI]
    public string? Parent
    {
        get => Advancement.Parent;
        set
        {
            if (Tier != BacapAdvancementTier.Root)
                ArgumentNullException.ThrowIfNull(value);

            if (Advancement.Parent == value)
                return;

            if (value is null)
            {
                Advancement = Advancement with { Parent = null };
                return;
            }

            // Extract the tab folder identifier from parent McPath (<namespace>:<tab>/<adv_name>)
            if (!BacapAdvancementTab.TryExtractTabFromMcPath(value, out var newTab))
            {
                throw new ArgumentException(
                    $"Cannot determine a valid BACAP tab from parent McPath '{value}'. Expected format: '<namespace>:<tab>/<advancement_name>'.",
                    nameof(value));
            }

            // Synchronize Rewards.Function path segment to match the new tab
            var updatedRewards = Advancement.Rewards;
            if (!string.IsNullOrWhiteSpace(updatedRewards?.Function))
            {
                var updatedFunction = MinecraftUtils.ReplaceFirstPathSegment(updatedRewards.Function, newTab.FolderName);
                updatedRewards = updatedRewards with { Function = updatedFunction };
            }

            Advancement = Advancement with
            {
                Parent = value,
                Rewards = updatedRewards
            };
        }
    }

    [PublicAPI]
    public void Sync()
    {
        EnsureMutable();

        MacroFunction.Update();
        MsgFunction.Update();
        ExpRewardFunction.Update();
        ItemRewardFunction.Update();
        TrophyRewardFunction.Update();
    }

    /// <summary>
    /// Recalculates and updates the FileInfo references of attached functions based on the current state.
    /// Does NOT instantiate new function objects, ensuring the AST is preserved in memory.
    /// </summary>
    private void UpdateFunctionFilePaths()
    {
        var relativePath = $"{MinecraftUtils.StripNamespace(Advancement.Rewards!.Function!)}.mcfunction";
        var basePath = Path.Combine(Datapack.DatapackDataPath.FullName, Datapack.Settings.RewardNamespace, "function");

        MacroFunction.File = new FileInfo(Path.Combine(basePath, relativePath));

        MsgFunction.File = new FileInfo(Path.Combine(basePath, "msg", relativePath));

        ExpRewardFunction.File = new FileInfo(Path.Combine(basePath, "exp", relativePath));

        ItemRewardFunction.File = new FileInfo(Path.Combine(basePath, "reward", relativePath));

        TrophyRewardFunction.File = new FileInfo(Path.Combine(basePath, "trophy", relativePath));
    }
}