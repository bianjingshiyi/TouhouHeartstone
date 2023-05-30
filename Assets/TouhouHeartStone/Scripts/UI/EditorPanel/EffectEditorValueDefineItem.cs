using System;
using UnityEngine;
using UnityEngine.UI;
namespace UI
{
    public class EffectEditorValueDefineItem : MonoBehaviour
    {
        #region 公有方法
        public void setType(string typeName)
        {
            _typeNameText.text = typeName;
        }
        public void setName(string name)
        {
            _nameText.text = name;
        }
        #endregion
        #region 私有方法
        #region 生命周期
        protected void Awake()
        {
            _deleteButton.onClick.AddListener(onDeleteButtonClickedCallback);
        }
        #endregion
        #region 事件回调
        private void onDeleteButtonClickedCallback()
        {
            onDeleteButtonClicked?.Invoke(this);
        }
        #endregion
        #endregion
        #region 事件
        public event Action<EffectEditorValueDefineItem> onDeleteButtonClicked;
        #endregion
        #region 属性字段
        [SerializeField]
        Text _typeNameText;
        [SerializeField]
        Text _nameText;
        [SerializeField]
        Button _deleteButton;
        #endregion
    }
}