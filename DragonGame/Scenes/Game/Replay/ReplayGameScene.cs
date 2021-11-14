using DragonGame.Scenes.Game.Input;

namespace DragonGame.Scenes.Game.Replay
{
    internal class ReplayGameScene : GameScene
    {
        private readonly Replay _replay;
        private GameInput _p1Input, _p2Input;

        public ReplayGameScene(Replay replay) : base(0)
        {
            _replay = replay;
            _replay.Setup(Random, this);
        }

        protected override void RunFrame()
        {
            base.RunFrame();
            _replay.FetchInput(FrameCount, ref _p1Input, ref _p2Input);

            Update(_p1Input, _p2Input);
            Draw();
        }
    }
}