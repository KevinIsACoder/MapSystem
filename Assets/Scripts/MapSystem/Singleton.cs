namespace MapSystem
{
    public class Singleton<T> where T : class, new()
    {
        private T m_instance;
        private static readonly object m_lockObj = new object();
        public T Instance 
        {
            get
            {
                lock (m_lockObj)
                {
                    //m_instance ??= new T();
                }
                return m_instance;
            }
        }
    }
}