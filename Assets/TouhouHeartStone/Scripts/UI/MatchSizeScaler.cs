using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    [AddComponentMenu("Layout/Match Size Scaler", 142)]
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    /// <summary>
    /// Resizes a RectTransform to fit a specified aspect ratio.
    /// </summary>
    public class MatchSizeScaler : UIBehaviour, ILayoutSelfController
    {
        #region 公有方法
        public bool IsComponentValidOnObject()
        {
            Canvas canvas = gameObject.GetComponent<Canvas>();
            if (canvas && canvas.isRootCanvas && canvas.renderMode != RenderMode.WorldSpace)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Method called by the layout system. Has no effect
        /// </summary>
        public virtual void SetLayoutHorizontal() { }

        /// <summary>
        /// Method called by the layout system. Has no effect
        /// </summary>
        public virtual void SetLayoutVertical() { }
        #endregion

        #region 私有方法
        protected MatchSizeScaler() { }

        protected override void OnEnable()
        {
            base.OnEnable();
            SetDirty();
        }

        protected override void OnDisable()
        {
            m_Tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            base.OnDisable();
        }

        protected override void Start()
        {
            base.Start();
            //Disable the component if the aspect mode is not valid or the object state/setup is not supported with AspectRatio setup.
            if (!IsComponentValidOnObject())
                this.enabled = false;
        }
        /// <summary>
        /// Update the rect based on the delayed dirty.
        /// Got around issue of calling onValidate from OnEnable function.
        /// </summary>
        protected virtual void Update()
        {
            if (m_DelayedSetDirty)
            {
                m_DelayedSetDirty = false;
                SetDirty();
            }
        }

        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();
            SetDirty();
        }

        /// <summary>
        /// Function called when this RectTransform or parent RectTransform has changed dimensions.
        /// </summary>
        protected override void OnRectTransformDimensionsChange()
        {
            UpdateRect();
        }
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            m_DelayedSetDirty = true;
        }

#endif

        /// <summary>
        /// Mark the AspectRatioFitter as dirty.
        /// </summary>
        protected void SetDirty()
        {
            UpdateRect();
        }
        private void UpdateRect()
        {
            if (!IsActive() || !IsComponentValidOnObject())
                return;

            m_Tracker.Clear();

            switch (m_ScaleMode)
            {
                case ScaleMode.Width:
                    {
                        m_Tracker.Add(this, rectTransform, DrivenTransformProperties.Scale);
                        var scale = Mathf.Clamp(rectTransform.rect.width / m_TargetWidth, m_MinScale, m_MaxScale);
                        rectTransform.localScale = new Vector3(scale, scale, 1);
                        break;
                    }
                case ScaleMode.Height:
                    {
                        m_Tracker.Add(this, rectTransform, DrivenTransformProperties.Scale);
                        var scale = Mathf.Clamp(rectTransform.rect.height / m_TargetHeight, m_MinScale, m_MaxScale);
                        rectTransform.localScale = new Vector3(scale, scale, 1);
                        break;
                    }
                case ScaleMode.Min:
                    {
                        m_Tracker.Add(this, rectTransform, DrivenTransformProperties.Scale);
                        var scale = Mathf.Min(rectTransform.rect.width / m_TargetWidth, rectTransform.rect.height / m_TargetHeight);
                        scale = Mathf.Clamp(scale, m_MinScale, m_MaxScale);
                        rectTransform.localScale = new Vector3(scale, scale, 1);
                        break;
                    }
                case ScaleMode.Max:
                    {
                        m_Tracker.Add(this, rectTransform, DrivenTransformProperties.Scale);
                        var scale = Mathf.Max(rectTransform.rect.width / m_TargetWidth, rectTransform.rect.height / m_TargetHeight);
                        scale = Mathf.Clamp(scale, m_MinScale, m_MaxScale);
                        rectTransform.localScale = new Vector3(scale, scale, 1);
                        break;
                    }
            }
        }

        private float GetSizeDeltaToProduceSize(float size, int axis)
        {
            return size - GetParentSize()[axis] * (rectTransform.anchorMax[axis] - rectTransform.anchorMin[axis]);
        }

        private Vector2 GetParentSize()
        {
            RectTransform parent = rectTransform.parent as RectTransform;
            return !parent ? Vector2.zero : parent.rect.size;
        }

        private static bool SetStruct<T>(ref T currentValue, T newValue) where T : struct
        {
            if (EqualityComparer<T>.Default.Equals(currentValue, newValue))
            {
                return false;
            }

            currentValue = newValue;
            return true;
        }

        #endregion

        #region 属性字段
        public ScaleMode scaleMode { get { return m_ScaleMode; } set { if (SetStruct(ref m_ScaleMode, value)) SetDirty(); } }
        public float targetWidth { get { return m_TargetWidth; } set { if (SetStruct(ref m_TargetWidth, value)) SetDirty(); } }
        public float targetHeight { get { return m_TargetHeight; } set { if (SetStruct(ref m_TargetHeight, value)) SetDirty(); } }
        public float minScale { get { return m_MinScale; } set { if (SetStruct(ref m_MinScale, value)) SetDirty(); } }
        public float maxScale { get { return m_MaxScale; } set { if (SetStruct(ref m_MaxScale, value)) SetDirty(); } }
        private RectTransform rectTransform
        {
            get
            {
                if (m_Rect == null)
                    m_Rect = GetComponent<RectTransform>();
                return m_Rect;
            }
        }
        [SerializeField]
        private ScaleMode m_ScaleMode = ScaleMode.None;
        [SerializeField]
        private float m_TargetWidth = 100;
        [SerializeField]
        private float m_TargetHeight = 100;
        [SerializeField]
        private float m_MinScale = 1;
        [SerializeField]
        private float m_MaxScale = 1;
        [System.NonSerialized]
        private RectTransform m_Rect;
        private bool m_DelayedSetDirty;
        // field is never assigned warning
#pragma warning disable 649
        private DrivenRectTransformTracker m_Tracker;
#pragma warning restore 649

        #endregion

        #region 内嵌类
        public enum ScaleMode
        {
            None,
            Width,
            Height,
            Min,
            Max
        }
        #endregion
    }
}
