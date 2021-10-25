using UnityEngine;
using System;
using UnityEngine.UI;

namespace UI
{
    class BuildFilterDetailCardPoolItem : MonoBehaviour
    {
        public void setName(string name)
        {
            _nameText.text = name;
        }
        public void setValue(bool value)
        {
            _toggle.SetIsOnWithoutNotify(value);
        }
        protected void Awake()
        {
            _toggle.onValueChanged.AddListener(onToggleValueChanged);
        }
        private void onToggleValueChanged(bool value)
        {
            onValueChanged?.Invoke(this, value);
        }
        public event Action<BuildFilterDetailCardPoolItem, bool> onValueChanged;
        [SerializeField]
        Toggle _toggle;
        [SerializeField]
        Text _nameText;
    }
}