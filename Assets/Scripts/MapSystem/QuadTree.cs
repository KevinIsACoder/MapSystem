using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace MapSystem
{
    public class Bound
    {
        public int x;
        public int y;
        public int width;
        public int height;

        public Bound(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }
    }
    
    public class QuadTree<T> where T: class
    {
        private Bound m_bound;
        private int m_maxNum = 10;
        private int m_maxLevels = 4;
        private int m_level = 0;

        private List<QuadTree<T>> m_nodes;
        private List<T> m_objects;
        private List<Bound> m_objectBounds;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bound"></param>
        /// <param name="max_objects"></param>
        /// <param name="max_levels"></param>
        /// <param name="level"></param>
        public QuadTree(Bound bound, int max_objects, int max_levels, int level)
        {
            m_bound = bound;
            m_maxNum = max_objects;
            m_maxLevels = max_levels;
            m_level = level;
            m_nodes = new List<QuadTree<T>>(4);
            m_objects = new List<T>();
            m_objectBounds = new List<Bound>();
        }

        public void Split()
        {
            var nextLevel = m_level + 1;
            var subWidth = m_bound.width / 2;
            var subHeight = m_bound.height / 2;
            var x = m_bound.x;
            var y = m_bound.y;

            m_nodes[0] = new QuadTree<T>(new Bound(x, y, subWidth, subHeight), m_maxNum, m_maxLevels, nextLevel);
            m_nodes[1] = new QuadTree<T>(new Bound(x + subWidth, y, subWidth, subHeight), m_maxNum, m_maxLevels, nextLevel);
            m_nodes[2] = new QuadTree<T>(new Bound(x, y + subHeight, subWidth, subHeight), m_maxNum, m_maxLevels, nextLevel);
            m_nodes[3] = new QuadTree<T>(new Bound(x + subWidth, y + subHeight, subWidth, subHeight), m_maxNum, m_maxLevels, nextLevel);
        }
        
        /*
 * Determine which node the object belongs to
 * @param Object pRect		bounds of the area to be checked, with x, y, width, height
 * @return Integer		index of the subnode (0-3), or -1 if pRect cannot completely fit within a subnode and is part of the parent node
 */
        public int GetIndex(Bound bound)
        {
            var index = -1;
            var horizontalMidPoint = this.m_bound.x + m_bound.width * 0.5f;
            var verticalMidPoint = m_bound.y + m_bound.height * 0.5f;

            var isTopQuad = bound.y < verticalMidPoint && bound.y + bound.height < verticalMidPoint;
            var isBottomQuad = bound.y > verticalMidPoint;
            
            //at left quadrants 
            if (bound.x < horizontalMidPoint && bound.x < bound.x + horizontalMidPoint)
            {
                if (isTopQuad)
                {
                    index = 0; //top left
                }
                else if (isBottomQuad)
                {
                    index = 2; //bottom left
                }
            }
            else if(bound.x > horizontalMidPoint) //at right quadrants
            {
                if (isTopQuad)
                {
                    index = 1; //top right
                }
                else if (isBottomQuad)
                {
                    index = 3;  //bottom right
                }
            }

            return index;
        }
        
        public List<QuadTree<T>> GetRelevantNodes(Bound bound)
        {
            var midX = m_bound.x + m_bound.width * 0.5f;
            var midY = m_bound.y + m_bound.height * 0.5f;

            var qs = new List<QuadTree<T>>();
            var isTop = bound.y <= midY;
            var isBottom = bound.y + bound.height > midY;
            if (bound.x < midX)
            {
                //left
                if(isTop) qs.Add(m_nodes[0]);
                if(isBottom) qs.Add(m_nodes[2]);
            }
            else if (bound.x + bound.width > midX)
            {
                if(isTop) qs.Add(m_nodes[1]);
                if(isBottom) qs.Add(m_nodes[3]);
            }
            return qs;
        }
        
        public void Insert(Bound bounds, T item)
        {
            if (m_nodes[0] != null)
            {
                var nodes = GetRelevantNodes(bounds);
                for (int i = 0; i < nodes.Count; i++)
                {
                    nodes[i].Insert(bounds, item);
                }
                return;
            }
            
            m_objects.Add(item);
            m_objectBounds.Add(bounds);
            
            if (m_objects.Count > m_maxNum && m_level < m_maxLevels)
            {
                if (m_nodes[0] == null)
                {
                    Split();
                    for (int i = 0; i < m_objectBounds.Count; i++)
                    {
                        var mBound = m_objectBounds[i];
                        var mObject = m_objects[i];
                        var nodes = GetRelevantNodes(mBound);
                        for (int j = 0; j < nodes.Count; j++)
                        {
                            nodes[i].Insert(mBound, mObject);
                        }
                    }
                }
            }
        }

        public List<T> Retrieve(Bound bounds)
        {
            var items = new List<T>();
            foreach (var node in GetRelevantNodes(bounds))
            {
                items.AddRange(node.Retrieve(bounds));
            }

            return items;
        }
    }
}