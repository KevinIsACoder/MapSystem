namespace Runtime.AdvancedBundleSystem.Module
{
    public abstract class BaseModule : IModule
    {
        protected bool m_Initialized = false;
        protected bool m_Destroyed = false;
        public virtual void Init()
        {
            m_Initialized = true;
        }

        public virtual void Destroy()
        {
            m_Destroyed = true;
        }
    }
}
