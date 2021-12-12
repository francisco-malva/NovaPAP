using System;
using DuckDuckJump.Engine.Utilities;
using DuckDuckJump.Scenes.Game.Gameplay;
using DuckDuckJump.Scenes.Game.Input;
using DuckDuckJump.Scenes.MainMenu;
using SDL2;

namespace DuckDuckJump.Scenes.Game.Network;

internal class OnlineGameScene : GameScene
{
    private readonly bool _onLeftSide;
    protected StreamReaderWriter Stream;

    public OnlineGameScene(GameInfo info, StreamReaderWriter stream, bool onLeftSide) : base(info)
    {
        Stream = stream;
        _onLeftSide = onLeftSide;
    }

    public override void OnTick()
    {
        try
        {
            var gameInput = GameInput.None;

            ProcessInput(ref gameInput, SDL.SDL_Scancode.SDL_SCANCODE_A, SDL.SDL_Scancode.SDL_SCANCODE_D,
                SDL.SDL_Scancode.SDL_SCANCODE_S);

            Stream.Writer.Write((byte)gameInput);
            var foreignInput = (GameInput)Stream.Reader.ReadByte();

            SimulateAndDraw(_onLeftSide ? gameInput : foreignInput, _onLeftSide ? foreignInput : gameInput);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Engine.Game.Instance.SceneManager.Set(new MainMenuScene());
        }
    }

    protected override void OnGameEnd()
    {
        Stream.Dispose();
        base.OnGameEnd();
    }
}