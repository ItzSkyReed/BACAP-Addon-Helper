using Spectre.Console;
using Spectre.Console.Rendering;

namespace UI.Styling;

/// <summary>
/// Decorator around <see cref="IAnsiConsole"/> that triggers a cancellation token when the Escape key is pressed.
/// </summary>
/// <param name="inner">The underlying console instance.</param>
public sealed class QKeyCancellableConsole(IAnsiConsole inner) : IAnsiConsole
{
    private QKeyCancellableInput? _input;

    public Profile Profile => inner.Profile;
    public IAnsiConsoleCursor Cursor => inner.Cursor;
    public IExclusivityMode ExclusivityMode => inner.ExclusivityMode;
    public RenderPipeline Pipeline => inner.Pipeline;
    public IAnsiConsoleInput Input => _input ?? inner.Input;
    public void WriteAnsi(Action<AnsiWriter> action) => inner.WriteAnsi(action);

    public void Clear(bool home) => inner.Clear(home);
    public void Write(IRenderable renderable) => inner.Write(renderable);

    /// <summary>
    /// Displays a prompt that can be cancelled by pressing the Escape key.
    /// </summary>
    /// <typeparam name="T">The result type of the prompt.</typeparam>
    /// <param name="prompt">The prompt specification to render.</param>
    /// <param name="cancellationToken">An optional external cancellation token.</param>
    /// <returns>A task that completes with the prompt result.</returns>
    /// <exception cref="OperationCanceledException">Thrown when the prompt is cancelled via Escape or the token.</exception>
    public async Task<T> PromptAsync<T>(IPrompt<T> prompt, CancellationToken cancellationToken = default)
    {
        using var escapeCts = new CancellationTokenSource();
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, escapeCts.Token);

        // Inject the input wrapper tied to the current invocation lifetime
        _input = new QKeyCancellableInput(inner.Input, escapeCts);

        try
        {
            return await AnsiConsoleExtensions.PromptAsync(this, prompt, linkedCts.Token);
        }
        finally
        {
            _input = null;
        }
    }

    private sealed class QKeyCancellableInput(IAnsiConsoleInput originalInput, CancellationTokenSource cts) : IAnsiConsoleInput
    {
        public bool IsKeyAvailable() => originalInput.IsKeyAvailable();

        public ConsoleKeyInfo? ReadKey(bool intercept)
        {
            var key = originalInput.ReadKey(intercept);
            if (key?.Key == ConsoleKey.Q)
            {
                cts.Cancel();
            }

            return key;
        }

        public async Task<ConsoleKeyInfo?> ReadKeyAsync(bool intercept, CancellationToken cancellationToken)
        {
            var key = await originalInput.ReadKeyAsync(intercept, cancellationToken);
            if (key?.Key == ConsoleKey.Q)
            {
                await cts.CancelAsync();
            }

            return key;
        }
    }
}