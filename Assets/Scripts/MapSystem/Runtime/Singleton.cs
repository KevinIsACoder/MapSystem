namespace MapSystem.Runtime
{
    public class Singleton<T> where T : class, new()
    {
        private static T m_instance;
        private static readonly object m_lockObj = new object();
        public static T Instance 
        {
            get
            {
                lock (m_lockObj)
                {
                    m_instance ??= new T();
                }
                return m_instance;
            }
        }
    }
}