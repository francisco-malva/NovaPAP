namespace DragonGame.Engine.Rollback
{
    internal interface IRollbackable
    {
        void Save(StateBuffer stateBuffer);
        void Rollback(StateBuffer stateBuffer);
    }
}