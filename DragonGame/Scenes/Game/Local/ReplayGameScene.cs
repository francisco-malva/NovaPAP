namespace DuckDuckJump.Scenes.Game.Local;

internal class ReplayGameScene : GameScene
{
    private Replay _replay;

    public ReplayGameScene(Replay replay) : base(replay.Info)
    {
        _replay = replay;
    }

    public override void OnTick()
    {
    }

    protected override void OnGameEnd()
    {
        base.OnGameEnd();
    }
}