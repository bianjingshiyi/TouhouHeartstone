using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.BattleTable
{
    public class HeadSlots : MonoBehaviour
    {
        #region 公有事件
        public void updateHead(float deltaTime)
        {
            alignItems(deltaTime);
        }
        public Secret createItem()
        {
            var obj = Instantiate(_itemTemplate, _itemRoot);
            obj.SetActive(true);
            var secret = obj.GetComponent<Secret>();
            return secret;
        }
        public void addItem(Secret item)
        {
            _itemList.Insert(_itemList.Count, item);
        }
        public void insertItem(int position, Secret item)
        {
            item.onPointerEnter += onItemPointerEnterCallback;
            item.onPointerExit += onItemPointerExitCallback;
            _itemList.Insert(position, item);
        }
        public bool removeItem(Secret item)
        {
            if (_itemList.Remove(item))
            {
                item.onPointerEnter -= onItemPointerEnterCallback;
                item.onPointerExit -= onItemPointerExitCallback;
                return true;
            }
            return false;
        }
        public void clear()
        {
            foreach (var item in _itemList)
            {
                item.onPointerEnter -= onItemPointerEnterCallback;
                item.onPointerExit -= onItemPointerExitCallback;
            }
            _itemList.Clear();
        }
        #endregion
        #region 私有方法
        #region 生命周期
        private void Awake()
        {
            _itemTemplate.SetActive(false);
        }
        #endregion

        #region 事件回调
        private void onItemPointerEnterCallback(Secret item)
        {
            onItemPointerEnter?.Invoke(item, this);
        }
        private void onItemPointerExitCallback(Secret item)
        {
            onItemPointerExit?.Invoke(item, this);
        }
        #endregion

        private void alignItems(float deltaTime)
        {
            int position = 0;
            for (int i = 0; i < _itemList.Count; i++)
            {
                // 更新随从位置。
                var item = _itemList[i];
                updateItemTransform(item.transform, position, deltaTime);
                position++;
            }
        }

        private void updateItemTransform(Transform trans, int index, float deltaTime)
        {
            Vector3 targetPosition = getItemTargetPosition(index);
            var distance = (targetPosition - trans.transform.localPosition).magnitude;
            trans.transform.localPosition = Vector3.MoveTowards(trans.transform.localPosition, targetPosition, distance * deltaTime * 10);
        }
        private Vector3 getItemTargetPosition(int position)
        {
            if (position < 0 || position >= _targetTransforms.Length)
            {
                position = 0;
            }
            return _itemRoot.InverseTransformPoint(_targetTransforms[position].position);
        }

        #endregion

        #region 事件
        public event Action<Secret, HeadSlots> onItemPointerEnter;
        public event Action<Secret, HeadSlots> onItemPointerExit;
        #endregion

        #region 属性字段
        [SerializeField]
        private GameObject _itemTemplate;
        [SerializeField]
        private Transform _itemRoot;
        [SerializeField]
        private List<Secret> _itemList = new List<Secret>();
        [SerializeField]
        private Transform[] _targetTransforms;
        #endregion
    }
}