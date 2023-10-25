using System;
using System.Collections;
using System.Collections.Generic;
using BJSYGameCore.UI;
using UnityEngine;

namespace UI
{
    public class CharacterSelectRow : MonoBehaviour
    {
        public void update(CharacterViewData[] viewDatas)
        {
            UIHelper.updateList(_itemListRoot, _itemTemplate, _itemList, viewDatas.Length,
                (i, rect) =>
                {
                    var item = rect.GetComponent<CharacterSelectItem>();
                    item.update(viewDatas[i]);
                },
                rect =>
                {
                    var item = rect.GetComponent<CharacterSelectItem>();
                    item.onClick += onItemClickedCallback;
                },
                rect =>
                {
                    var item = rect.GetComponent<CharacterSelectItem>();
                    item.onClick -= onItemClickedCallback;
                });
        }
        private void onItemClickedCallback(CharacterSelectItem item)
        {
            onItemClicked?.Invoke(_itemList.IndexOf(item.transform as RectTransform));
        }
        public event Action<int> onItemClicked;
        [SerializeField]
        private RectTransform _itemListRoot;
        [SerializeField]
        private RectTransform _itemTemplate;
        [SerializeField]
        private List<RectTransform> _itemList = new List<RectTransform>();
    }
}