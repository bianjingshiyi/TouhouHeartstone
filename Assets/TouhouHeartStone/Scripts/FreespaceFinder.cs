using System.Collections.Generic;
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
                if (ContainsSize(r, width, height))
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
        /// <param name="target">Original target position</param>
        /// <param name="rect">Nearest Rect completely containing the cut</param>
        public bool canCutByNearest(float width, float height, Vector2 target, out Rect rect)
        {
            return canCutByNearest(width, height, target, target, out rect);
        }
        /// <summary>
        /// Returns the nearest Rect (or null) that can contain the whole cut
        /// </summary>
        /// <param name="width">Width of the area</param>
        /// <param name="height">Height of the area</param>
        /// <param name="target">Original target position</param>
        /// <param name="distancePivot">The pivot point for distance check</param>
        /// <param name="rect">Nearest Rect completely containing the cut</param>
        public bool canCutByNearest(float width, float height, Vector2 target, Vector2 distancePivot, out Rect rect)
        {
            bool found = false;
            float nearestDis = 0;
            rect = default;
            foreach (Rect r in freeRects)
            {
                if (ContainsSize(r, width, height))
                {
                    float nearestX = Mathf.Clamp(target.x, r.xMin, r.xMax);
                    float nearestY = Mathf.Clamp(target.y, r.yMin, r.yMax);
                    Vector2 nearest = new Vector2(nearestX, nearestY);
                    float dis = Vector2.SqrMagnitude(nearest - distancePivot);
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
            Cut(cutRect.x, cutRect.y, cutRect.width, cutRect.height);
        }
        public void Cut(float cutRectX, float cutRectY, float cutRectWidth, float cutRectHeight)
        {
            int lastCount = freeRects.Count;
            for (int i = 0; i < lastCount; i++)
            {
                var rect = freeRects[i];
                CutRectNonAlloc(freeRects, rect, cutRectX, cutRectY, cutRectWidth, cutRectHeight);
            }
            freeRects.RemoveRange(0, lastCount);
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
            CutRectNonAlloc(res, rect, cutRect);
            return res;
        }
        public static void CutRectNonAlloc(List<Rect> list, Rect rect, Rect cutRect)
        {
            CutRectNonAlloc(list, rect, cutRect.x, cutRect.y, cutRect.width, cutRect.height);
        }
        public static void CutRectNonAlloc(List<Rect> list, Rect rect, float cutRectX, float cutRectY, float cutRectWidth, float cutRectHeight)
        {
            float cutRectMaxX = cutRectX + cutRectWidth;
            float cutRectMaxY = cutRectY + cutRectHeight;

            float rectX = rect.x;
            float rectY = rect.y;
            float rectWidth = rect.width;
            float rectHeight = rect.height;
            float rectMaxX = rectX + rectWidth;
            float rectMaxY = rectY + rectHeight;

            if (cutRectX >= rectMaxX ||
                cutRectY >= rectMaxY ||
                cutRectMaxX < rectX ||
                cutRectMaxY < rectY)
            {
                // no intersection, return myself
                list.Add(rect);
                return;
            }
            if (cutRectX <= rectX &&
                cutRectY <= rectY &&
                cutRectMaxX >= rectMaxX &&
                cutRectMaxY >= rectMaxY)
            {
                // the cut covers the whole freespce, return empty list
                return;
            }
            if (cutRectX > rectX)
            {
                // cut this freespace with the semiplane of abscissas less than x
                list.Add(new Rect(rectX, rectY, cutRectX - rectX, rectHeight));
            }
            if (cutRectY > rectY)
            {
                // cut this freespace with the semiplane of ordinates less than y
                list.Add(new Rect(rectX, rectY, rectWidth, cutRectY - rectY));
            }
            if (cutRectMaxX < rectMaxX)
            {
                // cut this freespace with the semiplane of abscissas greater than x+width
                list.Add(new Rect(cutRectX + cutRectWidth, rectY, rectMaxX - cutRectMaxX, rectHeight));
            }
            if (cutRectMaxY < rectMaxY)
            {
                // cut this freespace with the semiplane of ordinates greater than y+height
                list.Add(new Rect(rectX, cutRectY + cutRectHeight, rectWidth, rectMaxY - cutRectMaxY));
            }
        }
    }
}
