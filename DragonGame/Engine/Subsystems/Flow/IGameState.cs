#region

using SDL2;

#endregion

namespace DuckDuckJump.Engine.Subsystems.Flow;

public interface IGameState
{
    void Initialize();
    void Exit();
    void OnEvent(ref SDL.SDL_Event sdlEvent);
    void Update();
    void Draw();
}