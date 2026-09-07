using BacapGenerator.Models.Advancements.Functions;
using BacapGenerator.Models.Advancements.Functions.Trophy;
using BacapGenerator.Models.Interfaces;
using BacapGenerator.Services;
using BacapGenerator.Utils;
using Core.Advancements.Models;
using Core.TextComponents.Components;
using Core.TextComponents.Extensions;
using JetBrains.Annotations;

namespace BacapGenerator.Models.Advancements;

/// <summary>
/// Represents a valid, playable BACAP advancement with UI elements and rewards.
/// Contains purely in-memory state.
/// </summary>
public class BacapAdvancement : ManagedAdvancement
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
        out BacapAdvancement? result,
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

        var color = advancement.Display.Description?.GetTag<string>("color");

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
    /// Gets or sets the <see cref="Advancement"/> object.
    /// Mutating this property validates the new state.
    /// </summary>
    /// <summary>
    /// Gets or sets the <see cref="Advancement"/> object.
    /// Mutating this property validates the new state.
    /// </summary>
    // ReSharper disable once AnnotationConflictInHierarchy
    // ReSharper disable once UseNullableReferenceTypesAnnotationSyntax
    public override Advancement Advancement
    {
        get => base.Advancement!;
#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
        set
#pragma warning restore CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
        {
            EnsureMutable();
            ArgumentNullException.ThrowIfNull(value); // 3. Отсекаем null в рантайме

            if (!TryValidate(File, value, out var newTab, out var newTier, out var error))
            {
                throw new ArgumentException($"Cannot update advancement: {error}", nameof(value));
            }

            // Apply the new valid state. Disk is NOT touched.
            _tab = newTab;
            _tier = newTier;
            base.Advancement = value;

            Sync();
            UpdateFunctionFilePaths();
        }
    }

    public string TitleText
    {
        [PublicAPI]
        get => (TitleComponent as TranslatableComponent)?.Translate
               ?? (TitleComponent as PlainTextComponent)?.Text
               ?? TitleComponent.ToString()!;

        [PublicAPI]
        set
        {
            EnsureMutable();
            ArgumentException.ThrowIfNullOrEmpty(value);
            TitleComponent = new TranslatableComponent(value);
        }
    }

    [PublicAPI] public string FullDescriptionText => DescriptionComponent.ExtractPlainText();

    [PublicAPI] public string CleanDescriptionText => DescriptionComponent.ExtractFirstParagraph();

    [PublicAPI]
    public TextComponent TitleComponent
    {
        get => Advancement.Display!.Title!;
        set
        {
            EnsureMutable();
            ArgumentNullException.ThrowIfNull(value);

            if (Advancement == null)
                throw new InvalidOperationException("Cannot set Title on an uninitialized advancement Data object.");

            var currentDisplay = Advancement.Display ?? new AdvancementDisplay();
            Advancement = Advancement with { Display = currentDisplay with { Title = value } };

            Sync();
        }
    }

    public string DescriptionText
    {
        [PublicAPI]
        get => (DescriptionComponent as TranslatableComponent)?.Translate
               ?? (DescriptionComponent as PlainTextComponent)?.Text
               ?? DescriptionComponent.ToString()!;

        [PublicAPI]
        set
        {
            EnsureMutable();
            ArgumentException.ThrowIfNullOrEmpty(value);
            DescriptionComponent = new TranslatableComponent(value);
        }
    }

    [PublicAPI]
    public TextComponent DescriptionComponent
    {
        get => Advancement.Display!.Description!;
        set
        {
            EnsureMutable();
            ArgumentNullException.ThrowIfNull(value);

            if (Advancement == null)
                throw new InvalidOperationException("Cannot set Description on an uninitialized advancement Data object.");

            var currentDisplay = Advancement.Display ?? new AdvancementDisplay();
            Advancement = Advancement with { Display = currentDisplay with { Description = value } };

            Sync();
        }
    }

    /// <summary>
    /// Gets or sets the Minecraft path (McPath) of the BACAP advancement.
    /// </summary>
    /// <remarks>
    /// Overrides the base implementation to synchronize attached functions and update function file paths after the base file update.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when attempting to modify a read-only instance.</exception>
    /// <example>
    /// <code>
    /// bacapAdvancement.McPath = "bacap:tab/advancement_name";
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

            base.McPath = value;

            Sync();
            UpdateFunctionFilePaths();
        }
    }

    [PublicAPI]
    public BacapAdvancementTab Tab
    {
        get => _tab;
        set
        {
            EnsureMutable();
            ArgumentNullException.ThrowIfNull(value);

            if (_tab == value) return;

            var currentFunc = Advancement.Rewards!.Function!;
            var updatedFunction = MinecraftUtils.ReplaceFirstPathSegment(currentFunc, value.FolderName);

            Advancement = Advancement with
            {
                Rewards = Advancement.Rewards with
                {
                    Function = updatedFunction
                }
            };

            _tab = value;

            Sync();
            UpdateFunctionFilePaths();
        }
    }

    /// <summary>
    /// Gets or sets the parent advancement identifier (McPath).
    /// Mutating this property automatically extracts the tab from the parent path,
    /// synchronizes the rewards function path segment, and recalculates both <see cref="Tab"/> and <see cref="Tier"/>
    /// using <see cref="BacapTierResolver"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when attempting to modify a read-only instance or when <see cref="BacapTierResolver"/> fails to resolve a tier.
    /// </exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/> for non-root advancements.</exception>
    /// <exception cref="ArgumentException">Thrown when the parent McPath contains an unknown or invalid BACAP tab folder.</exception>
    /// <example>
    /// <code>
    /// // Sets parent, updating Tab to Adventure and Tier via BacapTierResolver
    /// advancement.Parent = "minecraft:adventure/root";
    /// </code>
    /// </example>
    [PublicAPI]
    public string? Parent
    {
        get => Advancement.Parent;
        set
        {
            EnsureMutable();

            if (Tier != BacapAdvancementTier.Root)
                ArgumentNullException.ThrowIfNull(value);

            if (Advancement.Parent == value)
                return;

            if (value is null)
            {
                base.Advancement = Advancement with { Parent = null };
                Sync();
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
            var currentFunc = Advancement.Rewards?.Function;
            var updatedRewards = Advancement.Rewards;
            if (!string.IsNullOrWhiteSpace(currentFunc))
            {
                var updatedFunction = MinecraftUtils.ReplaceFirstPathSegment(currentFunc, newTab.FolderName);
                updatedRewards = Advancement.Rewards! with { Function = updatedFunction };
            }

            // Resolve new Tier using BacapTierResolver with the updated tab and display properties
            var descriptionColor = Advancement.Display?.Description?.GetTag<string>("color")
                                   ?? Advancement.Display?.Description?.Style?.Color;

            var filename = Path.GetFileNameWithoutExtension(File.Name);
            var hidden = Advancement.Display?.Hidden ?? false;
            var frame = Advancement.Display?.Frame;

            if (!BacapTierResolver.TryResolve(filename, newTab, hidden, frame, descriptionColor, out var newTier))
            {
                throw new InvalidOperationException(
                    $"Cannot resolve tier for advancement '{filename}' in tab '{newTab.DisplayName}'.");
            }

            // Apply synchronized tab and tier
            _tab = newTab;
            _tier = newTier;

            base.Advancement = Advancement with
            {
                Parent = value,
                Rewards = updatedRewards
            };

            Sync();
            UpdateFunctionFilePaths();
        }
    }

    public BacapAdvancementTier Tier
    {
        get => _tier;
        set
        {
            EnsureMutable();
            if (value == BacapAdvancementTier.Root)
                throw new ArgumentException("Creating Root advancements is not allowed");

            _tier = value;
            Sync();
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
        var basePath = Path.Combine(Datapack.DatapackDataPath.ToString(), Datapack.Settings.RewardNamespace, "function");

        MacroFunction.File = new FileInfo(Path.Combine(basePath, relativePath));

        MsgFunction.File = new FileInfo(Path.Combine(basePath, "msg", relativePath));

        ExpRewardFunction.File = new FileInfo(Path.Combine(basePath, "exp", relativePath));

        ItemRewardFunction.File = new FileInfo(Path.Combine(basePath, "reward", relativePath));

        TrophyRewardFunction.File = new FileInfo(Path.Combine(basePath, "trophy", relativePath));
    }
}