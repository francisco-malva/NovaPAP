#region

using System.Drawing;
using System.Numerics;
using DuckDuckJump.Engine.Subsystems.Graphical;
using DuckDuckJump.Game.Assets;
using DuckDuckJump.Game.GameWork.Camera;
using DuckDuckJump.Game.GameWork.Platforming;
using DuckDuckJump.Game.GameWork.Players;

#endregion

namespace DuckDuckJump.Game.GameWork.Items;

internal struct ItemBox
{
    public short PlatformId;
    public byte InvisibleTimer;

    private static readonly Vector2 CollisionExtents = new(32, 32);

    private Vector2 Position
    {
        get
        {
            var position = PlatformWork.GetPlatform(PlatformId).Position;

            position.X += Platform.Extents.Width / 2.0f - CollisionExtents.X / 2.0f;
            position.Y -= 50.0f;

            return position;
        }
    }

    private RectangleF CollisionBody => new((PointF)Position, (SizeF)CollisionExtents);


    public void Update()
    {
        _onScreen = CameraWork.Camera.Bounds.IntersectsWith(CollisionBody);

        if (!_onScreen)
            return;

        if (InvisibleTimer > 0)
            --InvisibleTimer;
    }

    public void DrawMe()
    {
        if (!_onScreen || InvisibleTimer > 0)
            return;

        Graphics.Draw(MatchAssets.Texture(MatchAssets.TextureIndex.ItemBox), null,
            Matrix3x2.CreateTranslation(Position),
            Color.White);
    }

    private bool _onScreen;

    public bool IntersectsPlayer(ref Player player)
    {
        return InvisibleTimer == 0 && CollisionBody.IntersectsWith(player.PushBox);
    }
}