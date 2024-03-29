using System.Collections.Generic;

namespace MapSystem
{
    public class PriorityQueue<T> where T : class
    {
        private Queue<T> m_elements;

        public PriorityQueue()
        {
            m_elements = new Queue<T>();
        }

        public void Equeue(T element)
        {
            m_elements.Enqueue(element);
        }

        public T Dequeue()
        {
            return m_elements.Dequeue();
        }

        public bool Empty()
        {
            return m_elements.Count == 0;
        }
    }
}