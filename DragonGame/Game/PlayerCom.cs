#region

using System;
using DuckDuckJump.Engine.Utilities;

#endregion

namespace DuckDuckJump.Game;

internal static partial class Match
{
    internal static partial class PlayerWork
    {
        internal unsafe partial struct Player
        {
            private enum ComState : byte
            {
                Thinking,
                Acting
            }

            private readonly struct ComActionData
            {
                public readonly int PlatformRange;
                public readonly float ClimbingSpeed;
                public readonly byte ActTimeMax;

                public ComActionData(int platformRange, float climbingSpeed, byte actTimeMax)
                {
                    PlatformRange = platformRange;
                    ClimbingSpeed = climbingSpeed;
                    ActTimeMax = actTimeMax;
                }
            }

            private static readonly ComActionData[] ComActionTable =
            {
                new(1, 0.015f, 5)
            };

            private bool IsCom => _info.ComLevels[_myPlayerIndex] > 0;
            private byte MyComLevel => _info.ComLevels[_myPlayerIndex];
            private ref ComActionData MyComData => ref ComActionTable[MyComLevel - 1];
            private byte _comActTime;
            private int _comBegin;
            private int _comEnd;
            private float _comProgress;
            private ComState _comState;


            private fixed int _comPath[2048];
            private int _comPathLength;

            private void GenerateComPath()
            {
                _comPathLength = 0;
            }

            private void GenerateComStopped(int position)
            {
                InsertIntoPath(position);
                InsertIntoPath(position);
            }

            private void InsertIntoPath(int position)
            {
                _comPath[_comPathLength++] = position;
            }

            private void ResetComFields()
            {
                _comBegin = -1;
                _comEnd = -1;
                _comProgress = 0.0f;
                _comActTime = MyComData.ActTimeMax;
                _comState = ComState.Thinking;
                _comXBeginOffset = 0.0f;

                GenerateComPath();
            }

            private void ComGameUpdate()
            {
                _comProgress += MyComData.ClimbingSpeed;

                if (PlatformWork.GetIntersectingPlatform(ref this, out var index))
                {
                    _comXBeginOffset = Position.X;
                    _comBegin = index;
                    _comEnd = index + 1;
                    CorrectCameraHeight(ref PlatformWork.GetPlatform(_comBegin));
                    SetComState(ComState.Acting);
                }

                if (_comProgress < 1.0f) return;

                switch (_comState)
                {
                    case ComState.Thinking:
                        ++_comActTime;
                        if (_comActTime >= MyComData.ActTimeMax)
                        {
                            _comActTime = 0;
                            _comBegin = _comEnd;
                            _comEnd += Math.Min(RandomWork.Next(1, MyComData.PlatformRange + 1), _info.PlatformCount);
                            SetComState(ComState.Acting);
                        }
                        else
                        {
                            _comProgress %= 1.0f;
                        }

                        break;
                    case ComState.Acting:
                        _comXBeginOffset = Position.X;
                        _comBegin = _comEnd;
                        SetComState(ComState.Thinking);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            private void SetComState(ComState state)
            {
                _comState = state;
                _comProgress = 0.0f;
                _comActTime = 0;
            }

            private static float _comXBeginOffset;

            private void ApplyComSpeed()
            {
                var progress = Math.Clamp(_comProgress, 0.0f, 1.0f);

                var begin = GetTargetPosition(_comBegin);

                if (_comXBeginOffset != 0.0f) begin.X = _comXBeginOffset;

                var end = GetTargetPosition(_comEnd);

                var p2 = begin - JumpMaxPoint;
                var p3 = end - JumpMaxPoint;

                Position = Mathematics.CubicBezier(begin, p2, p3, end, progress);
            }
        }
    }
}