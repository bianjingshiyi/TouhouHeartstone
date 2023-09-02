using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class AnimationRefListItem : MonoBehaviour
    {
        #region 公有方法
        public void initDropdowns(string[] anims, string[] positions)
        {
            _animationDropdown.ClearOptions();
            _animationDropdown.AddOptions(new List<string>(anims));
            var posList = new List<string>(positions);
            _sourcePositionDropdown.ClearOptions();
            _sourcePositionDropdown.AddOptions(posList);
            _targetPositionDropdown.ClearOptions();
            _targetPositionDropdown.AddOptions(posList);
        }
        public void update(AnimationRefViewData viewData)
        {
            setKey(viewData.key);
            setAnimationIndex(viewData.animationIndex);
            setSourcePosition(viewData.sourcePosition);
            setTargetPosition(viewData.targetPosition);
        }
        public void setKey(string key)
        {
            _keyText.text = key;
        }
        public void setAnimationIndex(int index)
        {
            _animationDropdown.SetValueWithoutNotify(index);
        }
        public void setSourcePosition(int index)
        {
            _sourcePositionDropdown.SetValueWithoutNotify(index);
        }
        public void setTargetPosition(int index)
        {
            _targetPositionDropdown.SetValueWithoutNotify(index);
        }
        #endregion
        #region 私有方法
        #region 生命周期
        private void Awake()
        {
            _animationDropdown.onValueChanged.AddListener(onAnimationIndexChangedCallback);
            _sourcePositionDropdown.onValueChanged.AddListener(onSourcePositionChangedCallback);
            _targetPositionDropdown.onValueChanged.AddListener(onTargetPositionChangedCallback);
            _removeButton.onClick.AddListener(onRemoveButtonClickedCallback);
        }
        #endregion
        #region 事件回调
        private void onAnimationIndexChangedCallback(int index)
        {
            onAnimationChanged?.Invoke(this, index);
        }
        private void onSourcePositionChangedCallback(int index)
        {
            onSourcePositionChanged?.Invoke(this, index);
        }
        private void onTargetPositionChangedCallback(int index)
        {
            onTargetPositionChanged?.Invoke(this, index);
        }
        private void onRemoveButtonClickedCallback()
        {
            onRemoveButtonClicked?.Invoke(this);
        }
        #endregion
        #endregion
        #region 事件
        public event Action<AnimationRefListItem, int> onAnimationChanged;
        public event Action<AnimationRefListItem, int> onSourcePositionChanged;
        public event Action<AnimationRefListItem, int> onTargetPositionChanged;
        public event Action<AnimationRefListItem> onRemoveButtonClicked;
        #endregion
        [SerializeField]
        private Button _removeButton;
        [SerializeField]
        private Text _keyText;
        [SerializeField]
        private Dropdown _animationDropdown;
        [SerializeField]
        private Dropdown _sourcePositionDropdown;
        [SerializeField]
        private Dropdown _targetPositionDropdown;
    }
    public struct AnimationRefViewData
    {
        public string key;
        public int animationIndex;
        public int sourcePosition;
        public int targetPosition;
    }
}
