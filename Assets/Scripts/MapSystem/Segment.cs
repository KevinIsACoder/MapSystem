﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace MapSystem
{
    public class MetaInfo
    {
        public bool highway;
        public bool served;
    }

    public class Segment
    {
        public bool isHignWay;
        public Vector2 startPoint; //路段开始点
        public Vector2 endPoint; //路段结束点

        public List<Segment> forwardSegment; //下个路段
        public List<Segment> backwardSegment; //上个路段

        public Action setupBranchLinks;
        public float m_time;

        private float m_width;

        public Segment(Vector2 start_Point, Vector2 end_Point, float time, bool isHignWay = false)
        {
            m_width = isHignWay ? MapConsts.roadWidth : MapConsts.roadWidth;
            startPoint = start_Point;
            endPoint = end_Point;
        }

        public float Length => Vector2.Distance(startPoint, endPoint);

        public Rect Limits => new Rect(startPoint.x, startPoint.y, Math.Abs(startPoint.x - endPoint.x),
            Math.Abs(startPoint.y - endPoint.y));

        public bool StartIsBackwards()
        {
            if (backwardSegment.Count > 0)
            {
                if (backwardSegment == null) Debug.LogError(("impossib"));
                var ba =
                    MathUtil.EqualV(this.backwardSegment[0].startPoint, this.startPoint) ||
                    MathUtil.EqualV(this.backwardSegment[0].endPoint, this.startPoint);
                if (!ba) Debug.LogError("warning: backward");
                return ba;
            }
            else
            {
                // just in case we have no backwards links (we are start segment)
                return (
                    MathUtil.EqualV(this.forwardSegment[0].startPoint, this.endPoint) ||
                    MathUtil.EqualV(forwardSegment[0].endPoint, endPoint)
                );
            }
        }

        public List<Segment> LinksForEndContaining(Segment segment)
        {
            if (backwardSegment.IndexOf(segment) != -1)
            {
                return backwardSegment;
            }
            else if (forwardSegment.IndexOf(segment) != -1)
            {
                return forwardSegment;
            }
            else
            {
                return null;
            }
        }

        /**
   * split this segment into two segments, connecting the given segment to the newly created crossing
   *
   * left example in https://phiresky.github.io/procedural-cities/img/20151213214559.png
   *
   * @param point the coordinates the split will be at
   * @param thirdSegment the third segment that will be joined to the newly created crossing
   * @param segmentList the full list of all segments (new segment will be added here)
   * @param qTree quadtree for faster finding of segments (new segment will be added here)
   */
        public void Split(Vector2 point, Segment thirdSegment, List<Segment> segmentList, QuadTree<Segment> quadTree)
        {
            var splitSegment = Clone();
            var startIsBackwards = StartIsBackwards();
            segmentList.Add(splitSegment);
            quadTree.Insert(splitSegment.Limits, splitSegment);
            splitSegment.endPoint = point;
            startPoint = point;
            // links are not copied in the constructor, so
            // copy link array for the split part, keeping references the same
            splitSegment.backwardSegment = backwardSegment;
            splitSegment.forwardSegment = forwardSegment;

            Segment firstSplit;
            Segment[] fixLinks;
            Segment secondSplit;
            
            // determine which links correspond to which end of the split segment
            if (startIsBackwards) {
                firstSplit = splitSegment;
                secondSplit = this;
                fixLinks = splitSegment.backwardSegment.ToArray();
            } else {
                firstSplit = this;
                secondSplit = splitSegment;
                fixLinks = splitSegment.forwardSegment.ToArray();
            }
            
            // one of the ends of our segment is now instead part of the newly created segment
            // go through all linked roads at that end, and replace their inverse references from referring to this to referring to the newly created segment
            for (int i = 0; i < fixLinks.Length; i++)
            {
                var index = fixLinks[i].backwardSegment.IndexOf(this);
                if (index > 0)
                {
                    fixLinks[i].backwardSegment[index] = splitSegment;
                }
                else
                {
                    index = fixLinks[i].forwardSegment.IndexOf(this);
                    if (index > 0)
                    {
                        fixLinks[index] = splitSegment;
                    }
                }
            }
            
            // new crossing is between firstSplit, secondSplit, and thirdSegment
            firstSplit.forwardSegment.Clear();
            firstSplit.forwardSegment.Add(thirdSegment);
            firstSplit.forwardSegment.Add(secondSplit);
            
            secondSplit.backwardSegment.Clear();
            secondSplit.backwardSegment.Add(thirdSegment);
            secondSplit.backwardSegment.Add(firstSplit);
            thirdSegment.forwardSegment.Add(firstSplit);
            thirdSegment.forwardSegment.Add(secondSplit);
        }

        public static Segment GenerateSegment(Vector2 startPoint, Vector2 direction, float length, float time, MetaInfo metaInfo = null)
        {
            var endPoint = startPoint + direction * length;
            return new Segment(startPoint, endPoint, time);
        }

        public void InterSectWith(Segment segment)
        {
            
        }

        public Segment Clone()
        {
            return new Segment(startPoint, endPoint, m_time, isHignWay);
        }
    }
}