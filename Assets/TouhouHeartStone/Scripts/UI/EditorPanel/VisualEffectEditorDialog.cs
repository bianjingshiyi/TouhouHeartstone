
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
            var flags = viewData.hideFlags;
            setEvent(viewData.eventIndex);
            setAnimation(viewData.animIndex);
            setSourcePosition(viewData.sourcePosition);
            setSourcePositionVarName(viewData.sourcePositionVarName);
            setTargetPosition(viewData.targetPosition);
            setTargetPositionVarName(viewData.targetPositionVarName);
            foreach (var obj in _eventDropdownFieldObjects)
            {
                obj.SetActive(!flags.HasFlag(VFXDialogHideFlags.Event));
            }
            foreach (var obj in _sourcePositionFieldObjects)
            {
                obj.SetActive(!flags.HasFlag(VFXDialogHideFlags.SourcePosition));
            }
            foreach (var obj in _targetPositionFieldObjects)
            {
                obj.SetActive(!flags.HasFlag(VFXDialogHideFlags.TargetPosition));
            }
            foreach (var obj in _sourcePositionVarNameObjects)
            {
                obj.SetActive(!flags.HasFlag(VFXDialogHideFlags.SourcePositionVarName));
            }
            foreach (var obj in _targetPositionVarNameObjects)
            {
                obj.SetActive(!flags.HasFlag(VFXDialogHideFlags.TargetPositionVarName));
            }
        }
        public void updateColors(ColorModifyViewData[] colors)
        {
            _colorModifyItems.updateList(colors.Length, (i, rect) =>
            {
                var colorModifyItem = rect.GetComponent<ColorModifyItem>();
                colorModifyItem.update(colors[i]);
            },
            rect =>
            {
                var colorModifyItem = rect.GetComponent<ColorModifyItem>();
                colorModifyItem.onClick += onColorModifyItemClickedCallback;
            },
            rect =>
            {
                var colorModifyItem = rect.GetComponent<ColorModifyItem>();
                colorModifyItem.onClick -= onColorModifyItemClickedCallback;
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

        public void resetPreview()
        {
            _previewContent.localScale = Vector3.one;
            _previewTable.stop();
            updatePreviewPaused();
        }
        public void setPreviewVisible(bool value)
        {
            _previewRoot.SetActive(value);
            _previewTableRoot.SetActive(value);
        }
        public bool isPreviewVisible()
        {
            return _previewRoot.activeSelf;
        }
        public void setShowPreviewButtonText(string text)
        {
            _showPreviewButtonText.text = text;
        }
        public void updatePreviewPaused()
        {
            _pausePreviewActiveImage.color = _previewTable.isPaused() ? _pausePreviewActiveColor : _pausePreviewInactiveColor;
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
            _showPreviewButton.onClick.AddListener(() => onShowPreviewButtonClicked?.Invoke());
            _playPreviewButton.onClick.AddListener(() => onPreviewPlayButtonClicked?.Invoke());
            _pausePreviewButton.onClick.AddListener(() => onPreviewPauseButtonClicked?.Invoke());
            _stopPreviewButton.onClick.AddListener(() => onPreviewStopButtonClicked?.Invoke());
            _previewTable.onFinished += () => updatePreviewPaused();
            setPreviewVisible(false);
        }
        #region 事件回调
        private void onValueChangedCallback()
        {
            onPropValueChanged?.Invoke(toInfo());
        }
        private void onColorModifyItemClickedCallback(ColorModifyItem item)
        {
            onColorModifyItemClicked?.Invoke(_colorModifyItems.indexOf(item));
        }
        private void OnConfirmCallback()
        {
            onConfirm?.Invoke();
        }
        #endregion
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
        public event Action<int> onColorModifyItemClicked;
        public event Action onConfirm;

        public event Action onShowPreviewButtonClicked;
        public event Action onPreviewPlayButtonClicked;
        public event Action onPreviewPauseButtonClicked;
        public event Action onPreviewStopButtonClicked;
        #endregion
        #region 属性字段
        public AnimationPreviewTable previewTable => _previewTable;
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
        private GameObject[] _sourcePositionFieldObjects;
        [SerializeField]
        private GameObject[] _targetPositionFieldObjects;
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

        [Header("Preview")]
        [SerializeField]
        private Button _showPreviewButton;
        [SerializeField]
        private Text _showPreviewButtonText;
        [SerializeField]
        private GameObject _previewRoot;
        [SerializeField]
        private GameObject _previewTableRoot;
        [SerializeField]
        private Transform _previewContent;
        [SerializeField]
        private AnimationPreviewTable _previewTable;

        [SerializeField]
        private Button _playPreviewButton;
        [SerializeField]
        private Button _pausePreviewButton;
        [SerializeField]
        private Button _stopPreviewButton;

        [SerializeField]
        private Image _pausePreviewActiveImage;
        [SerializeField]
        private Color _pausePreviewActiveColor;
        [SerializeField]
        private Color _pausePreviewInactiveColor;
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
        public VFXDialogHideFlags hideFlags;
    }
    [Flags]
    public enum VFXDialogHideFlags
    {
        None = 0,
        Event = 1,
        Animation = 1 << 1,
        SourcePosition = 1 << 2,
        TargetPosition = 1 << 3,
        SourcePositionVarName = 1 << 4,
        TargetPositionVarName = 1 << 5,
    }
}