using BacapGenerator.Models.Advancements.Functions;
using BacapGenerator.Models.Advancements.Functions.Trophy;
using BacapGenerator.Models.Interfaces;
using BacapGenerator.Services;
using BacapGenerator.Utils;
using Core.Advancements.Models;
using Core.TextComponents.Components;
using JetBrains.Annotations;

namespace BacapGenerator.Models.Advancements;

/// <summary>
/// Represents a valid, playable BACAP advancement with UI elements and rewards.
/// </summary>
public class BacapAdvancement : ManagedAdvancement
{
    private BacapAdvancementTab _tab;
    private BacapAdvancementTier _tier;
    private string _mcPath;
    public IReadOnlyDatapack Datapack { get; }

    public MacroFunction MacroFunction { get; private set; } = null!;
    public ExpRewardFunction ExpRewardFunction { get; private set; } = null!;
    public MsgFunction MsgFunction { get; private set; } = null!;
    public ItemRewardFunction ItemRewardFunction { get; private set; } = null!;
    public TrophyRewardFunction TrophyRewardFunction { get; private set; } = null!;


    private BacapAdvancement(FileInfo file, Advancement advancement, IReadOnlyDatapack datapack, BacapAdvancementTab tab,
        BacapAdvancementTier tier, string mcPath)
        : base(file, advancement)
    {
        ArgumentNullException.ThrowIfNull(advancement);
        _tab = tab;
        _tier = tier;
        _mcPath = mcPath;
        Datapack = datapack;
    }


    /// <summary>
    /// Attempts to validate and create a new BacapAdvancement.
    /// Used by factories to avoid throwing exceptions on invalid JSON files.
    /// </summary>
    /// <param name="file">The physical file information.</param>
    /// <param name="advancement">The parsed Core JSON model.</param>
    /// <param name="datapack">The datapack of the advancement</param>
    /// <param name="result">The resulting object if validation succeeds.</param>
    /// <param name="errorMessage">The error message if validation fails.</param>
    /// <returns>True if the advancement is valid BACAP format; otherwise, false.</returns>
    public static bool TryCreate(
        FileInfo file,
        Advancement advancement,
        IReadOnlyDatapack datapack,
        out BacapAdvancement? result,
        out string? errorMessage)
    {
        result = null;

        // Use the shared validation logic
        if (!TryValidate(file, advancement, datapack, out var tab, out var tier, out var mcpath, out errorMessage))
            return false; // Creation failed, no exception thrown


        result = new BacapAdvancement(file, advancement, datapack, tab, tier, mcpath);

        UpdateFilePaths(result);

        return true;
    }

    /// <summary>
    /// Core validation logic shared between factory creation and property mutation.
    /// </summary>
    private static bool TryValidate(
        FileInfo file,
        Advancement advancement,
        IReadOnlyDatapack datapack,
        out BacapAdvancementTab tab,
        out BacapAdvancementTier tier,
        out string mcpath,
        out string? errorMessage)
    {
        tab = null!;
        tier = default;
        mcpath = string.Empty;
        errorMessage = null;

        if (advancement.Display?.Title is null)
        {
            errorMessage = "A valid BACAP advancement must contain a Display with a Title.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(advancement.Rewards?.Function))
        {
            errorMessage = "A valid BACAP advancement must contain a Reward Function.";
            return false;
        }

        if (!BacapUtils.TryExtractTab(advancement.Rewards.Function, out var tabName) ||
            !BacapAdvancementTab.TryFromFolderName(tabName, out tab!))
        {
            errorMessage = "Unknown or missing BACAP advancement tab in Reward Function.";
            return false;
        }

        var color = advancement.Display.Description?.GetTag<string>("color");

        if (!BacapTierResolver.TryResolve(
                filename: file.Name,
                tab: tab,
                hidden: advancement.Display.Hidden,
                frame: advancement.Display.Frame,
                descriptionColor: color,
                out tier))
        {
            errorMessage = $"Unable to resolve BACAP tier for file '{file.Name}'.";
            return false;
        }

        var relativePath = Path.GetRelativePath(datapack.DatapackDataPath.ToString(), file.FullName);

        mcpath = MinecraftUtils.ToMinecraftPath(relativePath);

        return true;
    }

    public override string ToString() => $"{GetType().Name}({File}): {_mcPath} {Tier}";

    /// <summary>
    /// Gets or sets the <see cref="Advancement"/> object.
    /// Mutating this property validates the new state.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when setting a null value.</exception>
    /// <exception cref="ArgumentException">Thrown when the new advancement data breaks BACAP rules.</exception>
    /// <summary>
    /// Gets or sets the <see cref="Advancement"/> object.
    /// Mutating this property validates the new state.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when setting a null value.</exception>
    /// <exception cref="ArgumentException">Thrown when the new advancement data breaks BACAP rules.</exception>
    public override Advancement Advancement
    {
        get => base.Advancement!;
        set
        {
            EnsureMutable();
            ArgumentNullException.ThrowIfNull(value);

            // Pass 'Datapack' context here to correctly resolve the relative path inside TryValidate.
            // If validation fails, we MUST throw to prevent invalid state.
            if (!TryValidate(File, value, Datapack, out var newTab, out var newTier, out var newMcPath, out var error))
            {
                throw new ArgumentException($"Cannot update advancement: {error}", nameof(value));
            }

            DeleteFilesFromDisk();

            // Apply the new valid state
            _tab = newTab;
            _tier = newTier;
            _mcPath = newMcPath;
            base.Advancement = value;

            Sync();
            UpdateFilePaths(this);
        }
    }


    /// <summary>
    /// Gets or sets the translation key string for the advancement title.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when attempting to set a null, empty, or whitespace string.</exception>
    /// <example>
    /// <code>
    /// adv.TitleText = "advancements.adventure.kill_a_mob.title";
    /// </code>
    /// </example>
    public string TitleText
    {
        get => (TitleComponent as TranslatableComponent)?.Translate
               ?? (TitleComponent as PlainTextComponent)?.Text
               ?? TitleComponent.ToString()!;

        set
        {
            EnsureMutable();
            ArgumentException.ThrowIfNullOrEmpty(value);
            TitleComponent = new TranslatableComponent(value);
        }
    }

    /// <summary>
    /// Gets or sets the root <see cref="TextComponent"/> for the advancement title.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when advancement data or display is missing.</exception>
    /// <example>
    /// <code>
    /// adv.TitleComponent = new TranslatableComponent("my.adv.key", Style: new TextStyle(Color: "gold"));
    /// </code>
    /// </example>
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

    [PublicAPI]
    public string McPath
    {
        get => _mcPath;

        set
        {
            if (_mcPath == value) return;

            EnsureMutable();

            DeleteFilesFromDisk();

            _mcPath = value;

            var physicalPath = MinecraftUtils.ResolvePhysicalPath(Datapack.DatapackDataPath.ToString(), "advancement", _mcPath);
            File = new FileInfo(physicalPath);

            Sync();
            UpdateFilePaths(this);
        }
    }

    /// <summary>
    /// Gets or sets the predefined <see cref="BacapAdvancementTab"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when setting a null value.</exception>
    [PublicAPI]
    public BacapAdvancementTab Tab
    {
        get => _tab;
        set
        {
            EnsureMutable();
            ArgumentNullException.ThrowIfNull(value);

            if (_tab == value)
                return;

            var currentFunc = Advancement.Rewards!.Function!;
            var updatedFunction = MinecraftUtils.ReplaceFirstPathSegment(currentFunc, value.FolderName);

            Advancement = Advancement with
            {
                Rewards = Advancement.Rewards with
                {
                    Function = updatedFunction
                }
            };

            DeleteFilesFromDisk();

            UpdateFilePaths(this);

            _tab = value;
            Sync();
        }
    }

    /// <summary>
    /// Gets or sets the parent McPath string.
    /// </summary>
    [PublicAPI]
    public string? Parent
    {
        get => Advancement.Parent;
        set
        {
            EnsureMutable();
            if (Tier != BacapAdvancementTier.Root)
                ArgumentNullException.ThrowIfNull(value);

            Advancement = Advancement with
            {
                Parent = value
            };
        }
    }


    /// <summary>
    /// Gets or sets the predefined BACAP advancement <see cref="BacapAdvancementTier"/>.
    /// </summary>
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

    /// <summary>
    /// Synchronizes all internal ASTs and models in memory without touching the disk.
    /// Called automatically when mutable properties change.
    /// </summary>
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
    /// Flushes all changes to disk. Acts like a database commit.
    /// </summary>
    [PublicAPI]
    public void Save()
    {
        EnsureMutable();

        Sync();

        File.Directory?.Create();
        System.IO.File.WriteAllText(File.FullName, Advancement.ToJson());
        File.Refresh();

        MacroFunction.WriteFile();
        MsgFunction.WriteFile();
        ExpRewardFunction.WriteFile();
        ItemRewardFunction.WriteFile();
        TrophyRewardFunction.WriteFile();
    }

    /// <summary>
    /// Deletes the advancement JSON and all associated function files from their current paths on disk.
    /// </summary>
    private void DeleteFilesFromDisk()
    {
        EnsureMutable();

        if (File.Exists) File.Delete();

        DeleteIfExists(MacroFunction.File);
        DeleteIfExists(MsgFunction.File);
        DeleteIfExists(ExpRewardFunction.File);
        DeleteIfExists(ItemRewardFunction.File);
        DeleteIfExists(TrophyRewardFunction.File);
        return;

        static void DeleteIfExists(FileInfo fileInfo)
        {
            if (fileInfo.Exists) fileInfo.Delete();
        }
    }

    /// <summary>
    /// Recalculates and updates FileInfo references for the advancement and all sub-functions based on the new tab.
    /// </summary>
    private static void UpdateFilePaths(BacapAdvancement advancement)
    {
        var relativePath = $"{MinecraftUtils.StripNamespace(advancement.Advancement.Rewards!.Function!)}.mcfunction";

        var macroPath = Path.Combine(advancement.Datapack.DatapackDataPath.ToString(),
            advancement.Datapack.Settings.RewardNamespace, "function", relativePath);
        var msgPath = Path.Combine(advancement.Datapack.DatapackDataPath.ToString(),
            advancement.Datapack.Settings.RewardNamespace, "function", "msg", relativePath);
        var expRewardPath = Path.Combine(advancement.Datapack.DatapackDataPath.ToString(),
            advancement.Datapack.Settings.RewardNamespace, "function", "exp", relativePath);
        var itemRewardPath = Path.Combine(advancement.Datapack.DatapackDataPath.ToString(),
            advancement.Datapack.Settings.RewardNamespace, "function", "reward", relativePath);
        var trophyRewardPath = Path.Combine(advancement.Datapack.DatapackDataPath.ToString(),
            advancement.Datapack.Settings.RewardNamespace, "function", "trophy", relativePath);

        advancement.MacroFunction = new MacroFunction(new FileInfo(macroPath), advancement);
        advancement.MsgFunction = new MsgFunction(new FileInfo(msgPath), advancement);
        advancement.ExpRewardFunction = new ExpRewardFunction(new FileInfo(expRewardPath), advancement);
        advancement.ItemRewardFunction = new ItemRewardFunction(new FileInfo(itemRewardPath), advancement);
        advancement.TrophyRewardFunction = new TrophyRewardFunction(new FileInfo(trophyRewardPath), advancement);
    }
}