using Core.Commands.Models.Interfaces;

namespace Core.McFunctions.Models.Extensions;

public static class CommandExtensions
{
    /// <summary>
    /// Wraps the command in an <see cref="ExecutableLine"/> so it can be added to an <see cref="McFunction"/>.
    /// </summary>
    /// <param name="command">The command to wrap.</param>
    /// <returns>An executable line representing the command.</returns>
    public static ExecutableLine ToLine(this ICommand command)
    {
        return new ExecutableLine(command);
    }
}