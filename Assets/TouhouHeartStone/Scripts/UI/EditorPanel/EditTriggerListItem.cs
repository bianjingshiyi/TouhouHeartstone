using System;
using UnityEngine;
using UnityEngine.UI;
namespace UI
{
    public class EditTriggerListItem : MonoBehaviour
    {
        public void updateView(string triggerName)
        {
            _nameText.text = "编辑" + triggerName;
        }
        protected void Awake()
        {
            _editButton.onClick.AddListener(onClickEditButtonCallback);
            _removeButton.onClick.AddListener(onClickRemoveButtonCallback);
        }
        private void onClickEditButtonCallback()
        {
            onClickEditButton?.Invoke(this);
        }
        private void onClickRemoveButtonCallback()
        {
            onClickRemoveButton?.Invoke(this);
        }
        public event Action<EditTriggerListItem> onClickEditButton;
        public event Action<EditTriggerListItem> onClickRemoveButton;
        [SerializeField]
        Text _nameText;
        [SerializeField]
        Button _editButton;
        [SerializeField]
        Button _removeButton;
    }
}