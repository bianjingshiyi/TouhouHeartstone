using System;

namespace UI
{
    partial class ActionNodeGraphPanel
    {
        #region 公有方法
        public ActionNodeItem createNodeForBuff(float posX, float posY, ActionNodeBuffViewData viewData, string[] outputNames)
        {
            ActionNodeItem node = createNode(posX, posY);
            node.onBuffChanged += onItemBuffChangedCallback;
            node.onClickEditBuffButton += onClickItemEditBuffButtonCallback;
            node.switchToBuffNode();
            updateBuffActionNode(node, viewData);
            node.setOutputs(outputNames);
            return node;
        }
        public void updateBuffActionNode(ActionNodeItem node, ActionNodeBuffViewData viewData)
        {
            node.setBuffValues(viewData.buffNames);
            node.setBuffValue(viewData.buffIndex);
        }
        #endregion
        #region 私有方法
        private void onItemBuffChangedCallback(ActionNodeItem item, int index)
        {
            onItemBuffChanged?.Invoke(item, index);
        }
        private void onClickItemEditBuffButtonCallback(ActionNodeItem item)
        {
            onClickItemEditBuffButton?.Invoke(item);
        }
        #endregion
        #region 事件
        public event Action<ActionNodeItem, int> onItemBuffChanged;
        public event Action<ActionNodeItem> onClickItemEditBuffButton;
        #endregion
    }
    public class ActionNodeBuffViewData
    {
        public string[] buffNames;
        public int buffIndex;
    }
    public class ActionNodePropertyModifierViewData
    {
        public int propertyTypeIndex;
        public string[] modifyTypes;
        public int modifyTypeIndex;
        public ActionNodeValueType valueType;
        public object value;
    }
}