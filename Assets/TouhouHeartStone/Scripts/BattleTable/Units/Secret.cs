using System;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.BattleTable
{
    public class Secret : TableUnit, IPointerEnterHandler, IPointerExitHandler
    {
        public void setMission(bool mission)
        {
            _secretObj.SetActive(!mission);
            _missionObj.SetActive(mission);
        }
        public void setSecretOpen(bool open)
        {
            animator.SetBool("Darken", !open);
        }
        public void triggerMission(string text)
        {
            _missionText.text = text;
            animator.SetTrigger("MissionTrigger");
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
        public HeadSlots headSlots { get; set; }
        public SimpleAnim onSecretEnter;
        public SimpleAnim onSecretTrigger;
        public SimpleAnim onSecretExit;
        public SimpleAnim onMissionEnter;
        public SimpleAnim onMissionTrigger;
        public SimpleAnim onMissionExit;
        public SimpleAnim onMissionCompleted;
        public SimpleAnim onSetDefine;
        [SerializeField]
        private GameObject _secretObj;
        [SerializeField]
        private GameObject _missionObj;
        [SerializeField]
        private Transform _dummyTarget;
        [SerializeField]
        private TextMeshPro _missionText;
        [SerializeField]
        private TableCard _card;
        #endregion
    }
}