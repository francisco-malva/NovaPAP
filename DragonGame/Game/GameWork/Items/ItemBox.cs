using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using DuckDuckJump.Engine.Subsystems.Graphical;
using DuckDuckJump.Game.GameWork.Platforming;

namespace DuckDuckJump.Game.GameWork.Items;

[StructLayout(LayoutKind.Sequential)]
internal struct ItemBox
{
    public short PlatformId;
    public byte InvisibleTimer;

    private static readonly Vector2 CollisionExtents = new(32, 32);
    public Vector2 Position => PlatformWork.GetPlatform(PlatformId).Position;
    public RectangleF CollisionBody => new((PointF)Position, (SizeF)CollisionExtents);


    public void DrawMe()
    {
        Graphics.Draw(Assets.Texture(Assets.TextureIndex.ItemBox), null, Matrix3x2.CreateTranslation(Position),
            Color.White);
    }
}