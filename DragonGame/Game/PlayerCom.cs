#region

using System;
using DuckDuckJump.Engine.Subsystems.Flow;
using DuckDuckJump.Engine.Utilities;

#endregion

namespace DuckDuckJump.Game;

internal static partial class Match
{
    internal static partial class PlayerWork
    {
        internal unsafe partial struct Player
        {
            private readonly struct ComActionData
            {
                public readonly byte PlatformRange;
                public readonly float StoppedChance;

                public ComActionData(byte platformRange, float stoppedChance)
                {
                    PlatformRange = platformRange;
                    StoppedChance = stoppedChance;
                }
            }

            private static readonly ComActionData[] ComActionTable =
            {
                new(1, 0.5f),
                new(1, 0.25f),
                new(2, 0.015f),
                new(2, 0.005f),
                new(3, 0.005f),
                new(3, 0.0025f),
                new(4, 0.0015f),
                new(4, 0.0f)
            };

            private bool IsCom => _info.ComLevels[_myPlayerIndex] > 0;
            private byte MyComLevel => _info.ComLevels[_myPlayerIndex];
            private ref ComActionData MyComData => ref ComActionTable[MyComLevel - 1];
            private short _comPathIndex;
            private float _comProgress;


            private const short MaxPathSize = 2048;
            private fixed short _comPath[MaxPathSize];
            private fixed float _randomOffsets[MaxPathSize];
            private short _comPathLength;

            private void GenerateComPath()
            {
                _comPathLength = 0;

                InsertIntoPath(-1);
                InsertIntoPath(0);
                short currentProgress = 0;

                while (currentProgress < _info.PlatformCount)
                {
                    var shouldStop = MyComData.StoppedChance != 0.0f &&
                                     RandomWork.Next(0.0f, 1.0f) <= MyComData.StoppedChance;

                    if (shouldStop)
                    {
                        GenerateComStopped(currentProgress);
                    }
                    else
                    {
                        ++currentProgress;
                        var nextPlatform =
                            (short) (currentProgress + RandomWork.Next((byte) 1, MyComData.PlatformRange));
                        InsertIntoPath(currentProgress);
                        InsertIntoPath(nextPlatform);
                        currentProgress = nextPlatform;
                    }
                }

                for (var i = 0; i < _comPathLength; i++) _randomOffsets[i] = RandomWork.Next(-20.0f, 20.0f);
            }

            private void GenerateComStopped(short position)
            {
                InsertIntoPath(position);
                InsertIntoPath(position);
            }

            private void InsertIntoPath(short position)
            {
                _comPath[_comPathLength++] = position;
            }

            private void ResetComFields()
            {
                _comPathIndex = 0;
                _comProgress = 0.0f;

                GenerateComPath();
            }

            private void ComGameUpdate()
            {
                _comProgress += GameFlow.TimeStep;

                if (!(_comProgress >= 1.0f)) return;

                JumpSfx();
                _comProgress %= 1.0f;
                ++_comPathIndex;
                _lastJumpedPlatform = _comPath[_comPathIndex];

                if (_lastJumpedPlatform >= 0)
                    CorrectCameraHeight(_lastJumpedPlatform);
            }

            private void ApplyComSpeed()
            {
                var progress = Math.Clamp(_comProgress, 0.0f, 1.0f);

                var begin = GetTargetPosition(_comPath[_comPathIndex]);
                begin.X += _randomOffsets[_comPathIndex];

                var endIndex = Math.Clamp(_comPathIndex + 1, 0, _comPathLength - 1);

                var end = GetTargetPosition(_comPath[endIndex]);
                end.X += _randomOffsets[endIndex];

                var p2 = begin - JumpMaxPoint;
                var p3 = end - JumpMaxPoint;

                Position = Mathematics.CubicBezier(begin, p2, p3, end, progress);
            }
        }
    }
}