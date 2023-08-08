using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.BattleTable
{
    public class MeshButton : MonoBehaviour, IPointerClickHandler
    {
        #region 接口实现
        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            onClick?.Invoke();
        }
        #endregion

        #region 事件
        public Action onClick;
        #endregion
        #region 属性字段
        public string text
        {
            get => _text?.text ?? string.Empty;
            set
            {
                if (_text)
                    _text.text = value;
            }
        }
        [SerializeField]
        private TextMeshPro _text;
        #endregion
    }
}
