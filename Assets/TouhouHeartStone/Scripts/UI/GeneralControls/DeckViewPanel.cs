using System;
using UnityEngine;
namespace UI
{
    class DeckViewPanel
    {
        #region 公有方法
        public void update((string name, Sprite image, bool isWarning)[] viewData)
        {
            throw new NotImplementedException();
        }
        #endregion
        #region 事件
        /// <summary>
        /// 当点击卡组列表，参数为点击卡组的索引。
        /// </summary>
        public Action<int> onClickDeckListItem;
        #endregion
    }
}
