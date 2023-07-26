using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Game.BattleTable
{
    public class Field : MonoBehaviour, IEnumerable<Servant>
    {
        public void updateField(float deltaTime)
        {
            alignServants(deltaTime);
        }
        public void setEmptyServantSlot(int slot)
        {
            _emptySlotIndex = slot;
        }
        public void addServant(Servant servant)
        {
            insertServant(_servantList.Count, servant);
        }
        public void insertServant(int position, Servant servant)
        {
            servant.onClick += onServantClickCallback;
            servant.onBeginDrag += onServantBeginDragCallback;
            servant.onDrag += onServantDragCallback;
            servant.onEndDrag += onServantEndDragCallback;
            servant.onPointerEnter += onServantPointerEnterCallback;
            servant.onPointerExit += onServantPointerExitCallback;
            servant.transform.SetParent(_servantRoot, false);
            _servantList.Insert(position, servant);
        }
        public bool removeServant(Servant servant)
        {
            if (_servantList.Remove(servant))
            {
                servant.onClick -= onServantClickCallback;
                servant.onBeginDrag -= onServantBeginDragCallback;
                servant.onDrag -= onServantDragCallback;
                servant.onEndDrag -= onServantEndDragCallback;
                servant.onPointerEnter -= onServantPointerEnterCallback;
                servant.onPointerExit -= onServantPointerExitCallback;
                return true;
            }
            return false;
        }
        public void clear()
        {
            foreach (var servant in _servantList)
            {
                servant.onClick -= onServantClickCallback;
                servant.onBeginDrag -= onServantBeginDragCallback;
                servant.onDrag -= onServantDragCallback;
                servant.onEndDrag -= onServantEndDragCallback;
                servant.onPointerEnter -= onServantPointerEnterCallback;
                servant.onPointerExit -= onServantPointerExitCallback;
            }
            _servantList.Clear();
        }
        #region 私有方法
        #region 生命周期
        #endregion

        #region 事件回调
        private void onServantClickCallback(Servant servant, PointerEventData eventData)
        {
            onServantClick?.Invoke(servant, this, eventData);
        }
        private void onServantBeginDragCallback(Servant servant, PointerEventData eventData)
        {
            onServantBeginDrag?.Invoke(servant, this, eventData);
        }
        private void onServantDragCallback(Servant servant, PointerEventData eventData)
        {
            onServantDrag?.Invoke(servant, this, eventData);
        }
        private void onServantEndDragCallback(Servant servant, PointerEventData eventData)
        {
            onServantEndDrag?.Invoke(servant, this, eventData);
        }
        private void onServantPointerEnterCallback(Servant servant, PointerEventData eventData)
        {
            onServantPointerEnter?.Invoke(servant, this, eventData);
        }
        private void onServantPointerExitCallback(Servant servant, PointerEventData eventData)
        {
            onServantPointerExit?.Invoke(servant, this, eventData);
        }
        #endregion

        #region 接口实现
        public IEnumerator<Servant> GetEnumerator()
        {
            return ((IEnumerable<Servant>)_servantList).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_servantList).GetEnumerator();
        }
        #endregion
        private void alignServants(float deltaTime)
        {
            var width = _servantList.Count * _servantWidth;
            if (_emptySlotIndex >= 0)
            {
                width += _servantWidth;
            }
            float startX = -(width - _servantWidth) * 0.5f;
            for (int i = 0; i < _servantList.Count; i++)
            {
                if (i == _emptySlotIndex)
                {
                    startX += _servantWidth;
                }
                var servant = _servantList[i];
                var targetPosition = Vector3.left * startX;

                servant.transform.localPosition += (targetPosition - servant.transform.localPosition) * deltaTime * 10;
                startX = startX + _servantWidth;
            }
        }

        #endregion


        #region 事件
        public Action<Servant, Field, PointerEventData> onServantClick;
        public Action<Servant, Field, PointerEventData> onServantBeginDrag;
        public Action<Servant, Field, PointerEventData> onServantDrag;
        public Action<Servant, Field, PointerEventData> onServantEndDrag;
        public Action<Servant, Field, PointerEventData> onServantPointerEnter;
        public Action<Servant, Field, PointerEventData> onServantPointerExit;
        #endregion

        public Servant this[int i]
        {
            get => _servantList[i];
            set => _servantList[i] = value;
        }

        public int Count => _servantList.Count;

        [SerializeField]
        private Transform _servantRoot;
        [SerializeField]
        private List<Servant> _servantList;
        [SerializeField]
        private int _emptySlotIndex = -1;
        [SerializeField]
        private float _servantWidth = 1.7f;
    }
}