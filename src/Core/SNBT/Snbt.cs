namespace Core.SNBT;

public static class Snbt
{
    public static SnbtCompoundBuilder Compound() => new();
    public static SnbtListBuilder List() => new();
}