using System;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.BattleTable
{
    public class Secret : TableUnit, IPointerEnterHandler, IPointerExitHandler
    {
        public void setOpen(bool open)
        {
        }

        public void setMission(bool mission)
        {
            _secretObj.SetActive(!mission);
            _missionObj.SetActive(mission);
        }

        #region 事件回调
        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            onPointerEnter?.Invoke(this);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            onPointerExit?.Invoke(this);
        }
        #endregion

        #region 事件
        public event Action<Secret> onPointerEnter;
        public event Action<Secret> onPointerExit;
        #endregion

        #region 属性字段
        public Transform dummyTarget => _dummyTarget;
        public TableCard card => _card;

        public SimpleAnim onEnter;
        public SimpleAnim onTrigger;
        public SimpleAnim onExit;
        [SerializeField]
        private GameObject _secretObj;
        [SerializeField]
        private GameObject _missionObj;
        [SerializeField]
        private Transform _dummyTarget;
        [SerializeField]
        private TableCard _card;
        #endregion
    }
}