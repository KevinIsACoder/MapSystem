using System;
using System.Collections.Generic;

namespace MapSystem.Runtime
{
    public class PriorityQueue<TItem, TPriority>
    {
        private int m_count = 0;
        private TItem[] m_nodes;
        private Comparison<TPriority> m_compare;
        
        public PriorityQueue(int maxNodes, Comparison<TPriority> compare)
        {
            m_count = 0;
            m_nodes = new TItem[maxNodes + 1];
            m_compare = compare;
        }

        public int Count
        {
            get => m_count;
        }

        public void Clear()
        {
            Array.Clear(m_nodes, 1, m_count);
        }

        public void Equeue(TItem item, TPriority priority)
        {
            if(item == null)
                return;
            m_count++;
            if (m_count > m_nodes.Length - 1) //扩容
            {
                Array.Resize(ref m_nodes, m_nodes.Length * 2);
            }
            
            
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