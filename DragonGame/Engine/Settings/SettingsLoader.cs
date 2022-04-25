#region

using System.IO;
using System.Xml.Serialization;

#endregion

namespace DuckDuckJump.Engine.Settings;

public static class SettingsLoader
{
    public static readonly GameSettings Current;

    static SettingsLoader()
    {
        var serializer = new XmlSerializer(typeof(GameSettings));

        if (!File.Exists("config.xml"))
        {
            Current = new GameSettings(64, 64);
        }
        else
        {
            using var file = File.OpenRead("config.xml");
            Current = (GameSettings) serializer.Deserialize(file) ?? new GameSettings(64, 64);
        }
    }

    public static void Save()
    {
        var serializer = new XmlSerializer(typeof(GameSettings));

        using var file = File.OpenWrite("config.xml");
        serializer.Serialize(file, Current);
    }
}