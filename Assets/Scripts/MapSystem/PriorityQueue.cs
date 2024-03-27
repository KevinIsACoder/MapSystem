using System.Collections.Generic;

namespace MapSystem
{
    public class PriorityQueue<T>
    {
        private List<T> m_elements;

        public PriorityQueue(T[] mElements)
        {
            m_elements = new List<T>();
            m_elements.AddRange(mElements);
        }

        public void Enqueue(T element)
        {
            m_elements.Add(element);
        }

        public bool Empty()
        {
            return m_elements.Count == 0;
        }
    }
}