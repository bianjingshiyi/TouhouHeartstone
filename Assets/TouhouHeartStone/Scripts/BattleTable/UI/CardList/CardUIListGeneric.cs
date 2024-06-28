using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.BattleTable
{
    public abstract class CardUIList<T> : MonoBehaviour where T : CardUIItem
    {
        public T getItem(int index)
        {
            return _choices[index] as T;
        }
        public int indexOf(T item)
        {
            return _choices.IndexOf(item);
        }
        public T add()
        {
            var choice = Instantiate(_itemTemplate, _itemRoot).GetComponent<T>();
            choice.gameObject.SetActive(true);
            _choices.Add(choice);
            addItemEvents(choice);
            return choice;
        }
        public void clear()
        {
            foreach (var choice in _choices)
            {
                removeItemEvents(choice);
                choice.destroy();
            }
            _choices.Clear();
        }
        #region 私有方法
        #region 生命周期
        private void Awake()
        {
            _itemTemplate.SetActive(false);
        }
        #endregion

        protected virtual void addItemEvents(T item)
        {
            item.onPointerEnter += onItemPointerEnterCallback;
            item.onPointerExit += onItemPointerExitCallback;
            item.onClick += onItemClickCallback;
        }
        protected virtual void removeItemEvents(T item)
        {
            item.onPointerEnter -= onItemPointerEnterCallback;
            item.onPointerExit -= onItemPointerExitCallback;
            item.onClick -= onItemClickCallback;
        }
        private void onItemPointerEnterCallback(CardUIItem card, PointerEventData pointer)
        {
            onItemPointerEnter?.Invoke(indexOf(card as T));
        }
        private void onItemPointerExitCallback(CardUIItem card, PointerEventData pointer)
        {
            onItemPointerExit?.Invoke(indexOf(card as T));
        }
        private void onItemClickCallback(CardUIItem card, PointerEventData pointer)
        {
            onItemClicked?.Invoke(indexOf(card as T));
        }
        #endregion

        #region 事件
        public Action<int> onItemPointerEnter;
        public Action<int> onItemPointerExit;
        public Action<int> onItemClicked;
        #endregion

        #region 属性字段
        [SerializeField]
        private GameObject _itemTemplate;
        [SerializeField]
        private Transform _itemRoot;
        [SerializeField]
        private List<T> _choices = new List<T>();
        #endregion
    }
}
