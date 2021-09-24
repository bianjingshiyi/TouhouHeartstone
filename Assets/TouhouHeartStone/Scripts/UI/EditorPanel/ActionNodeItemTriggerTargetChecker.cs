using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    class ActionNodeItemTriggerTargetChecker : MonoBehaviour
    {
        public void setName(string name)
        {
            _nameText.text = name;
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
        public event Action<ActionNodeItemTriggerTargetChecker> onClickEditButton;
        public event Action<ActionNodeItemTriggerTargetChecker> onClickRemoveButton;
        [SerializeField]
        Button _editButton;
        [SerializeField]
        Button _removeButton;
        [SerializeField]
        Text _nameText;
    }
}