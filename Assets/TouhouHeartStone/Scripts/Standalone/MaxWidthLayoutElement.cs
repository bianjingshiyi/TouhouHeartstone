using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 
/// </summary>
/// <see cref="https://forum.unity.com/threads/limit-max-width-of-layout-component.316625/#post-6835478"/>
[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public class MaxWidthLayoutElement : UIBehaviour, ILayoutElement
{
    private float m_PreferredWidth;

    public float MaxForcedWidth;
    public LayoutGroup LayoutGroup;

    public virtual float minWidth => -1;
    public virtual float minHeight => -1;
    public virtual float preferredWidth => m_PreferredWidth;
    public virtual float preferredHeight => -1;
    public virtual float flexibleWidth => -1;
    public virtual float flexibleHeight => -1;
    public virtual int layoutPriority => 1;

    public virtual void CalculateLayoutInputHorizontal() { }
    public virtual void CalculateLayoutInputVertical() { }

    #region Unity Lifetime calls

    protected override void OnEnable()
    {
        base.OnEnable();
        SetDirty();
    }

    protected override void OnTransformParentChanged()
    {
        LayoutGroup.CalculateLayoutInputHorizontal();
        var preferredWidth = LayoutGroup.preferredWidth;
        var parentWidth = ((RectTransform)transform.parent).rect.width;
        m_PreferredWidth = Math.Max(parentWidth, Math.Min(preferredWidth, MaxForcedWidth));
        SetDirty();
    }

    protected override void OnDisable()
    {
        SetDirty();
        base.OnDisable();
    }

    protected override void OnDidApplyAnimationProperties()
    {
        SetDirty();
    }

    protected override void OnBeforeTransformParentChanged()
    {
        SetDirty();
    }

    #endregion

    protected void SetDirty()
    {
        if (!IsActive()) { return; }
        LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        SetDirty();
    }

#endif
}
