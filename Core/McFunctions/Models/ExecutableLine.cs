using Core.Commands.Models.Interfaces;
using Core.McFunctions.Models.Interfaces;

namespace Core.McFunctions.Models;

/// <summary>
/// Represents an executable command within the function.
/// </summary>
/// <param name="Command">The underlying command object.</param>
public record ExecutableLine(ICommand Command) : IMcFunctionLine
{
    public string Build() => Command.Build();
}