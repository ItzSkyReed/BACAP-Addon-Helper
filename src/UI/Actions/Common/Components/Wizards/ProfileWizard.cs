using Core.DataComponents.Components;
using Core.Items;
using Core.Registries;
using Spectre.Console;
using UI.Styling;

namespace UI.Actions.Common.Components.Wizards;

/// <summary>
/// Wizard for configuring player head skins via username (<c>minecraft:profile</c>).
/// </summary>
public class ProfileWizard : IComponentWizard
{
    /// <inheritdoc/>
    public string ComponentId => ProfileComponent.ComponentId;

    /// <inheritdoc/>
    public string DisplayTitle => "Player Profile / Head (minecraft:profile)";

    /// <inheritdoc/>
    public void Execute(ItemStack stack, MinecraftData mcData)
    {
        TuiTheme.RenderHeader("Player Head Profile Configuration");

        var currentProfile = stack.Components.Get<ProfileComponent>()?.Name ?? string.Empty;

        var username = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter player username (or leave empty to clear):")
                .DefaultValue(currentProfile)
                .AllowEmpty()
        ).Trim();

        if (string.IsNullOrWhiteSpace(username))
        {
            stack.Components.Remove<ProfileComponent>();
            TuiTheme.ShowSuccess("Profile component removed.");
        }
        else
        {
            stack.Components.Set(new ProfileComponent(username));
            TuiTheme.ShowSuccess($"Profile set to username '{username}'.");
        }

        TuiTheme.WaitForKey();
    }
}