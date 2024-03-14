using System.Collections.Generic;
using System.Linq;

namespace Game.BattleTable
{
    public static class PileInsertHelper
    {
        /// <summary>
        /// 向牌堆列表插入一个项，会自动调整占位符的位置。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pileItems"></param>
        /// <param name="emptyPositions"></param>
        /// <param name="position"></param>
        /// <param name="item"></param>
        public static void insertItem<T>(List<T> pileItems, List<int> emptyPositions, int position, T item)
        {
            int insertPos = getInsertPosition(pileItems, emptyPositions, position);
            pileItems.Insert(insertPos, item);
            addToEmpty(emptyPositions, position);
        }
        /// <summary>
        /// 向牌堆列表插入一个项，会自动调整占位符的位置。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pileItems"></param>
        /// <param name="emptyPositions"></param>
        /// <param name="pileItem"></param>
        /// <returns></returns>
        public static bool removeItem<T>(List<T> pileItems, List<int> emptyPositions, T pileItem)
        {
            int handPos = getItemRealPosition(pileItems, emptyPositions, pileItem);
            if (pileItems.Remove(pileItem))
            {
                removeFromEmpty(emptyPositions, handPos);
                return true;
            }
            return false;
        }
        private static void addToEmpty(List<int> emptyPositions, int position)
        {
            emptyPositions.Remove(position);
            for (int i = 0; i < emptyPositions.Count; i++)
            {
                var pos = emptyPositions[i];
                if (pos > position)
                {
                    emptyPositions.RemoveAt(i);
                    break;
                }
            }
        }
        private static void removeFromEmpty(List<int> emptyPositions, int position)
        {
            emptyPositions.Remove(position);
            for (int i = 0; i < emptyPositions.Count; i++)
            {
                var pos = emptyPositions[i];
                if (position < pos)
                {
                    emptyPositions[i] = pos - 1;
                }
            }
        }
        private static int getItemRealPosition<T>(List<T> pileItems, List<int> emptyPositions, T hand)
        {
            int index = pileItems.IndexOf(hand);
            index += emptyPositions.Where(pos => pos < index).Count();
            return index;
        }
        private static int getInsertPosition<T>(List<T> pileItems, List<int> emptyPositions, int position)
        {
            var insertPos = position;
            var emptySlots = emptyPositions.Where(pos => pos < position).Count();
            insertPos -= emptySlots;
            if (insertPos > pileItems.Count)
            {
                insertPos = pileItems.Count;
                for (int i = pileItems.Count + emptySlots; i < position; i++)
                {
                    emptyPositions.Add(i);
                }
            }
            return insertPos;
        }
    }
}
