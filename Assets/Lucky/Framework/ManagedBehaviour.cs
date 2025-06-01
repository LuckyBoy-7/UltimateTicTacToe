namespace Lucky.Framework
{
    public abstract class ManagedBehaviour : ManagedBehaviourBase
    {
        protected virtual bool CanUpdate => true;

        #region Update

        protected virtual void ManagedUpdate()
        {
        }

        protected virtual void ManagedFixedUpdate()
        {
        }

        public sealed override void Update()
        {
            if (CanUpdate)
            {
                ManagedUpdate();
            }
        }

        public sealed override void FixedUpdate()
        {
            if (CanUpdate)
            {
                ManagedFixedUpdate();
            }
        }

        #endregion
    }
}