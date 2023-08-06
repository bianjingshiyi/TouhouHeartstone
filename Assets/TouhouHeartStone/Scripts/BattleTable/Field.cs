using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;

namespace Game.BattleTable
{
    public class Field : MonoBehaviour, IEnumerable<Servant>
    {
        public void updateField(float deltaTime)
        {
            alignServants(deltaTime);
        }
        public void setPreviewPosition(int position)
        {
            _previewPosition = position;
        }
        public Transform addPlaceHolder(int position)
        {
            Transform placeholder = Instantiate(_placeHolderTemplate, _placeHolderRoot).transform;
            placeholder.gameObject.SetActive(true);
            _placeHolders.Add(placeholder);
            _placeHolderPositions.Add(position);

            int count = getItemCount();
            placeholder.localPosition = getItemTargetPosition(position, count);
            return placeholder;
        }
        public bool removePlaceHolder(Transform transform)
        {
            var index = _placeHolders.IndexOf(transform);
            if (index >= 0)
            {
                _placeHolders.RemoveAt(index);
                _placeHolderPositions.RemoveAt(index);
                Destroy(transform.gameObject);
                return true;
            }
            return false;
        }
        public void addServant(Servant servant)
        {
            insertServant(_servantList.Count, servant);
        }
        public Servant addServant()
        {
            return insertServant(_servantList.Count);
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
        public Servant insertServant(int position)
        {
            Servant servant = Instantiate(_servantTemplate).GetComponent<Servant>();
            servant.gameObject.SetActive(true);
            insertServant(position, servant);

            int count = getItemCount();
            servant.transform.localPosition = getItemTargetPosition(position, count);
            return servant;
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
                servant.destroy();
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
                servant.destroy();
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
            int count = getItemCount();
            int position = 0;
            for (int i = 0; i < _servantList.Count; i++)
            {
                // 空白位置。
                for (int holderIndex = 0; holderIndex < _placeHolders.Count; holderIndex++ )
                {
                    int placePos = _placeHolderPositions[holderIndex];
                    if (placePos == i)
                    {
                        Transform placeHolder = _placeHolders[holderIndex];
                        updateItemTransform(placeHolder, position, count, deltaTime);
                        position++;
                    }
                }
                // 预览随从位置。
                if (i == _previewPosition)
                {
                    updateItemTransform(_previewPlaceHolder, position, count, deltaTime);
                    position++;
                }
                // 更新随从位置。
                var servant = _servantList[i];
                updateItemTransform(servant.transform, position, count, deltaTime);
                position++;
            }
        }

        private void updateItemTransform(Transform trans, int index, int count, float deltaTime)
        {
            Vector3 targetPosition = getItemTargetPosition(index, count);
            var distance = (targetPosition - trans.transform.localPosition).magnitude;
            trans.transform.localPosition = Vector3.MoveTowards(trans.transform.localPosition, targetPosition, distance * deltaTime * 10);
        }
        private int getItemCount()
        {
            bool hasPreview = _previewPosition >= 0;
            return _servantList.Count + _placeHolders.Count + (hasPreview ? 1 : 0);
        }
        private Vector3 getItemTargetPosition(int position, int count)
        {
            var width = count * _servantWidth;
            float startX = -(width - _servantWidth) * 0.5f;
            float x = startX + position * _servantWidth;
            return Vector3.left * x;
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
        public Transform PreviewPlaceHolder => _previewPlaceHolder;

        public int Count => _servantList.Count;

        [Header("Servants")]
        [SerializeField]
        private Transform _servantRoot;
        [SerializeField]
        private List<Servant> _servantList = new List<Servant>();
        [SerializeField]
        private float _servantWidth = 1.7f;
        [SerializeField]
        private GameObject _servantTemplate;

        [Header("Preview")]
        [SerializeField]
        private Transform _previewPlaceHolder;
        [SerializeField]
        private int _previewPosition = -1;

        [Header("Place Holders")]
        [SerializeField]
        private Transform _placeHolderRoot;
        [SerializeField]
        private List<Transform> _placeHolders = new List<Transform>();
        [SerializeField]
        private List<int> _placeHolderPositions = new List<int>();
        [SerializeField]
        private GameObject _placeHolderTemplate;
    }
}