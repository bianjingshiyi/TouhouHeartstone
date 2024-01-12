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
            if (index < 0 || index >= _itemList.Count)
                return null;
            return _itemList[index];
        }
        public T getElement<T>(int index) where T : Component
        {
            return getElement(index)?.GetComponent<T>();
        }
        public IEnumerable<RectTransform> getElements()
        {
            return _itemList;
        }
        public IEnumerable<T> getElements<T>() where T : Component
        {
            foreach (var item in _itemList)
            {
                yield return item.GetComponent<T>();
            }
        }
        public int count => _itemList.Count;
        [SerializeField]
        private RectTransform _listRoot;
        [SerializeField]
        private RectTransform _template;
        [SerializeField]
        private List<RectTransform> _itemList = new List<RectTransform>();
    }
}
