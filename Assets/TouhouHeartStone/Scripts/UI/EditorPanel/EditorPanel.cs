using System.Collections.Generic;
using UnityEngine;
using Game;
using BJSYGameCore;
using System.IO;
using UnityEngine.UI;
using System;
namespace UI
{
    public partial class EditorPanel
    {
        /// <summary>
        /// 显示卡池列表
        /// </summary>
        public void displayCardPoolList()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 显示卡牌列表
        /// </summary>
        public void displayCardList()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 更新卡池列表
        /// </summary>
        /// <param name="cardPoolInfos"></param>
        public void updateCardPoolList(CardPoolInfo[] cardPoolInfos)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 设置卡牌列表数量（会引发卡牌列表刷新事件）
        /// </summary>
        /// <param name="count"></param>
        public void updateCardListCount(int count)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 设置排序类型选项
        /// </summary>
        /// <param name="sortTypes"></param>
        public void setSortTypes(string[] sortTypes)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 设置排序类型的值
        /// </summary>
        /// <param name="index"></param>
        public void setSortType(int index)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 设置角色过滤器的选项
        /// </summary>
        /// <param name="filterNames"></param>
        public void setCharacterFilters(string[] filterNames)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 设置角色过滤器的值
        /// </summary>
        /// <param name="index"></param>
        public void setCharaterFilter(int index)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 当点击卡池列表项，参数为列表项索引
        /// </summary>
        public event Action<int> onClickCardPoolListItem;
        /// <summary>
        /// 当更新卡牌列表，参数为更新的列表项索引和UI物体
        /// </summary>
        public event Action<int, RectTransform> onUpdateCardListItem;
        /// <summary>
        /// 当点击卡牌列表，参数为点击的列表项索引
        /// </summary>
        public event Action<int> onClickCardListItem;
        /// <summary>
        /// 当更改排序类型，参数为更改的索引
        /// </summary>
        public event Action<int> onChangeSortTypeValue;
        /// <summary>
        /// 当改变角色过滤器的值，参数为更改的索引
        /// </summary>
        public event Action<int> onChangeCharacterFilterValue;
    }
    [Serializable]
    public class CardPoolInfo
    {
        public string cardPoolName;
        public string imageResPath;
    }
}