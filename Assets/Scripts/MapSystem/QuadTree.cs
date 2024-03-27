using UnityEngine;

namespace MapSystem
{
    public class QuadTree<T> where T: class
    {
        private Rect m_bound;
        private int m_maxNum;
        private int m_maxLevels;
        private int m_maxLevel;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bound"></param>
        /// <param name="max_objects"></param>
        /// <param name="max_levels"></param>
        /// <param name="level"></param>
        public QuadTree(Rect bound, int max_objects, int max_levels, int level)
        {
            m_bound = bound;
            m_maxNum = max_objects;
            m_maxLevels = max_levels;
            m_maxLevel = level;
        }

        public void Split()
        {
            
        }

        public T[] Retrieve(Rect bounds)
        {
            
        }
        
    }
}