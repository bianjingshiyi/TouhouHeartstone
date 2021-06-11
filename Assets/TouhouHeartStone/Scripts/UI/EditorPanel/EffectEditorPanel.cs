using System;
namespace UI
{
    /// <summary>
    /// 效果编辑器UI脚本
    /// </summary>
    public partial class EffectEditorPanel
    {
        /// <summary>
        /// 显示生效范围选项组
        /// </summary>
        /// <param name="names"></param>
        /// <param name="values"></param>
        public void displayActiveRangeToggleGroup(string[] names, bool[] values)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 当点击编辑生效范围按钮
        /// </summary>
        public event Action onClickEditActiveRangeButton;
        /// <summary>
        /// 当结束生效范围编辑
        /// </summary>
        public event Action<bool[]> onCloseActiveRangeToggleGroup;
        /// <summary>
        /// 当点击编辑激活动作按钮
        /// </summary>
        public event Action onClickEditEnableActionButton;
        /// <summary>
        /// 当点击禁用激活动作按钮
        /// </summary>
        public event Action onClickEditDisableActionButton;
        /// <summary>
        /// 当点击添加触发器按钮
        /// </summary>
        public event Action onClickAddTriggerButton;
        /// <summary>
        /// 当点击移除触发器按钮，参数为移除的触发器索引。
        /// </summary>
        public event Action<int> onClickRemoveTriggerButton;
        /// <summary>
        /// 当点击编辑触发器按钮，参数为编辑的触发器索引。
        /// </summary>
        public event Action<int> onClickEditTriggerButton;
        ToggleGroupPanel _activeRangeToggleGroup;
    }
}