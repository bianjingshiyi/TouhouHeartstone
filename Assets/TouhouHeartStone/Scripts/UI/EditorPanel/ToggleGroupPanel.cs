using System;
namespace UI
{
    /// <summary>
    /// 包含若干组选框的窗口，点击窗口以外的区域关闭并触发事件传递选框的值
    /// </summary>
    public partial class ToggleGroupPanel
    {
        /// <summary>
        /// 显示窗口
        /// </summary>
        /// <param name="names">选框名称的数组</param>
        /// <param name="values">选框的值</param>
        public void display(string[] names, bool[] values)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 当点击选框以外的区域将其关闭，参数为当前选框的值
        /// </summary>
        public event Action<bool[]> onClosePanel;
    }
}