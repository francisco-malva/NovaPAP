#region

using System.Numerics;
using DuckDuckJump.Engine.Subsystems.Graphical;
using DuckDuckJump.Engine.Utilities;

#endregion

namespace DuckDuckJump.Game;

internal static partial class Match
{
    public static class CameraWork
    {
        public static readonly Camera Camera;
        public static Vector2 Target;
        private static float _cameraX;
        private static float _cameraY;
        private static float _cameraXVelocity;
        private static float _cameraYVelocity;

        static CameraWork()
        {
            Camera = new Camera();
        }

        public static void Reset()
        {
            Target = Vector2.Zero;
            _cameraX = 0.0f;
            _cameraY = 0.0f;
            _cameraXVelocity = 0.0f;
            _cameraYVelocity = 0.0f;
        }

        public static void UpdateMe()
        {
            _cameraX = Mathematics.SmoothDamp(_cameraX, Target.X, ref _cameraXVelocity, 0.35f);
            _cameraY = Mathematics.SmoothDamp(_cameraY, Target.Y, ref _cameraYVelocity, 0.35f);

            Camera.Position = new Vector2(_cameraX, _cameraY);
        }
    }
}