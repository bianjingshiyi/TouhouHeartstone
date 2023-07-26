using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.BattleTable
{
    public class Secret : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public void setActive(bool open)
        {
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
    }
}