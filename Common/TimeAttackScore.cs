#region

using System.Text.Json.Serialization;

#endregion

namespace Common;

[Serializable]
public class TimeAttackScore
{
    public TimeAttackScore(string name, long timeTaken)
    {
        Name = name;
        TimeTaken = timeTaken;
    }

    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("time-taken")] public long TimeTaken { get; set; }
}