using System.Diagnostics;
using DuckDuckJump.Engine;
using DuckDuckJump.Engine.Scenes;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics;
using DuckDuckJump.Game;
using DuckDuckJump.Game.Gameplay.Resources;
using DuckDuckJump.Game.Input.InputHandlers;
using SDL2;

namespace DuckDuckJump.Scenes.Game;

internal class SingleMatchGameScene : Scene
{
    private readonly PhysicalInputHandler _inputHandler = new(
        new InputProfile(SDL.SDL_Scancode.SDL_SCANCODE_A, SDL.SDL_Scancode.SDL_SCANCODE_D,
            SDL.SDL_Scancode.SDL_SCANCODE_S, SDL.SDL_Scancode.SDL_SCANCODE_ESCAPE),
        new InputProfile(SDL.SDL_Scancode.SDL_SCANCODE_J, SDL.SDL_Scancode.SDL_SCANCODE_L,
            SDL.SDL_Scancode.SDL_SCANCODE_L, SDL.SDL_Scancode.SDL_SCANCODE_P));

    private readonly GameMatch _match;

    private readonly Renderer _renderer;
    private readonly GameplayResources _resources;

    public SingleMatchGameScene(GameInfo info)
    {
        Debug.Assert(GameContext.Instance != null, "Engine.Game.Instance != null");
        _renderer = GameContext.Instance.Renderer;
        _resources = new GameplayResources(ResourceManager);

        _match = new GameMatch(info, _resources);
    }

    public override void OnTick()
    {
        _match.Update(_inputHandler);

        _match.Draw();
        _renderer.Present();
    }

    protected override void OnUnload()
    {
        _resources.Dispose();
    }
}