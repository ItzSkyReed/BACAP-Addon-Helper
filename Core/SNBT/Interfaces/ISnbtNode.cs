namespace Core.SNBT.Interfaces;

public interface ISnbtNode
{
    string ToSnbtString(bool pretty = true, string indent = "");
}