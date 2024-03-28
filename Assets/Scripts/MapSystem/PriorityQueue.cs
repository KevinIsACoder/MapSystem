using System.Collections.Generic;

namespace MapSystem
{
    public class PriorityQueue<T>
    {
        private List<T> m_elements;

        public PriorityQueue()
        {
            m_elements = new List<T>();
        }

        public void Enqueue(T element)
        {
            m_elements.Add(element);
        }

        public T Dequeue()
        {
            return m_elements[0];
        }

        public bool Empty()
        {
            return m_elements.Count == 0;
        }
    }
}