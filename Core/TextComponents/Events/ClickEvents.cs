using Core.SNBT;
using Core.SNBT.Interfaces;
using JetBrains.Annotations;

namespace Core.TextComponents.Events;

public abstract record ClickEvent(string Action)
{
    [PublicAPI]
    public ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound().Put("action", Action);
        Populate(builder);
        return builder.Build();
    }

    protected abstract void Populate(SnbtCompoundBuilder builder);
}

[PublicAPI]
public record OpenUrlClickEvent(string Url) : ClickEvent("open_url")
{
    protected override void Populate(SnbtCompoundBuilder builder) => builder.Put("url", Url);
}
[PublicAPI]
public record OpenFileClickEvent(string Path) : ClickEvent("open_file")
{
    protected override void Populate(SnbtCompoundBuilder builder) => builder.Put("path", Path);
}
[PublicAPI]

public record RunCommandClickEvent(string Command) : ClickEvent("run_command")
{
    protected override void Populate(SnbtCompoundBuilder builder) => builder.Put("command", Command);
}

[PublicAPI]
public record SuggestCommandClickEvent(string Command) : ClickEvent("suggest_command")
{
    protected override void Populate(SnbtCompoundBuilder builder) => builder.Put("command", Command);
}

[PublicAPI]
public record ChangePageClickEvent(int Page) : ClickEvent("change_page")
{
    protected override void Populate(SnbtCompoundBuilder builder) => builder.Put("page", Page);
}

[PublicAPI]
public record CopyToClipboardClickEvent(string Value) : ClickEvent("copy_to_clipboard")
{
    protected override void Populate(SnbtCompoundBuilder builder) => builder.Put("value", Value);
}

[PublicAPI]
public record ShowDialogClickEvent(ISnbtNode Dialog) : ClickEvent("show_dialog")
{
    protected override void Populate(SnbtCompoundBuilder builder) => builder.Put("dialog", Dialog);
}

[PublicAPI]
public record CustomClickEvent(string Id, string? Payload = null) : ClickEvent("custom")
{
    protected override void Populate(SnbtCompoundBuilder builder)
    {
        builder.Put("id", Id);
        if (Payload != null)
            builder.Put("payload", Payload);
    }
}