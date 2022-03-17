namespace ScoringServer.Paths;

[AttributeUsage(AttributeTargets.Class)]
public class PathAttribute : Attribute
{
    public readonly string PathName;

    public PathAttribute(string pathName)
    {
        PathName = pathName;
    }
}