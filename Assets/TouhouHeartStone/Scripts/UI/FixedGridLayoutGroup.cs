using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.UI;

namespace UI
{
    [AddComponentMenu("Layout/Fixed Grid Layout Group", 152)]
    /// <summary>
    /// Layout class to arrange child elements in a grid format.
    /// </summary>
    /// <remarks>
    /// The GridLayoutGroup component is used to layout child layout elements in a uniform grid where all cells have the same size. The size and the spacing between cells is controlled by the GridLayoutGroup itself. The children have no influence on their sizes.
    /// </remarks>
    public class FixedGridLayoutGroup : LayoutGroup
    {
        #region 公有方法
        /// <summary>
        /// Called by the layout system to calculate the horizontal layout size.
        /// Also see ILayoutElement
        /// </summary>
        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();

            if (m_Constraint == Constraint.FixedRowCount)
            {
                int minColumns = Mathf.CeilToInt(rectChildren.Count / (float)m_ConstraintCount - 0.001f);
                float minSpace = padding.horizontal + (cellSizeValue + spacing.x) * minColumns - spacing.x;
                SetLayoutInputForAxis(minSpace, minSpace, -1, 0);
            }
            else
            {
                SetLayoutInputForAxis(0, 0, -1, 0);
            }
        }

        /// <summary>
        /// Called by the layout system to calculate the vertical layout size.
        /// Also see ILayoutElement
        /// </summary>
        public override void CalculateLayoutInputVertical()
        {
            if (m_Constraint == Constraint.FixedColumnCount)
            {
                int minRows = Mathf.CeilToInt(rectChildren.Count / (float)m_ConstraintCount - 0.001f);
                float minSpace = padding.vertical + (cellSizeValue + spacing.y) * minRows - spacing.y;
                SetLayoutInputForAxis(minSpace, minSpace, -1, 1);
            }
            else
            {
                SetLayoutInputForAxis(0, 0, -1, 1);
            }

        }

        /// <summary>
        /// Called by the layout system
        /// Also see ILayoutElement
        /// </summary>
        public override void SetLayoutHorizontal()
        {
            SetCellsAlongAxis(0);
        }

        /// <summary>
        /// Called by the layout system
        /// Also see ILayoutElement
        /// </summary>
        public override void SetLayoutVertical()
        {
            SetCellsAlongAxis(1);
        }
        public Vector2 getCellSize()
        {
            if (m_Constraint == Constraint.FixedColumnCount)
            {
                float width = rectTransform.rect.width;
                int cellCountX = m_ConstraintCount;
                float cellSizeX = (width - padding.horizontal + spacing.x) / cellCountX - spacing.x;
                return new Vector2(cellSizeX, cellSizeValue);
            }
            else
            {
                float height = rectTransform.rect.height;
                int cellCountY = m_ConstraintCount;
                float cellSizeY = (height - padding.vertical + spacing.y) / cellCountY - spacing.y;
                return new Vector2(cellSizeValue, cellSizeY);
            }
        }
        #endregion

        #region 私有方法

        protected FixedGridLayoutGroup()
        { }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            constraintCount = constraintCount;
        }

#endif
        private void SetCellsAlongAxis(int axis)
        {
            // Normally a Layout Controller should only set horizontal values when invoked for the horizontal axis
            // and only vertical values when invoked for the vertical axis.
            // However, in this case we set both the horizontal and vertical position when invoked for the vertical axis.
            // Since we only set the horizontal position and not the size, it shouldn't affect children's layout,
            // and thus shouldn't break the rule that all horizontal layout must be calculated before all vertical layout.
            var cellSize = getCellSize();
            var rectChildrenCount = rectChildren.Count;
            if (axis == 0)
            {
                // Only set the sizes when invoked for horizontal axis, not the positions.

                for (int i = 0; i < rectChildrenCount; i++)
                {
                    RectTransform rect = rectChildren[i];

                    m_Tracker.Add(this, rect,
                        DrivenTransformProperties.Anchors |
                        DrivenTransformProperties.AnchoredPosition |
                        DrivenTransformProperties.SizeDelta);

                    rect.anchorMin = Vector2.up;
                    rect.anchorMax = Vector2.up;
                    rect.sizeDelta = cellSize;
                }
                return;
            }

            float width = rectTransform.rect.size.x;
            float height = rectTransform.rect.size.y;

            int cellCountX = 1;
            int cellCountY = 1;
            if (m_Constraint == Constraint.FixedColumnCount)
            {
                cellCountX = m_ConstraintCount;

                if (rectChildrenCount > cellCountX)
                    cellCountY = rectChildrenCount / cellCountX + (rectChildrenCount % cellCountX > 0 ? 1 : 0);
            }
            else if (m_Constraint == Constraint.FixedRowCount)
            {
                cellCountY = m_ConstraintCount;

                if (rectChildrenCount > cellCountY)
                    cellCountX = rectChildrenCount / cellCountY + (rectChildrenCount % cellCountY > 0 ? 1 : 0);
            }
            else
            {
                if (cellSize.x + spacing.x <= 0)
                    cellCountX = int.MaxValue;
                else
                    cellCountX = Mathf.Max(1, Mathf.FloorToInt((width - padding.horizontal + spacing.x + 0.001f) / (cellSize.x + spacing.x)));

                if (cellSize.y + spacing.y <= 0)
                    cellCountY = int.MaxValue;
                else
                    cellCountY = Mathf.Max(1, Mathf.FloorToInt((height - padding.vertical + spacing.y + 0.001f) / (cellSize.y + spacing.y)));
            }

            int cornerX = (int)startCorner % 2;
            int cornerY = (int)startCorner / 2;

            int cellsPerMainAxis, actualCellCountX, actualCellCountY;
            if (startAxis == Axis.Horizontal)
            {
                cellsPerMainAxis = cellCountX;
                actualCellCountX = Mathf.Clamp(cellCountX, 1, rectChildrenCount);

                if (m_Constraint == Constraint.FixedRowCount)
                    actualCellCountY = Mathf.Min(cellCountY, rectChildrenCount);
                else
                    actualCellCountY = Mathf.Clamp(cellCountY, 1, Mathf.CeilToInt(rectChildrenCount / (float)cellsPerMainAxis));
            }
            else
            {
                cellsPerMainAxis = cellCountY;
                actualCellCountY = Mathf.Clamp(cellCountY, 1, rectChildrenCount);

                if (m_Constraint == Constraint.FixedColumnCount)
                    actualCellCountX = Mathf.Min(cellCountX, rectChildrenCount);
                else
                    actualCellCountX = Mathf.Clamp(cellCountX, 1, Mathf.CeilToInt(rectChildrenCount / (float)cellsPerMainAxis));
            }

            Vector2 requiredSpace = new Vector2(
                actualCellCountX * cellSize.x + (actualCellCountX - 1) * spacing.x,
                actualCellCountY * cellSize.y + (actualCellCountY - 1) * spacing.y
            );
            Vector2 startOffset = new Vector2(
                GetStartOffset(0, requiredSpace.x),
                GetStartOffset(1, requiredSpace.y)
            );

            // Fixes case 1345471 - Makes sure the constraint column / row amount is always respected
            int childrenToMove = 0;
            if (rectChildrenCount > m_ConstraintCount && Mathf.CeilToInt((float)rectChildrenCount / (float)cellsPerMainAxis) < m_ConstraintCount)
            {
                childrenToMove = m_ConstraintCount - Mathf.CeilToInt((float)rectChildrenCount / (float)cellsPerMainAxis);
                childrenToMove += Mathf.FloorToInt((float)childrenToMove / ((float)cellsPerMainAxis - 1));
                if (rectChildrenCount % cellsPerMainAxis == 1)
                    childrenToMove += 1;
            }

            for (int i = 0; i < rectChildrenCount; i++)
            {
                int positionX;
                int positionY;
                if (startAxis == Axis.Horizontal)
                {
                    if (m_Constraint == Constraint.FixedRowCount && rectChildrenCount - i <= childrenToMove)
                    {
                        positionX = 0;
                        positionY = m_ConstraintCount - (rectChildrenCount - i);
                    }
                    else
                    {
                        positionX = i % cellsPerMainAxis;
                        positionY = i / cellsPerMainAxis;
                    }
                }
                else
                {
                    if (m_Constraint == Constraint.FixedColumnCount && rectChildrenCount - i <= childrenToMove)
                    {
                        positionX = m_ConstraintCount - (rectChildrenCount - i);
                        positionY = 0;
                    }
                    else
                    {
                        positionX = i / cellsPerMainAxis;
                        positionY = i % cellsPerMainAxis;
                    }
                }

                if (cornerX == 1)
                    positionX = actualCellCountX - 1 - positionX;
                if (cornerY == 1)
                    positionY = actualCellCountY - 1 - positionY;

                SetChildAlongAxis(rectChildren[i], 0, startOffset.x + (cellSize[0] + spacing[0]) * positionX, cellSize[0]);
                SetChildAlongAxis(rectChildren[i], 1, startOffset.y + (cellSize[1] + spacing[1]) * positionY, cellSize[1]);
            }
        }

        #endregion

        #region 属性字段

        /// <summary>
        /// Which corner should the first cell be placed in?
        /// </summary>
        public Corner startCorner { get { return m_StartCorner; } set { SetProperty(ref m_StartCorner, value); } }
        /// <summary>
        /// Which axis should cells be placed along first
        /// </summary>
        /// <remarks>
        /// When startAxis is set to horizontal, an entire row will be filled out before proceeding to the next row. When set to vertical, an entire column will be filled out before proceeding to the next column.
        /// </remarks>
        public Axis startAxis { get { return m_StartAxis; } set { SetProperty(ref m_StartAxis, value); } }

        /// <summary>
        /// The spacing to use between layout elements in the grid on both axises.
        /// </summary>
        public Vector2 spacing { get { return m_Spacing; } set { SetProperty(ref m_Spacing, value); } }
        /// <summary>
        /// Which constraint to use for the GridLayoutGroup.
        /// </summary>
        /// <remarks>
        /// Specifying a constraint can make the GridLayoutGroup work better in conjunction with a [[ContentSizeFitter]] component. When GridLayoutGroup is used on a RectTransform with a manually specified size, there's no need to specify a constraint.
        /// </remarks>
        public Constraint constraint { get { return m_Constraint; } set { SetProperty(ref m_Constraint, value); } }
        /// <summary>
        /// How many cells there should be along the constrained axis.
        /// </summary>
        public int constraintCount { get { return m_ConstraintCount; } set { SetProperty(ref m_ConstraintCount, Mathf.Max(1, value)); } }
        public float cellSizeValue { get { return m_CellSizeValue; } set { SetProperty(ref m_CellSizeValue, value); } }
        [SerializeField]
        protected Corner m_StartCorner = Corner.UpperLeft;
        [SerializeField]
        protected Axis m_StartAxis = Axis.Horizontal;
        [SerializeField]
        protected Vector2 m_CellSize = new Vector2(100, 100);
        [SerializeField]
        protected Vector2 m_Spacing = Vector2.zero;
        [SerializeField]
        protected Constraint m_Constraint = Constraint.FixedColumnCount;
        [SerializeField]
        protected int m_ConstraintCount = 2;
        [SerializeField]
        protected float m_CellSizeValue = 100;
        #endregion

        #region 内嵌类
        /// <summary>
        /// Which corner is the starting corner for the grid.
        /// </summary>
        public enum Corner
        {
            /// <summary>
            /// Upper Left corner.
            /// </summary>
            UpperLeft = 0,
            /// <summary>
            /// Upper Right corner.
            /// </summary>
            UpperRight = 1,
            /// <summary>
            /// Lower Left corner.
            /// </summary>
            LowerLeft = 2,
            /// <summary>
            /// Lower Right corner.
            /// </summary>
            LowerRight = 3
        }

        /// <summary>
        /// The grid axis we are looking at.
        /// </summary>
        /// <remarks>
        /// As the storage is a [][] we make access easier by passing a axis.
        /// </remarks>
        public enum Axis
        {
            /// <summary>
            /// Horizontal axis
            /// </summary>
            Horizontal = 0,
            /// <summary>
            /// Vertical axis.
            /// </summary>
            Vertical = 1
        }

        /// <summary>
        /// Constraint type on either the number of columns or rows.
        /// </summary>
        public enum Constraint
        {
            /// <summary>
            /// Constrain the number of columns to a specified number.
            /// </summary>
            FixedColumnCount = 0,
            /// <summary>
            /// Constraint the number of rows to a specified number.
            /// </summary>
            FixedRowCount = 1
        }
        #endregion
    }
}
