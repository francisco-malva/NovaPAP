﻿using System;
using DragonGame.Engine.Assets.Audio;
using DragonGame.Engine.Input;
using DragonGame.Engine.Scenes;
using DragonGame.Engine.Utilities;
using DragonGame.Engine.Wrappers.SDL2;
using DragonGame.Scenes.Game.Gameplay;
using DragonGame.Scenes.Game.Gameplay.Players.AI;
using DragonGame.Scenes.Game.Input;
using DragonGame.Scenes.MainMenu;
using Engine.Wrappers.SDL2;
using ManagedBass;
using SDL2;

namespace DragonGame.Scenes.Game
{
    internal enum AnnouncerType : byte
    {
        GetReady,
        Go,
        P1Wins,
        P2Wins,
        Draw
    }

    internal abstract class GameScene : Scene
    {
        public const int GameBorder = 32;
        public const int Width = 640;
        public const int Height = 480;

        public const byte GetReadyTime = 120;
        public const byte RoundEndTime = 120;

        private readonly Texture _gameBorder;

        private readonly AudioClip _getReady, _go, _p1Wins, _p2Wins, _draw;

        private readonly AudioClip _music;
        private readonly int _musicChannel;

        protected readonly GameField P1Field;
        protected readonly GameField P2Field;

        protected readonly DeterministicRandom Random;
        private int _announcerChannel;

        private GameState _state = GameState.GetReady;
        private byte _stateTimer;
        private Winner _winner;

        protected GameScene(byte roundsToWin, bool p1Ai = false, bool p2Ai = false,
            AiDifficulty difficulty = AiDifficulty.Easy)
        {
            FrameCount = 0;

            _gameBorder = Engine.Game.Instance.TextureManager["UI/game-border"];
            Random = new DeterministicRandom();

            P1Field = new GameField(roundsToWin, p1Ai, difficulty, Random);
            P2Field = new GameField(roundsToWin, p2Ai, difficulty, Random);

            _music = Engine.Game.Instance.AudioManager["mus-test"];

            _musicChannel = Bass.SampleGetChannel(_music.Handle, BassFlags.Loop);
            Bass.ChannelPlay(_musicChannel);


            _p1Wins = Engine.Game.Instance.AudioManager["p1-wins"];
            _p2Wins = Engine.Game.Instance.AudioManager["p2-wins"];
            _getReady = Engine.Game.Instance.AudioManager["get-ready"];
            _go = Engine.Game.Instance.AudioManager["go"];
            _draw = Engine.Game.Instance.AudioManager["draw"];
        }

        protected ulong FrameCount { get; private set; }

        private void PlayAnnouncer(AnnouncerType announcer)
        {
            AudioClip sample;

            switch (announcer)
            {
                case AnnouncerType.GetReady:
                    sample = _getReady;
                    break;
                case AnnouncerType.Go:
                    sample = _go;
                    break;
                case AnnouncerType.P1Wins:
                    sample = _p1Wins;
                    break;
                case AnnouncerType.P2Wins:
                    sample = _p2Wins;
                    break;
                case AnnouncerType.Draw:
                    sample = _draw;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(announcer), announcer, null);
            }

            _announcerChannel = Bass.SampleGetChannel(sample.Handle, BassFlags.Default);
            Bass.ChannelPlay(_announcerChannel);
        }

        protected void SetRoundsToWin(byte roundsToWin)
        {
            P1Field.RoundsToWin = roundsToWin;
            P2Field.RoundsToWin = roundsToWin;
        }

        private void EndRound(Winner winner)
        {
            _winner = winner;
            ChangeState(GameState.PlayerWon);
        }

        protected void SimulateAndDraw(GameInput p1Input, GameInput p2Input)
        {
            SimulateFrame(p1Input, p2Input);
            Draw();
            ++FrameCount;
        }

        protected static void ProcessInput(ref GameInput input,
            SDL.SDL_Scancode leftScanCode,
            SDL.SDL_Scancode rightScanCode, SDL.SDL_Scancode specialScanCode)
        {
            input = GameInput.None;

            input |= Keyboard.KeyDown(leftScanCode) ? GameInput.Left : GameInput.None;
            input |= Keyboard.KeyDown(rightScanCode) ? GameInput.Right : GameInput.None;
            input |= Keyboard.KeyDown(specialScanCode) ? GameInput.Special : GameInput.None;
        }

        private void DecideWinner()
        {
            switch (_winner)
            {
                case Winner.Both:
                    P1Field.WinRound(true);
                    P2Field.WinRound(true);
                    PlayAnnouncer(AnnouncerType.Draw);
                    break;
                case Winner.P1:
                    P1Field.WinRound();
                    P2Field.LoseRound();
                    PlayAnnouncer(AnnouncerType.P1Wins);
                    break;
                case Winner.P2:
                    P2Field.WinRound();
                    P1Field.LoseRound();
                    PlayAnnouncer(AnnouncerType.P2Wins);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected void ChangeState(GameState state)
        {
            _stateTimer = 0;
            _state = state;

            switch (_state)
            {
                case GameState.GetReady:
                    Bass.ChannelSetAttribute(_musicChannel, ChannelAttribute.Volume, 0.25f);
                    PlayAnnouncer(AnnouncerType.GetReady);
                    P1Field.GetReady();
                    P2Field.GetReady();
                    break;
                case GameState.InGame:
                    Bass.ChannelSetAttribute(_musicChannel, ChannelAttribute.Volume, 0.50f);
                    PlayAnnouncer(AnnouncerType.Go);
                    P1Field.BeginRound();
                    P2Field.BeginRound();
                    break;
                case GameState.PlayerWon:
                    Bass.ChannelSetAttribute(_musicChannel, ChannelAttribute.Volume, 0.25f);
                    DecideWinner();
                    break;
                case GameState.GameOver:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state));
            }
        }

        private void GetReadyUpdate(GameInput p1Input, GameInput p2Input)
        {
            if (_stateTimer == GetReadyTime)
            {
                ChangeState(GameState.InGame);
                return;
            }

            P1Field.Update(p1Input);
            P2Field.Update(p2Input);
        }

        private void InGameUpdate(GameInput p1Input, GameInput p2Input)
        {
            P1Field.Update(p1Input);
            P2Field.Update(p2Input);

            if (P1Field.PlayerWonRound && P2Field.PlayerWonRound)
                EndRound(Winner.Both);
            else if (P1Field.PlayerWonRound)
                EndRound(Winner.P1);
            else if (P2Field.PlayerWonRound) EndRound(Winner.P2);
        }

        private void PlayerWonUpdate(GameInput p1Input, GameInput p2Input)
        {
            P1Field.Update(p1Input);
            P2Field.Update(p2Input);

            if (_stateTimer == RoundEndTime)
            {
                if (P1Field.PlayerWonGame || P2Field.PlayerWonGame)
                    ChangeState(GameState.GameOver);
                else
                    ChangeState(GameState.GetReady);
            }
        }

        private void SimulateFrame(GameInput p1Input, GameInput p2Input)
        {
            switch (_state)
            {
                case GameState.GetReady:
                    GetReadyUpdate(p1Input, p2Input);
                    break;
                case GameState.InGame:
                    InGameUpdate(p1Input, p2Input);
                    break;
                case GameState.PlayerWon:
                    PlayerWonUpdate(p1Input, p2Input);
                    break;
                case GameState.GameOver:
                    Engine.Game.Instance.SceneManager.Set(new MainMenuScene());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            ++_stateTimer;
        }

        private void Draw()
        {
            var renderer = Engine.Game.Instance.Renderer;

            P1Field.Draw();
            P2Field.Draw();

            var p1Dest = new Rectangle(GameBorder / 2, GameBorder / 2, GameField.Width, GameField.Height);
            var p2Dest = new Rectangle(Width / 2 + GameBorder / 2, GameBorder / 2, GameField.Width, GameField.Height);

            renderer.SetDrawColor(Color.Black);
            renderer.Clear();
            renderer.Copy(_gameBorder, null, null);
            renderer.Copy(P1Field.OutputTexture, null, p1Dest);
            renderer.Copy(P2Field.OutputTexture, null, p2Dest);
            renderer.Present();
        }

        protected override void OnUnload()
        {
            OnGameEnd();
            P1Field.Dispose();
            P2Field.Dispose();
        }

        protected virtual void OnGameEnd()
        {
            Bass.ChannelStop(_musicChannel);
        }
    }
}