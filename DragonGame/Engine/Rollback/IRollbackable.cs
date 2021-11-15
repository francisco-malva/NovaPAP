namespace DragonGame.Engine.Rollback
{
    internal interface IRollbackable
    {
        ///<summary>
        /// Saves the state of the object to the StateBuffer.
        ///</summary>
        void Save(StateBuffer stateBuffer);

        ///<summary>
        /// Restores the state of the object from the StateBuffer.
        ///</summary>
        void Rollback(StateBuffer stateBuffer);
    }
}