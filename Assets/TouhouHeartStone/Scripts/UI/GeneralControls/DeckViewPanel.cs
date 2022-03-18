using BJSYGameCore.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI
{
    public class DeckViewPanel : MonoBehaviour
    {
        #region 公有方法

        public void update((string name, Sprite image, bool isWarning)[] deckViewData)
        {
            UIHelper.updateList(_deckListRoot,_deckListItemTemplate,_deckItemList,deckViewData.Length,(i, item) =>
            {
                DeckListItem deckListItem = item.GetComponentInChildren<DeckListItem>();
                deckListItem.updateView(deckViewData[i].name, deckViewData[i].image);
                deckListItem.setWarning(deckViewData[i].isWarning);
                deckListItem.SetMode(DeckListItem.Mode.DeckSelect);
                deckListItem.SetSelectable(!deckViewData[i].isWarning);
            }, item =>
            {
                DeckListItem deckListItem = item.GetComponent<DeckListItem>();
                deckListItem.onClick += onClickDeckListItemCallback;
            }, item =>
            {
                DeckListItem deckListItem = item.GetComponent<DeckListItem>();
                deckListItem.onClick -= onClickDeckListItemCallback;
            });
        }

        #endregion

        #region 私有方法

        #region 生命周期

        private void Awake()
        {
            if (_deckListItemTemplate.transform.parent == _deckListRoot)
            {
                _deckListItemTemplate.gameObject.SetActive(false);
            }
        }

        #endregion

        #region 事件回调

        private void onClickDeckListItemCallback(DeckListItem item)
        {
            onClickDeckListItem?.Invoke(_deckItemList.IndexOf(item.transform as RectTransform));
        }
        #endregion
        
        #endregion

        #region 事件

        /// <summary>
        /// 当点击右侧卡组列表项，参数为索引
        /// </summary>
        public event Action<int> onClickDeckListItem;
        #endregion

        #region 属性字段

        [SerializeField] 
        private RectTransform _deckListRoot;
        [SerializeField]
        private RectTransform _deckListItemTemplate;
        [SerializeField]
        List<RectTransform> _deckItemList = new List<RectTransform>();

        #endregion
    }
}