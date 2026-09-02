namespace Core.SNBT.Interfaces;

public interface ISnbtNode
{
    string ToSnbtString(bool pretty = false, string indent = "");
}