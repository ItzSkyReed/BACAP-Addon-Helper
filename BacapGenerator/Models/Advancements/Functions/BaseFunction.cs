using Core.Advancements.Models;
using Core.McFunctions.Models;
using JetBrains.Annotations;
using Pidgin;

namespace BacapGenerator.Models.Advancements.Functions;

/// <summary>
/// Base class for all loaded advancements in the workspace.
/// </summary>
public abstract class BaseFunction
{
    protected BacapAdvancement BacapAdvancement { get; }

    [PublicAPI]
    public McFunction Function
    {
        get;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    }

    [PublicAPI]
    public FileInfo File
    {
        get;
        private set
        {
            ArgumentNullException.ThrowIfNull(value);

            // 1. Проверяем, что field НЕ null (т.е. это НЕ первый вызов из конструктора)
            // 2. И только потом сравниваем пути
            if (field != null && !string.Equals(field.FullName, value.FullName, StringComparison.OrdinalIgnoreCase))
            {
                // Delete the old file if it exists on disk
                if (System.IO.File.Exists(field.FullName))
                    System.IO.File.Delete(field.FullName);

                // Ensure destination directory exists
                value.Directory?.Create();

                // Write the current function content to the new destination
                // БЕЗОПАСНОСТЬ: используем оператор ?., так как на этапе инициализации Function может быть null
                // И используем Build(), так как мы писали его для McFunction ранее!
                var content = Function?.Build() ?? string.Empty;
                System.IO.File.WriteAllText(value.FullName, content);

                value.Refresh();
            }

            // Сохраняем новое значение в backing field
            field = value;
        }
    }

    /// <summary>
    /// Gets the corresponding advancement data model.
    /// </summary>
    [PublicAPI]
    public Advancement Advancement => BacapAdvancement.Advancement;

    internal BaseFunction(FileInfo file, BacapAdvancement bacapAdvancement)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(bacapAdvancement);

        BacapAdvancement = bacapAdvancement;
        File = file;


        try
        {
            var content = file.Exists
                ? System.IO.File.ReadAllText(file.FullName)
                : string.Empty;

            Function = McFunction.Parse(content);
        }
        catch (ParseException ex)
        {
            throw new InvalidOperationException(
                $"Unable to parse function file '{file.FullName}' for advancement {Advancement}", ex);
        }
    }

    /// <summary>
    /// Writes the current <see cref="Function"/> content to the physical file represented by <see cref="File"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="Function"/> is null or uninitialized.</exception>
    /// <example>
    /// <code>
    /// baseFunction.WriteFile();
    /// </code>
    /// </example>
    [PublicAPI]
    public void WriteFile()
    {
        BacapAdvancement.EnsureMutable();

        File.Directory?.Create();
        System.IO.File.WriteAllText(File.FullName, Function.Build());
        File.Refresh();
    }

    /// <summary>
    /// Asynchronously writes the current <see cref="Function"/> content to the physical file represented by <see cref="File"/>.
    /// </summary>
    /// <param name="cancellationToken">An optional token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="Function"/> is null or uninitialized.</exception>
    /// <example>
    /// <code>
    /// await baseFunction.WriteFileAsync(cancellationToken);
    /// </code>
    /// </example>
    [PublicAPI]
    public async Task WriteFileAsync(CancellationToken cancellationToken = default)
    {
        BacapAdvancement.EnsureMutable();

        File.Directory?.Create();
        await System.IO.File.WriteAllTextAsync(File.FullName, Function.Build(), cancellationToken);
        File.Refresh();
    }


    /// <summary>
    /// Updates the function's internal state (commands, arguments, etc.)
    /// based on the current state of the parent Advancement.
    /// </summary>
    [PublicAPI]
    public abstract void Update();

    public override string ToString() => $"{GetType().Name}({File.Name})";
}