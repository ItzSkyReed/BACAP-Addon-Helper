namespace UI.Interfaces;
/// <summary>
/// Base interface for any executable screen or command in the TUI.
/// </summary>
public interface ITuiAction
{
    /// <summary>
    /// Gets the display name of the action shown in the menu.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Executes the action logic.
    /// </summary>
    Task ExecuteAsync();
}

// Marker interfaces for specific menus
public interface IMainMenuAction : ITuiAction { }
public interface IManageAdvancementsAction : ITuiAction { }
/// <summary>
/// Marker interface for actions available in the Debug Advancements sub-menu.
/// </summary>
public interface IDebugAdvancementsAction : ITuiAction { }