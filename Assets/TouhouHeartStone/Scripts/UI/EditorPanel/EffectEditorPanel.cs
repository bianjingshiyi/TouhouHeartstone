using System;
using UnityEngine;

namespace UI
{
    public class EffectEditorPanel : MonoBehaviour
    {
        #region 公有方法
        public void setEventTypeNames(string[] names)
        {
            if (_createEventEffectDialog)
                _createEventEffectDialog.setTypes(names);
        }
        public void setEventTypeValue(int index)
        {
            if (_createEventEffectDialog)
                _createEventEffectDialog.setEventValue(index);
        }
        public void displayEventEffectDialog(bool display)
        {
            if (!_createEventEffectDialog)
                return;
            if (display)
            {
                _createEventEffectDialog.display();
            }
            else
            {
                _createEventEffectDialog.hide();
            }
        }
        #endregion
        #region 私有方法

        #region 生命周期
        protected void Awake()
        {
            // Event Effects.
            if (_createEventEffectDialog)
                _createEventEffectDialog.onConfirm += index => onConfirmCreateEventEffect?.Invoke(index);
        }
        #endregion

        #endregion
        #region 事件
        /// <summary>
        /// 当确认创建事件效果。
        /// </summary>
        public event Action<int> onConfirmCreateEventEffect;

        #endregion
        [Header("Event Effects")]
        [SerializeField]
        CreateEventEffectDialog _createEventEffectDialog;
    }
}