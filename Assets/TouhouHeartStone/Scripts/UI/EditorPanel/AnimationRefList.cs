using System;
using System.Collections.Generic;
using BJSYGameCore.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class AnimationRefList : MonoBehaviour
    {
        public void update(AnimationRefViewData[] viewDatas, string[] anims, string[] positions)
        {
            UIHelper.updateList(_animationRefListRoot, _animationRefTemplate, _animationRefList, viewDatas.Length,
                (index, rect) =>
                {
                    var item = rect.GetComponent<AnimationRefListItem>();
                    item.initDropdowns(anims, positions);
                    item.update(viewDatas[index]);
                },
                rect =>
                {
                    var item = rect.GetComponent<AnimationRefListItem>();
                    item.onAnimationChanged += onAnimationChangedCallback;
                    item.onSourcePositionChanged += onSourcePositionChangedCallback;
                    item.onTargetPositionChanged += onTargetPositionChangedCallback;
                    item.onRemoveButtonClicked += onRemoveButtonClickedCallback;
                },
                rect =>
                {
                    var item = rect.GetComponent<AnimationRefListItem>();
                    item.onAnimationChanged -= onAnimationChangedCallback;
                    item.onSourcePositionChanged -= onSourcePositionChangedCallback;
                    item.onTargetPositionChanged -= onTargetPositionChangedCallback;
                    item.onRemoveButtonClicked -= onRemoveButtonClickedCallback;
                });
        }
        #region 私有方法
        #region 生命周期
        private void Awake()
        {
            _addAnimationRefButton.onClick.AddListener(onAddButtonClickedCallback);
        }
        #endregion
        #region 事件回调
        private void onAddButtonClickedCallback()
        {
            onAddButtonClicked?.Invoke();
        }
        private void onAnimationChangedCallback(AnimationRefListItem item, int index)
        {
            onAnimationChanged?.Invoke(_animationRefList.IndexOf(item.transform as RectTransform), index);
        }
        private void onSourcePositionChangedCallback(AnimationRefListItem item, int index)
        {
            onSourcePositionChanged?.Invoke(_animationRefList.IndexOf(item.transform as RectTransform), index);
        }
        private void onTargetPositionChangedCallback(AnimationRefListItem item, int index)
        {
            onTargetPositionChanged?.Invoke(_animationRefList.IndexOf(item.transform as RectTransform), index);
        }
        private void onRemoveButtonClickedCallback(AnimationRefListItem item)
        {
            onRemoveButtonClicked?.Invoke(_animationRefList.IndexOf(item.transform as RectTransform));
        }
        #endregion
        #endregion
        #region 事件
        public event Action onAddButtonClicked;
        public event Action<int, int> onAnimationChanged;
        public event Action<int, int> onSourcePositionChanged;
        public event Action<int, int> onTargetPositionChanged;
        public event Action<int> onRemoveButtonClicked;
        #endregion
        [SerializeField]
        private Button _addAnimationRefButton;
        [SerializeField]
        private RectTransform _animationRefListRoot;
        [SerializeField]
        private List<RectTransform> _animationRefList;
        [SerializeField]
        private RectTransform _animationRefTemplate;
    }
}
