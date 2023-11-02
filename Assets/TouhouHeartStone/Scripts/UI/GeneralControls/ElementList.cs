using System;
using System.Collections.Generic;
using BJSYGameCore.UI;
using UnityEngine;

namespace UI
{
    public class ElementList : MonoBehaviour
    {
        public void updateList(
            int count,
            Action<int, RectTransform> onUpdate,
            Action<RectTransform> onCreateOrEnable = null,
            Action<RectTransform> onDestroyOrDisable = null,
            bool dontDestroy = false,
            bool rebuild = false)
        {
            UIHelper.updateList(_listRoot, _template, _itemList, count, onUpdate, onCreateOrEnable, onDestroyOrDisable, dontDestroy, rebuild);
        }
        public int indexOf(RectTransform trans)
        {
            return _itemList.IndexOf(trans);
        }
        public int indexOf(Component comp)
        {
            return indexOf(comp.transform as RectTransform);
        }
        public RectTransform getElement(int index)
        {
            return _itemList[index];
        }
        [SerializeField]
        private RectTransform _listRoot;
        [SerializeField]
        private RectTransform _template;
        [SerializeField]
        private List<RectTransform> _itemList = new List<RectTransform>();
    }
}
