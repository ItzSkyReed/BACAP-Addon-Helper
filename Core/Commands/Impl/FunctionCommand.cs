using System.Text;
using System.Text.Json.Serialization;
using Core.Commands.Models;
using Core.SNBT.Nodes;

namespace Core.Commands.Impl;

/// <summary>
/// Specifies the source type for the 'with' macro argument resolution in the /function command.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FunctionWithSourceType
{
    /// <summary>
    /// No 'with' clause is specified.
    /// </summary>
    None,

    /// <summary>
    /// Arguments are sourced from a block entity's NBT.
    /// </summary>
    Block,

    /// <summary>
    /// Arguments are sourced from an entity's NBT.
    /// </summary>
    Entity,

    /// <summary>
    /// Arguments are sourced from command storage.
    /// </summary>
    Storage
}

/// <summary>
/// Represents the /function command, which runs a function or functions in a tag.
/// </summary>
/// <param name="CommandName">The resource location of the function or function tag (prefixed with #).</param>
/// <param name="Arguments">Optional SNBT compound tag for direct macro arguments.</param>
/// <param name="WithSourceType">The type of source for the 'with' macro arguments.</param>
/// <param name="WithSource">The source (block position, entity selector, or storage id) for the 'with' clause.</param>
/// <param name="WithPath">The optional NBT path for the 'with' clause.</param>
/// <param name="IsMacro">Specifies whether this command is a macro.</param>
public record FunctionCommand(
    string CommandName,
    SnbtCompound? Arguments = null,
    FunctionWithSourceType WithSourceType = FunctionWithSourceType.None,
    string? WithSource = null,
    string? WithPath = null,
    bool IsMacro = false
) : CommandBase(IsMacro)
{
    protected override string BuildInternal()
    {
        var sb = new StringBuilder($"function {CommandName}");

        if (Arguments != null)
            sb.Append($" {Arguments.ToSnbtString(pretty: false)}");

        else if (WithSourceType != FunctionWithSourceType.None)
        {
            var typeStr = WithSourceType.ToString().ToLowerInvariant();
            sb.Append($" with {typeStr} {WithSource}");

            if (!string.IsNullOrWhiteSpace(WithPath))
                sb.Append($" {WithPath}");
        }

        return sb.ToString();
    }
}