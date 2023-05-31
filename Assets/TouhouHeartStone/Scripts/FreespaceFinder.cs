using System;
using System.Collections.Generic;
using ExcelLibrary.BinaryFileFormat;
using UnityEngine;

namespace FreespaceFinder
{
    /// <summary>
    /// This class holds a List of Freespaces that can be cut
    /// At the beginning you have only one Freespace covering the whole plane, 
    /// adding cuts the List gets populated with all the residual areas.
    /// </summary>
    public class Region
    {
        private List<Rect> freeRects;
        /// <summary>
        /// Public constructor
        /// </summary>
        /// <param name="width">Width of the area</param>
        /// <param name="height">Height of the area</param>
        public Region(float width, float height)
        {
            freeRects = new List<Rect>
            {
                new Rect(0, 0, width, height)
            };
        }
        /// <summary>
        /// Returns the first Rect (or null) that can contain the whole cut
        /// </summary>
        /// <param name="width">Width of the area</param>
        /// <param name="height">Height of the area</param>
        /// <param name="rect">First Rect completely containing the cut</param>
        public bool canCut(float width, float height, out Rect rect)
        {
            foreach (Rect r in freeRects)
            {
                if (ContainsSize(r, height, width))
                {
                    rect = r;
                    return true;
                }
            }
            rect = default;
            return false;
        }
        /// <summary>
        /// Returns the nearest Rect (or null) that can contain the whole cut
        /// </summary>
        /// <param name="width">Width of the area</param>
        /// <param name="height">Height of the area</param>
        /// <param name="rect">Nearest Rect completely containing the cut</param>
        public bool canCutByNearest(float width, float height, Vector2 target, out Rect rect)
        {
            bool found = false;
            float nearestDis = 0;
            rect = default;
            foreach (Rect r in freeRects)
            {
                if (ContainsSize(r, height, width))
                {
                    float dis = Vector2.SqrMagnitude(r.center - target);
                    if (!found || dis < nearestDis)
                    {
                        nearestDis = dis;
                        rect = r;
                        found = true;
                    }
                }
            }
            return found;
        }
        /// <summary>
        /// Makes the cut in the region.
        /// Intersects the desired cut with all the Freespaces in the List.
        /// WARNING. No check is made, at this point, if you can do the cut. 
        ///          You have to call the canCut function before using this method 
        ///          and take the adequate coordinates from the Freespace returned.
        /// </summary>
        /// <param name="x">Leftmost position of the cut</param>
        /// <param name="y">Topmost position of the cut</param>
        /// <param name="width">Width of the area</param>
        /// <param name="height">Height of the area</param>
        public void Cut(Rect cutRect)
        {
            List<Rect> freeSpaces = new List<Rect>();
            foreach (Rect rect in freeRects)
            {
                freeSpaces.AddRange(CutRect(rect, cutRect));
            }
            freeRects = freeSpaces;
        }
        public bool ContainsFreeRect(Rect rect)
        {
            foreach (Rect r in freeRects)
            {
                if (r.xMax >= rect.xMax &&
                    r.xMin <= rect.xMin &&
                    r.yMax >= rect.yMax &&
                    r.yMin <= rect.yMin)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Tests if this area can contain a cut of the given height and width
        /// </summary>
        /// <param name="width">Width of the area</param>
        /// <param name="height">Height of the area</param>
        /// <returns>true if this Freespce can contain the needed cut else false</returns>
        public static bool ContainsSize(Rect rect, float width, float height)
        {
            if (rect.width >= width && rect.height >= height)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// Creates cuts from this Freespace
        /// If the proposed cut does not intersect with this Freespace, do nothing and return a List containing this.
        /// If the proposed cut completely contains this Freespace, return an empty List.
        /// When the cut intersects the Freespace, cut the Freespace with the semiplanes around the desired cut
        /// </summary>
        /// <param name="x">Leftmost position of the cut</param>
        /// <param name="y">Topmost position of the cut</param>
        /// <param name="width">Width of the area</param>
        /// <param name="height">Height of the area</param>
        /// <returns>List of Freespaces created from the intersection of the Freespace and the cut</returns>
        public static List<Rect> CutRect(Rect rect, Rect cutRect)
        {
            List<Rect> res = new List<Rect>();
            if (cutRect.x >= rect.x + rect.width ||
                cutRect.y >= rect.y + rect.height ||
                (cutRect.x + cutRect.width) < rect.x ||
                (cutRect.y + cutRect.height) < rect.y)
            {
                // no intersection, return myself
                res.Add(rect);
                return res;
            }
            if (cutRect.x <= rect.x &&
                cutRect.y <= rect.y &&
                (cutRect.x + cutRect.width) >= (rect.x + rect.width) &&
                (cutRect.y + cutRect.height) >= (rect.y + rect.height))
            {
                // the cut covers the whole freespce, return empty list
                return res;
            }
            if (cutRect.x > rect.x)
            {
                // cut this freespace with the semiplane of abscissas less than x
                res.Add(new Rect(rect.x, rect.y, cutRect.x - rect.x, rect.height));
            }
            if (cutRect.y > rect.y)
            {
                // cut this freespace with the semiplane of ordinates less than y
                res.Add(new Rect(rect.x, rect.y, rect.width, cutRect.y - rect.y));
            }
            if ((cutRect.x + cutRect.width) < (rect.x + rect.width))
            {
                // cut this freespace with the semiplane of abscissas greater than x+width
                res.Add(new Rect(cutRect.x + cutRect.width, rect.y, (rect.x + rect.width) - (cutRect.x + cutRect.width), rect.height));
            }
            if ((cutRect.y + cutRect.height) < (rect.y + rect.height))
            {
                // cut this freespace with the semiplane of ordinates greater than y+height
                res.Add(new Rect(rect.x, cutRect.y + cutRect.height, rect.width, (rect.y + rect.height) - (cutRect.y + cutRect.height)));
            }
            return res;
        }
    }
}
