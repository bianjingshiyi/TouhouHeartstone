using System.Collections;

namespace Game
{
    public static class ListValueHelper
    {
        #region 获取
        public static object getListElementByIndexes(this object value, int[] itemIndexes)
        {
            if (itemIndexes != null && itemIndexes.Length > 0)
            {
                var result = value;
                foreach (var itemIndex in itemIndexes)
                {
                    if (result is IList list && itemIndex >= 0 && itemIndex < list.Count)
                        result = list[itemIndex];
                    else
                        return null;
                }
                return result;
            }
            return value;
        }
        public static bool tryGetListElementByIndexes<T>(this object value, int[] itemIndexes, out T result)
        {
            if (getListElementByIndexes(value, itemIndexes) is T tValue)
            {
                result = tValue;
                return true;
            }
            result = default;
            return false;
        }
        public static T getListElementByIndexes<T>(this object value, int[] itemIndexes)
        {
            if (tryGetListElementByIndexes(value, itemIndexes, out T result))
                return result;
            return default;
        }
        #endregion

        #region 设置
        public static bool replaceListElementByIndexes(this object listValue, int[] itemIndexes, object value, out object newValue)
        {
            if (itemIndexes != null && itemIndexes.Length > 0)
            {
                var listObj = listValue;
                var count = itemIndexes.Length;
                for (int i = 0; i < count; i++)
                {
                    var itemIndex = itemIndexes[i];
                    if (listObj is not IList list || itemIndex < 0 || itemIndex >= list.Count) // 不存在该列表，或者索引无效
                    {
                        newValue = listValue;
                        return false;
                    }
                    if (i < count - 1) // 非最后一项，获取子列表。
                        listObj = list[itemIndex];
                    else // 最后一项，直接设置值。
                        list[itemIndex] = value;
                }
                newValue = listValue;
                return true;
            }
            else
            {
                newValue = value;
                return true;
            }
        }
        #endregion
    }
}
