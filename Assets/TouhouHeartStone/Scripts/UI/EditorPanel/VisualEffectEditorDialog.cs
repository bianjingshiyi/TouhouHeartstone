
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class VisualEffectEditorDialog : MonoBehaviour
    {
        #region 公有方法
        public void initEffects(string[] effectNames)
        {
            _effectDropdown.ClearOptions();
            _effectDropdown.AddOptions(new List<string>(effectNames));
        }
        public void initEvents(string[] eventNames)
        {
            foreach (var obj in _eventDropdownFieldObjects)
            {
                obj.SetActive(eventNames != null);
            }
            _eventDropdown.ClearOptions();
            if (eventNames != null)
                _eventDropdown.AddOptions(new List<string>(eventNames));
        }
        public void initPositionOptions(string[] options)
        {
            _sourcePositionDropdown.ClearOptions();
            _sourcePositionDropdown.AddOptions(new List<string>(options));
            _targetPositionDropdown.ClearOptions();
            _targetPositionDropdown.AddOptions(new List<string>(options));
        }
        public void update(VisualEffectDialogViewData viewData)
        {
            setEvent(viewData.eventIndex);
            setAnimation(viewData.animIndex);
            setSourcePosition(viewData.sourcePosition);
            setSourcePositionVarName(viewData.sourcePositionVarName);
            foreach (var obj in _sourcePositionVarNameObjects)
            {
                obj.SetActive(viewData.sourcePositionVarNameVisible);
            }
            setTargetPosition(viewData.targetPosition);
            setTargetPositionVarName(viewData.targetPositionVarName);
            foreach (var obj in _targetPositionVarNameObjects)
            {
                obj.SetActive(viewData.targetPositionVarNameVisible);
            }
            _colorModifyItems.updateList(viewData.colorModifies.Length, (i, rect) =>
            {
                var colorModifyItem = rect.GetComponent<ColorModifyItem>();
                colorModifyItem.update(viewData.colorModifies[i]);
            });
        }
        public void setEvent(int index)
        {
            _eventDropdown.SetValueWithoutNotify(index);
        }
        public void setAnimation(int index)
        {
            _effectDropdown.SetValueWithoutNotify(index);
        }
        public void setSourcePosition(int index)
        {
            _sourcePositionDropdown.SetValueWithoutNotify(index);
        }
        public void setTargetPosition(int index)
        {
            _targetPositionDropdown.SetValueWithoutNotify(index);
        }
        public void setSourcePositionVarName(string varName)
        {
            _sourcePositionVarNameField.SetTextWithoutNotify(varName);
        }
        public void setTargetPositionVarName(string varName)
        {
            _targetPositionVarNameField.SetTextWithoutNotify(varName);
        }
        #endregion
        #region 私有方法
        protected void Awake()
        {
            _eventDropdown.onValueChanged.AddListener(v => onValueChangedCallback());
            _effectDropdown.onValueChanged.AddListener(v => onValueChangedCallback());
            _sourcePositionDropdown.onValueChanged.AddListener(v => onValueChangedCallback());
            _targetPositionDropdown.onValueChanged.AddListener(v => onValueChangedCallback());
            _sourcePositionVarNameField.onEndEdit.AddListener(v => onValueChangedCallback());
            _targetPositionVarNameField.onEndEdit.AddListener(v => onValueChangedCallback());
            _confirmButton.onClick.AddListener(OnConfirmCallback);
        }
        private void onValueChangedCallback()
        {
            onPropValueChanged?.Invoke(toInfo());
        }
        private void OnConfirmCallback()
        {
            onConfirm?.Invoke();
        }
        private VisualEffectDialogInfo toInfo()
        {
            return new VisualEffectDialogInfo()
            {
                eventIndex = _eventDropdown.value,
                animIndex = _effectDropdown.value - 1,
                sourcePosition = _sourcePositionDropdown.value,
                targetPosition = _targetPositionDropdown.value,
                sourcePositionVarName = _sourcePositionVarNameField.text,
                targetPositionVarName = _targetPositionVarNameField.text,
            };
        }
        #endregion
        #region 事件
        public event Action<VisualEffectDialogInfo> onPropValueChanged;
        public event Action onConfirm;
        #endregion
        #region 属性字段
        [SerializeField]
        private Dropdown _eventDropdown;
        [SerializeField]
        private Dropdown _effectDropdown;
        [SerializeField]
        private Dropdown _sourcePositionDropdown;
        [SerializeField]
        private Dropdown _targetPositionDropdown;
        [SerializeField]
        private GameObject[] _eventDropdownFieldObjects;
        [SerializeField]
        private GameObject[] _sourcePositionVarNameObjects;
        [SerializeField]
        private GameObject[] _targetPositionVarNameObjects;
        [SerializeField]
        private InputField _sourcePositionVarNameField;
        [SerializeField]
        private InputField _targetPositionVarNameField;
        [SerializeField]
        private ElementList _colorModifyItems;
        [SerializeField]
        private Button _confirmButton;
        #endregion
    }
    public struct VisualEffectDialogInfo
    {
        public int eventIndex;
        public int animIndex;
        public int sourcePosition;
        public int targetPosition;
        public string sourcePositionVarName;
        public string targetPositionVarName;
    }
    public struct VisualEffectDialogViewData
    {
        public int eventIndex;
        public int animIndex;
        public int sourcePosition;
        public int targetPosition;
        public string sourcePositionVarName;
        public string targetPositionVarName;
        public bool sourcePositionVarNameVisible;
        public bool targetPositionVarNameVisible;
        public ColorModifyViewData[] colorModifies;
    }
}