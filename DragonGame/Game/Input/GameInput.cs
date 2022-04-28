#region

using System;

#endregion

namespace DuckDuckJump.Game.Input;

[Flags]
internal enum GameInput : byte
{
    None = 0,
    Left = 1,
    Right = 2,
    Special = 4
}