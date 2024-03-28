using System.Collections.Generic;
using UnityEngine;

namespace MapSystem
{

    public class Bounds
    {
        public float x;
        public float y;
        public float width;
        public float height;
    }
    
    public class QuadTreeRoot<T> where T : class
    {
        public QuadTree<T> topLeft;
        public QuadTree<T> topRight;
        public QuadTree<T> bottomLeft;
        public QuadTree<T> bottomRight;
    }

    public class QuadTreeLeaf<T> where T : class
    {
        public T[] objects;
        public Rect[] bounds;
    }
    
    public class QuadTree<T> where T: class
    {
        private Rect m_bound;
        private int m_maxNum;
        private int m_maxLevels = 10;
        private int m_level = 4;

        private QuadTreeRoot<T> m_root;
        private QuadTreeLeaf<T> m_leaf;
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
            m_level = level;
        }

        public void Split()
        {
            var level = m_level + 1;
            var width = this.m_bound.width / 2;
            var height = this.m_bound.height / 2;
            var x = this.m_bound.x;
            var y = this.m_bound.y;

            if (m_leaf != null)
            {
                m_root = new QuadTreeRoot<T>()
                {
                    topLeft = new QuadTree<T>(new Rect(x, y, width, height), m_maxNum, m_maxLevels, level),
                    topRight = new QuadTree<T>(new Rect(x + width, y, width, height), m_maxNum, m_maxLevels, level),
                    bottomLeft = new QuadTree<T>(new Rect(x, y + height, width, height), m_maxNum, m_maxLevels, level),
                    bottomRight = new QuadTree<T>(new Rect(x + width, y + height, width, height), m_maxNum, m_maxLevels, level)
                };
            }
            
            // add all objects to their corresponding subnodes
            for (var i = 0; i < m_leaf.bounds.Length; i++) 
            {
                var rect = m_leaf.bounds[i];
                var obj = m_leaf.objects[i];
                if (rect != null && obj != null)
                {
                    
                }
            }
        }

        public QuadTree<T>[] GetRelevantNodes()
        {
            return new QuadTree<T>[1];
        }

        public void Insert(Rect bound, T obj)
        {
            
        }
        
        public T[] Retrieve(Rect bounds)
        {
            return new T[1];
        }
        
    }
}