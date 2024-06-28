using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.BattleTable
{
    public class MulliganDialog : MonoBehaviour
    {
        public void setActive(bool active)
        {
            gameObject.SetActive(active);
        }
        public void setWaiting(bool waiting)
        {
            _waitingText.gameObject.SetActive(waiting);
            _acceptButton.gameObject.SetActive(!waiting);
        }
        public MulliganCardItem getItem(int index)
        {
            return _cardList.getItem(index);
        }
        public int indexOf(MulliganCardItem item)
        {
            return _cardList.indexOf(item);
        }
        public MulliganCardItem addChoice()
        {
            return _cardList.add();
        }
        public void clearChoices()
        {
            _cardList.clear();
        }
        #region 私有方法
        #region 生命周期
        private void Awake()
        {
            _acceptButton.onClick.AddListener(onAcceptButtonClickCallback);
            _cardList.onItemPointerEnter += onItemPointerEnterCallback;
            _cardList.onItemPointerExit += onItemPointerExitCallback;
            _cardList.onItemClicked += onItemClickCallback;
        }
        #endregion
        private void onItemPointerEnterCallback(int index)
        {
            onItemPointerEnter?.Invoke(index);
        }
        private void onItemPointerExitCallback(int index)
        {
            onItemPointerExit?.Invoke(index);
        }
        private void onItemClickCallback(int index)
        {
            onItemClicked?.Invoke(index);
        }
        private void onAcceptButtonClickCallback()
        {
            onAccept?.Invoke();
        }
        #endregion

        #region 事件
        public Action<int> onItemPointerEnter;
        public Action<int> onItemPointerExit;
        public Action<int> onItemClicked;
        public Action onAccept;
        #endregion

        #region 属性字段
        [SerializeField]
        private MulliganCardList _cardList;
        [SerializeField]
        private Text _waitingText;
        [SerializeField]
        private Button _acceptButton;
        #endregion
    }
}
