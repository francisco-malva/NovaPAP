#region

using System;
using DuckDuckJump.Engine.Subsystems.Flow;

#endregion

namespace DuckDuckJump;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        GameFlow.Run();
    }
}