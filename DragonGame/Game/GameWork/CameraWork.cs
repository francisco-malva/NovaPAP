#region

using System.IO;
using System.Numerics;
using Common.Utilities;
using DuckDuckJump.Engine.Subsystems.Flow;
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

        public static void SaveMe(Stream stream)
        {
            Camera.Save(stream);
            stream.Write(Target);
            stream.Write(_cameraX);
            stream.Write(_cameraY);
            stream.Write(_cameraXVelocity);
            stream.Write(_cameraYVelocity);
        }

        public static void LoadMe(Stream stream)
        {
            Camera.Load(stream);
            Target = stream.Read<Vector2>();
            _cameraX = stream.Read<float>();
            _cameraY = stream.Read<float>();
            _cameraXVelocity = stream.Read<float>();
            _cameraYVelocity = stream.Read<float>();
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
            _cameraX = Mathematics.SmoothDamp(_cameraX, Target.X, ref _cameraXVelocity, 0.35f, GameFlow.TimeStep);
            _cameraY = Mathematics.SmoothDamp(_cameraY, Target.Y, ref _cameraYVelocity, 0.35f, GameFlow.TimeStep);

            Camera.Position = new Vector2(_cameraX, _cameraY);
        }
    }
}