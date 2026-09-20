namespace Core.SNBT;

public static class Snbt
{
    public static SnbtCompoundBuilder Compound() => new SnbtCompoundBuilder();
    public static SnbtListBuilder List() => new SnbtListBuilder();
}