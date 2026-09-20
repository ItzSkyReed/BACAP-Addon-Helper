namespace Core.Commands.Models;

public readonly record struct Selector
{
    public string Value { get; }

    private Selector(string value) => Value = value;

    public static Selector AllPlayers { get; } = new("@a");
    public static Selector SelectedPlayer { get; } = new("@s");
    public static Selector AllEntities { get; } = new("@e");
    public static Selector NearestPlayer { get; } = new("@n");
    public static Selector RandomPlayer { get; } = new("@r");

    /// <summary>
    /// Creates a custom selector from the specified string value.
    /// </summary>
    /// <param name="value">The raw selector string.</param>
    /// <returns>A new <see cref="Selector"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is null or empty.</exception>
    public static Selector Custom(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return new Selector(value);
    }

    public override string ToString() => Value;

    /// <summary>
    /// Explicitly converts a string value to a <see cref="Selector"/>.
    /// </summary>
    public static explicit operator Selector(string value) => Custom(value);

    /// <summary>
    /// Implicitly converts a <see cref="Selector"/> to its string representation.
    /// </summary>
    public static implicit operator string(Selector selector) => selector.Value;
}