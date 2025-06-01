namespace Lucky.Framework.Inputs_
{
    public abstract class VirtualInput
    {
        public VirtualInput()
        {
            Inputs.Register(this);
        }

        public void Deregister()
        {
            Inputs.Register(this);
        }

        public abstract void Update();
    }
}