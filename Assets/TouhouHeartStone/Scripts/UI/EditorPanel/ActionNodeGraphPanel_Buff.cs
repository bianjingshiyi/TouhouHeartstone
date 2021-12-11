using System;

namespace UI
{
    partial class ActionNodeGraphPanel
    {
        #region 公有方法
        public ActionNodeItem createNodeForBuff(float posX, float posY, ActionNodeBuffViewData viewData, string[] outputNames)
        {
            ActionNodeItem node = createNode(posX, posY);
            node.switchToBuffNode();
            node.setBuffValues(viewData.buffNames);
            node.setBuffValue(viewData.buffIndex);
            node.setBuffId(viewData.buffId.ToString());
            node.setupPropModifierTemplate(viewData.propertyTypes, viewData.modifyTypes);
            node.updatePropModifierList(viewData.propModifiersViewData);
            node.updateEffectList(viewData.effectTypeNames, viewData.effectNames);
            node.setOutputs(outputNames);
            node.onBuffChanged += onItemBuffChangedCallback;
            node.onClickNewBuffButton += onClickItemNewBuffButtonCallback;
            node.onClickDeleteBuffButton += onClickItemDeleteBuffButtonCallback;
            node.onClickAddPropModifierButton += onClickItemAddPropModifierButtonCallback;
            node.onClickRemovePropModifierButton += onClickItemRemovePropModifierButtonCallback;
            node.onPropModifierPropertyTypeChanged += onItemPropModifierPropertyTypeChangedCallback;
            node.onPropModifierModifyTypeChanged += onItemPropModifierModifyTypeChangedCallback;
            node.onPropModifierValueChanged += onItemPropModifierValueChangedCallback;
            node.onPropModifierAddEffect += onItemPropModifierAddEffectCallback;
            node.onPropModifierRemoveEffect += onItemPropModifierRemoveEffectCallback;
            node.onPropModifierEditEffect += onItemPropModifierEditEffectCallback;
            return node;
        }
        #endregion
        #region 私有方法
        private void onItemBuffChangedCallback(ActionNodeItem item, int index)
        {
            onItemBuffChanged?.Invoke(item, index);
        }
        private void onClickItemNewBuffButtonCallback(ActionNodeItem item)
        {
            onClickItemNewBuffButton?.Invoke(item);
        }
        private void onClickItemDeleteBuffButtonCallback(ActionNodeItem item)
        {
            onClickItemDeleteBuffButton?.Invoke(item);
        }
        private void onClickItemAddPropModifierButtonCallback(ActionNodeItem item)
        {
            onClickItemAddPropModifierButton?.Invoke(item);
        }
        private void onClickItemRemovePropModifierButtonCallback(ActionNodeItem item, int index)
        {
            onClickItemRemovePropModifierButton?.Invoke(item, index);
        }
        private void onItemPropModifierPropertyTypeChangedCallback(ActionNodeItem item, int index, int propTypeIndex)
        {
            onItemPropModifierPropertyTypeChanged?.Invoke(item, index, propTypeIndex);
        }
        private void onItemPropModifierModifyTypeChangedCallback(ActionNodeItem item, int index, int modifyTypeIndex)
        {
            onItemPropModifierModifyTypeChanged?.Invoke(item, index, modifyTypeIndex);
        }
        private void onItemPropModifierValueChangedCallback(ActionNodeItem item, int index, object value)
        {
            onItemPropModifierValueChanged?.Invoke(item, index, value);
        }
        private void onItemPropModifierAddEffectCallback(ActionNodeItem item, int index)
        {
            onItemPropModifierAddEffect?.Invoke(item, index);
        }
        private void onItemPropModifierRemoveEffectCallback(ActionNodeItem item, int index)
        {
            onItemPropModifierRemoveEffect?.Invoke(item, index);
        }
        private void onItemPropModifierEditEffectCallback(ActionNodeItem item, int index)
        {
            onItemPropModifierEditEffect?.Invoke(item, index);
        }
        #endregion
        #region 事件
        public event Action<ActionNodeItem, int> onItemBuffChanged;
        public event Action<ActionNodeItem> onClickItemNewBuffButton;
        public event Action<ActionNodeItem> onClickItemDeleteBuffButton;
        public event Action<ActionNodeItem> onClickItemAddPropModifierButton;
        public event Action<ActionNodeItem, int> onClickItemRemovePropModifierButton;
        public event Action<ActionNodeItem, int, int> onItemPropModifierPropertyTypeChanged;
        public event Action<ActionNodeItem, int, int> onItemPropModifierModifyTypeChanged;
        public event Action<ActionNodeItem, int, object> onItemPropModifierValueChanged;
        public event Action<ActionNodeItem, int> onItemPropModifierAddEffect;
        public event Action<ActionNodeItem, int> onItemPropModifierRemoveEffect;
        public event Action<ActionNodeItem, int> onItemPropModifierEditEffect;
        #endregion
    }
    public class ActionNodeBuffViewData
    {
        public string[] buffNames;
        public int buffIndex;
        public int buffId;
        public string[] propertyTypes;
        public string[] modifyTypes;
        public ActionNodePropertyModifierViewData[] propModifiersViewData;
        public string[] effectTypeNames;
        public string[] effectNames;
    }
    public class ActionNodePropertyModifierViewData
    {
        public int propertyTypeIndex;
        public int modifyTypeIndex;
        public ActionNodeValueType valueType;
        public object value;
    }
}