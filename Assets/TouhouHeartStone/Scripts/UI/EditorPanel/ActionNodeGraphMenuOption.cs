using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    class ActionNodeGraphMenuOption : MonoBehaviour
    {
        public void setTitle(string name)
        {
            _nameText.text = name;
        }
        protected void Awake()
        {
            _button.onClick.AddListener(onClickCallback);
        }
        private void onClickCallback()
        {
            onClick?.Invoke(this);
        }
        public event Action<ActionNodeGraphMenuOption> onClick;
        [SerializeField]
        Button _button;
        [SerializeField]
        Text _nameText;
    }
}