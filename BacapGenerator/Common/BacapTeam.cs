namespace BacapGenerator.Common;

public sealed record BacapTeam(string Color)
{

    public static readonly BacapTeam Aqua =  new("aqua");
    public static readonly BacapTeam Black = new("black");
    public static readonly BacapTeam Blue = new("blue");
    public static readonly BacapTeam DarkAqua =  new("dark_aqua");
    public static readonly BacapTeam DarkBlue =  new("dark_blue");
    public static readonly BacapTeam DarkGray =  new("dark_gray");
    public static readonly BacapTeam DarkGreen =  new("dark_green");
    public static readonly BacapTeam DarkPurple =  new("dark_purple");
    public static readonly BacapTeam DarkRed =  new("dark_red");
    public static readonly BacapTeam Gold =  new("gold");
    public static readonly BacapTeam Gray =  new("gray");
    public static readonly BacapTeam Green =  new("green");
    public static readonly BacapTeam Red = new("red");
    public static readonly BacapTeam White =  new("white");
    public static readonly BacapTeam Yellow =  new("yellow");

    /// <summary>
    /// Gets the collection of all predefined tabs.
    /// </summary>
    public static IReadOnlyCollection<BacapTeam> All { get; } =
    [
        Aqua, Black, Blue, DarkAqua, DarkBlue, DarkGray,
        DarkGreen, DarkPurple, DarkRed, Gold, Gray, Green,
        Red, White, Yellow
    ];
}