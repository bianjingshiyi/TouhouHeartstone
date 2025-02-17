using System;
using System.Collections;
using System.Linq;
using TouhouCardEngine;

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
                    if (result is Pair pair)
                    {
                        if (itemIndex == 0)
                            return pair.key;
                        else if (itemIndex == 1)
                            return pair.value;
                        else
                            return null;
                    }
                    else if (result is IList list && itemIndex >= 0 && itemIndex < list.Count)
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
                int itemIndex = itemIndexes[0];
                if (listValue is IList list)
                {
                    if (itemIndexes.Length > 1)
                    {
                        if (0 <= itemIndex && itemIndex < list.Count)
                        {
                            bool result = list[itemIndex].replaceListElementByIndexes(itemIndexes.Skip(1).ToArray(), value, out object childValue);
                            list[itemIndex] = childValue;
                            newValue = list;
                            return result;
                        }
                        else
                        {
                            // 索引无效
                            newValue = listValue;
                            return false;
                        }
                    }
                    else
                    {
                        list[itemIndex] = value;
                        newValue = list;
                        return true;
                    }
                }
                else if (listValue is Pair pair)
                {
                    if (itemIndexes.Length > 1)
                    {
                        bool result;
                        if (itemIndex == 0)
                        {
                            result = pair.key.replaceListElementByIndexes(itemIndexes.Skip(1).ToArray(), value, out object childValue);
                            pair.key = childValue;
                        }
                        else if (itemIndex == 1)
                        {
                            result = pair.value.replaceListElementByIndexes(itemIndexes.Skip(1).ToArray(), value, out object childValue);
                            pair.value = childValue;
                        }
                        else
                        {
                            // 索引无效
                            result = false;
                        }
                        newValue = pair;
                        return result;
                    }
                    else
                    {
                        if (itemIndex == 0)
                            pair.key = value;
                        else if (itemIndex == 1)
                            pair.value = value;
                        else
                        {
                            // 索引无效
                            newValue = pair;
                            return false;
                        }
                        newValue = pair;
                        return true;
                    }
                }
                else
                {
                    // 不存在子结构
                    newValue = listValue;
                    return false;
                }
            }
            else
            {
                newValue = value;
                return true;
            }
        }
        #endregion

        public static T cloneValue<T>(this T value)
        {
            if (value is ICloneable clonable)
            {
                if (clonable.Clone() is T tValue)
                    return tValue;
            }
            return value;
        }
    }
}
