#region

using System;
using System.Drawing;
using System.IO;
using System.Numerics;
using Common.Utilities;
using DuckDuckJump.Engine.Subsystems.Flow;
using DuckDuckJump.Engine.Subsystems.Graphical;
using DuckDuckJump.Engine.Utilities;
using DuckDuckJump.Game.Input;

#endregion

namespace DuckDuckJump.Game;

internal static partial class Match
{
    internal static partial class PlayerWork
    {
        internal partial struct Player
        {
            private static readonly SizeF BodyExtents = new(45, 50);
            public Vector2 Position;
            public RectangleF PushBox => new(Position.X, Position.Y, BodyExtents.Width, BodyExtents.Height);

            public RectangleF PlatformBox =>
                new(Position.X + BodyExtents.Width * 0.25f, Position.Y + BodyExtents.Height * 0.5f,
                    BodyExtents.Width * 0.5f, BodyExtents.Height * 0.5f);

            private bool Descending => _yPosDelta > 5f;

            private static readonly Vector2[] InitialPositions =
            {
                new(Graphics.Midpoint.X / 2.0f, Graphics.LogicalSize.Height - BodyExtents.Height),
                new(Graphics.Midpoint.X * 1.5f, Graphics.LogicalSize.Height - BodyExtents.Height)
            };

            public bool Lost;
            private byte _myPlayerIndex;

            public void Reset(byte playerIndex)
            {
                _myPlayerIndex = playerIndex;
                Lost = false;

                if (IsCom) ResetComFields();


                _lastJumpedPlatform = -1;

                _velocity = Vector2.Zero;
                _xVelocity = 0.0f;
                _targetXVelocity = 0.0f;
                _xVelocityDelta = 0.0f;
                _yVelocity = 0.0f;

                Position = InitialPositions[playerIndex];
            }

            public void ClampPosition()
            {
                Position.X = Math.Clamp(Position.X, 0.0f, Graphics.LogicalSize.Width - BodyExtents.Width);

                if (!(Position.Y > Graphics.LogicalSize.Height - BodyExtents.Height)) return;
                Position.Y = Graphics.LogicalSize.Height - BodyExtents.Height;
                Jump();
            }

            public void UpdateMe(GameInput input)
            {
                switch (State)
                {
                    case MatchState.InGame:
                        if (IsCom)
                        {
                            ComGameUpdate();
                        }
                        else
                        {
                            HumanGameUpdate(input);
                            PlatformDetectionUpdate();
                        }

                        FallDetectionUpdate();
                        break;

                    case MatchState.Winner:
                        break;
                    case MatchState.GetReady:
                        break;
                    case MatchState.NotInitialized:
                        break;
                    case MatchState.Over:
                        break;
                    case MatchState.TimeUp:
                        break;
                    case MatchState.BeginningMessage:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            private void FallDetectionUpdate()
            {
                if (!CameraWork.Camera.Bounds.IntersectsWith(PushBox) && Position.Y > CameraWork.Camera.Bounds.Bottom)
                    Lost = true;
            }

            private void PlatformDetectionUpdate()
            {
                if (!Descending)
                    return;

                if (!PlatformWork.GetIntersectingPlatform(ref this, out _lastJumpedPlatform)) return;

                ref var platform = ref PlatformWork.GetPlatform(_lastJumpedPlatform);

                Position.Y = platform.Position.Y - BodyExtents.Height;

                CorrectCameraHeight(_lastJumpedPlatform);
                Jump();
            }

            private short _lastJumpedPlatform;

            public short LastJumpedPlatform => _lastJumpedPlatform;

            private void CorrectCameraHeight(short targetId)
            {
                BackgroundWork.SetTarget(targetId);

                ref var newTarget = ref PlatformWork.GetPlatform(targetId);

                var transformedY = newTarget.Position.Y - Graphics.Midpoint.Y * 1.5f;
                if (transformedY < CameraWork.Target.Y)
                    CameraWork.Target = new Vector2(CameraWork.Target.X, transformedY);
            }

            private float _yPosDelta;

            private static readonly Vector2 JumpMaxPoint = new(0.0f, 480.0f / 1.35f);

            private Vector2 GetTargetPosition(short targetIndex)
            {
                if (targetIndex < 0) return InitialPositions[_myPlayerIndex];

                return PlatformWork.GetPlatform(targetIndex).Position +
                       Vector2.UnitX * PlatformWork.Platform.Extents.Width / 2.0f -
                       Vector2.UnitX * BodyExtents.Width / 2.0f -
                       Vector2.UnitY * BodyExtents.Height;
            }

            private void ApplyHumanSpeed()
            {
                _xVelocity = Mathematics.SmoothDamp(_xVelocity, _targetXVelocity, ref _xVelocityDelta, 0.1f, GameFlow.TimeStep);
                _yVelocity += YDamping;

                _velocity = new Vector2(_xVelocity, _yVelocity);
                Position += _velocity;
            }

            public void ApplySpeed()
            {
                var prev = Position;

                if (IsCom)
                    ApplyComSpeed();
                else
                    ApplyHumanSpeed();

                _yPosDelta = Position.Y - prev.Y;
            }

            private void Jump(float multiplier = 1.0f)
            {
                JumpSfx();
                var velocity = JumpVelocity * multiplier;
                _yVelocity = velocity;
            }

            private void JumpSfx()
            {
                SoundEffectWork.Queue(Assets.SfxIndex.Jump, 0.25f);
            }

            private static readonly Color ComColor = Color.FromArgb(128, 128, 128, 128);

            public void DrawMe()
            {
                var color = IsCom ? ComColor : Color.White;
                Graphics.Draw(Assets.Texture(Assets.TextureIndex.Player), null,
                    Matrix3x2.CreateTranslation(Position), color);
            }

            public unsafe void SaveMe(Stream stream)
            {
                fixed (Player* ptr = &this)
                {
                    var store = new Span<byte>(ptr, sizeof(Player));
                    stream.Write(store);
                }
            }

            public unsafe void LoadMe(Stream stream)
            {
                fixed (Player* ptr = &this)
                {
                    var store = new Span<byte>(ptr, sizeof(Player));
                    stream.Read(store);
                }
            }
        }
    }
}