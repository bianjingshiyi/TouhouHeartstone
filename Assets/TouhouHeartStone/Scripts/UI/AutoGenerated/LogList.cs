using System;
using BJSYGameCore.UI;
using UnityEngine;

namespace UI
{
    public class LogList : MonoBehaviour
    {
        public void updateLogListCount(int count)
        {
            _logLayoutGroup.setCount(count);
        }
        public int getLogItemIndex(RectTransform rect)
        {
            return _logLayoutGroup.getItemIndex(rect);
        }
        private void Awake()
        {
            _logLayoutGroup.cellPrefab.gameObject.SetActive(false);
        }
        public event Action<int, RectTransform> onEnableLogListItem
        {
            add { _logLayoutGroup.onEnableItem += value; }
            remove { _logLayoutGroup.onEnableItem -= value; }
        }
        public event Action<int, RectTransform> onDisableLogListItem
        {
            add { _logLayoutGroup.onDisableItem += value; }
            remove { _logLayoutGroup.onDisableItem += value; }
        }
        public event Action<int, RectTransform> onUpdateLogListItem
        {
            add { _logLayoutGroup.onUpdateItem += value; }
            remove { _logLayoutGroup.onUpdateItem -= value; }
        }
        [SerializeField]
        private VirtualListLayoutGroup _logLayoutGroup;
    }
}
