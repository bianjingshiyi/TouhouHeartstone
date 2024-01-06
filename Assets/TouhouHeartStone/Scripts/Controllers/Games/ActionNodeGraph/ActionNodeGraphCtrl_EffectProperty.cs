using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using UnityEngine;

namespace Controllers
{
    partial class ActionNodeGraphCtrl
    {
        public void addEffectProperty(PropertyItemInfo item)
        {
            createEffectProperty(item.propertyName, item.type, item.value);
            updateEffectProperties();
            addRecordBreakpoint();
        }
        /// <summary>
        /// 属性名是否已经被使用。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool isPropertyNameTaken(string name)
        {
            if (_effect.hasProp(name) && _effect.getProp(name) != null)
                return true;
            return false;
        }
        public void setEffectProp(string name, object value)
        {
            var beforeValue = _effect.getProp(name);
            _effect.setProp(name, value);

            // 复制一份之前的值和新的值，存储在记录中，用以随时回退。
            var recordBeforeValue = beforeValue;
            var recordValue = value;
            if (beforeValue is ICloneable beforeCloneable)
            {
                recordBeforeValue = beforeCloneable.Clone();
            }
            if (value is ICloneable valueCloneable)
            {
                recordValue = valueCloneable.Clone();
            }
            addOperationRecord(new ChangeEffectPropertyValueOperation(this, name, recordBeforeValue, recordValue));
            updatePropertyItem(name);
        }
        /// <summary>
        /// 更新一项属性。
        /// </summary>
        /// <param name="name">属性名称。</param>
        public async void updatePropertyItem(string name)
        {
            var itemInfo = GetEffectPropertyItem(name);
            var nodeItem = getEffectEntryNodeItem();
            if (itemInfo != null)
            {
                var field = nodeItem.getEffectPropertyItemField(name);
                if (!field)
                    return;
                field.updateItem(itemInfo);
                await field.valueInputField.setInputFieldCardByValue(itemInfo.value, getCardPoolEnv(), game);
            }
        }
        #region 事件回调
        private void onClickAddEffectPropertyButtonCallback()
        {
            if (isReadonly)
                return;
            onAddEffectPropertyButtonClicked?.Invoke();
        }
        private void onClickEffectPropertyItemButtonCallback(string name, int index, ValueInputFieldButton button)
        {
            switch (button)
            {
                case ValueInputFieldButton.CardButton:
                    onClickEffectPropertyItemCardButtonCallback(name, index);
                    break;
                case ValueInputFieldButton.CardEditButton:
                    onClickEffectPropertyItemCardEditButtonCallback(name, index);
                    break;
                case ValueInputFieldButton.BuffEditButton:
                    onClickEffectPropertyItemBuffEditButtonCallback(name, index);
                    break;
            }
        }
        private async void onClickEffectPropertyItemCardButtonCallback(string name, int index)
        {
            if (isReadonly)
                return;
            var prop = _effect.getProp<DefineReference[]>(name);
            if (prop == null)
                return;
            //打开卡牌选择界面
            long poolId = _curCardPoolId;
            var define = prop?[index];
            if (define != null && define.defineId != 0)
            {
                poolId = define.cardPoolId;
            }
            await displayCardSelectPanel(poolId);
            _curCardSeekingEffectPropName = name;
            _curCardSeekingEffectPropIndex = index;
        }
        private void onClickEffectPropertyItemCardEditButtonCallback(string name, int index)
        {
            onOpenEffectPropertyItemCardDefine?.Invoke(_effect, name, index);
        }
        private void onEffectPropertyValueChangedCallback(string name, object value)
        {
            if (isReadonly)
                return;
            // 从UI传递到的效果属性要复制一份，免得UI的数据修改时，效果的属性也会同时更改。
            if (value is ICloneable valueCloneable)
            {
                value = valueCloneable.Clone();
            }
            setEffectProp(name, value);
            addRecordBreakpoint();
        }
        private void onClickEffectPropertyButtonCallback(string name, ValueInputFieldButton button)
        {
            switch (button)
            {
                case ValueInputFieldButton.CardButton:
                    onClickEffectPropertyCardButtonCallback(name);
                    break;
                case ValueInputFieldButton.CardEditButton:
                    onClickEffectPropertyCardEditButtonCallback(name);
                    break;
                case ValueInputFieldButton.BuffEditButton:
                    onClickEffectPropertyBuffEditButtonCallback(name);
                    break;
            }
        }
        private async void onClickEffectPropertyCardButtonCallback(string name)
        {
            if (isReadonly)
                return;
            var prop = _effect.getProp<DefineReference>(name);
            if (prop == null)
                return;
            //打开卡牌选择界面
            long poolId = _curCardPoolId;
            if (prop.defineId != 0)
            {
                poolId = prop.cardPoolId;
            }
            await displayCardSelectPanel(poolId);
            _curCardSeekingEffectPropName = name;
            _curCardSeekingEffectPropIndex = -1;
        }
        private void onClickEffectPropertyCardEditButtonCallback(string name)
        {
            onOpenEffectPropertyCardDefine?.Invoke(_effect, name);
        }
        private async void onClickEffectPropertyBuffEditButtonCallback(string name)
        {
            var buffRefObj = _effect.getProp(name);
            if (tryGetBuffDefine(buffRefObj, out GeneratedBuffDefine buffDefine))
            {
                await scene.editor.openBuffEditor(buffDefine);
            }
        }
        private async void onClickEffectPropertyItemBuffEditButtonCallback(string name, int index)
        {
            var prop = _effect.getProp<IEnumerable<DefineReference>>(name);
            if (prop == null)
                return;
            var buffRef = prop.ElementAt(index);
            if (tryGetBuffDefine(buffRef, out GeneratedBuffDefine buffDefine))
            {
                await scene.editor.openBuffEditor(buffDefine);
            }
        }
        private void onDeleteEffectPropertyCallback(string name)
        {
            if (isReadonly)
                return;
            var item = GetEffectPropertyItem(name);
            _effect.removeProp(name);
            addOperationRecord(new RemoveEffectPropertyOperation(this, item));

            updateEffectProperties();
            addRecordBreakpoint();
        }
        #endregion

        #region 私有方法
        private ActionNodeItem getEffectEntryNodeItem()
        {
            Node entryNode = null;
            if (_effect is GeneratedActiveEffect active)
            {
                entryNode = active.entryNode;
            }
            else if (_effect is GeneratedAura aura)
            {
                entryNode = aura.entryNode;
            }
            else if (_effect is GeneratedEventEffect eventEff)
            {
                entryNode = eventEff.entryNode;
            }
            else if (_effect is GeneratedPassiveEffect passive)
            {
                entryNode = passive.entryNode;
            }
            if (entryNode == null)
            {
                return null;
            }
            return getActionNodeItem(entryNode);
        }
        /// <summary>
        /// 更新所有效果属性。
        /// </summary>
        private async void updateEffectProperties()
        {
            var itemInfos = GetEffectPropItems();
            var nodeItem = getEffectEntryNodeItem();
            if (nodeItem != null)
            {
                nodeItem.updateEffectProperties(itemInfos);
                var fields = nodeItem.getEffectPropertyItemFields();
                var env = getCardPoolEnv();
                for (int i = 0; i < itemInfos.Length; i++)
                {
                    var field = fields[i];
                    var itemInfo = itemInfos[i];
                    if (!field)
                        return;
                    await field.valueInputField.setInputFieldCardByValue(itemInfo.value, env, game);
                }
            }
        }
        private PropertyItemInfo[] GetEffectPropItems()
        {
            var names = _effect.getPropNames();
            List<PropertyItemInfo> items = new List<PropertyItemInfo>();
            foreach (string name in names)
            {
                var item = GetEffectPropertyItem(name);
                if (item == null)
                    continue;
                items.Add(item);
            }
            return items.ToArray();
        }
        private PropertyItemInfo GetEffectPropertyItem(string name)
        {
            var value = _effect.getProp(name);
            if (value == null)
                return null;

            if (value is ICloneable cloneable)
            {
                value = cloneable.Clone();
            }
            var valueType = value.GetType();
            if (!valueType.tryGetValueTypeByType(out var type) || !CardEditorCtrl.isTypeCustomProp(type.type))
                return null;

            var item = new PropertyItemInfo(name, type)
            {
                value = value
            };
            return item;
        }
        private void createEffectProperty(string name, NodeValueType type, object value)
        {
            if (value == null)
            {
                value = type.getDefaultValue();
            }
            _effect.setProp(name, value);
            addOperationRecord(new AddEffectPropertyOperation(this, name, type, value));
        }
        private void selectCardForEffectProperty(CardDefine cardDefine)
        {
            object value;
            var defRef = cardDefine.getDefineRef();
            if (_curCardSeekingEffectPropIndex >= 0)
            {
                var array = _effect.getProp<DefineReference[]>(_curCardSeekingEffectPropName);
                if (array == null || _curCardSeekingEffectPropIndex >= array.Length)
                {
                    var newArray = new DefineReference[_curCardSeekingEffectPropIndex + 1];
                    array.CopyTo(newArray, 0);
                    array = newArray;
                }
                array[_curCardSeekingEffectPropIndex] = defRef;
                value = array.Clone();
            }
            else
            {
                value = defRef;
            }
            setEffectProp(_curCardSeekingEffectPropName, value);
            addRecordBreakpoint();
        }
        #endregion

        #region 内嵌类
        class AddEffectPropertyOperation : GraphOperation
        {
            public AddEffectPropertyOperation(ActionNodeGraphCtrl graphCtrl, PropertyItemInfo propItem) :
                this(graphCtrl, propItem.propertyName, propItem.type, propItem.value)
            {
            }
            public AddEffectPropertyOperation(ActionNodeGraphCtrl graphCtrl, string propertyName, NodeValueType type, object value) : base(graphCtrl)
            {
                _propertyName = propertyName;
                _type = type;
                _value = value;
            }
            public override void undo()
            {
                _graphCtrl._effect.removeProp(_propertyName);
                _graphCtrl.updateEffectProperties();
            }
            public override void redo()
            {
                _graphCtrl.createEffectProperty(_propertyName, _type, _value);
                _graphCtrl.updateEffectProperties();
            }
            private string _propertyName;
            private NodeValueType _type;
            private object _value;
        }
        class RemoveEffectPropertyOperation : GraphOperation
        {
            public RemoveEffectPropertyOperation(ActionNodeGraphCtrl graphCtrl, PropertyItemInfo propItem) : base(graphCtrl)
            {
                _propertyName = propItem.propertyName;
                _type = propItem.type;
                _value = propItem.value;
            }
            public override void undo()
            {
                _graphCtrl.createEffectProperty(_propertyName, _type, _value);
                _graphCtrl.updateEffectProperties();
            }
            public override void redo()
            {
                _graphCtrl._effect.removeProp(_propertyName);
                _graphCtrl.updateEffectProperties();
            }
            private string _propertyName;
            private NodeValueType _type;
            private object _value;
        }
        class ChangeEffectPropertyValueOperation : GraphOperation
        {
            public ChangeEffectPropertyValueOperation(ActionNodeGraphCtrl graphCtrl, string name, object beforeValue, object afterValue) : base(graphCtrl)
            {
                _name = name;
                _beforeValue = beforeValue;
                _afterValue = afterValue;
            }
            private void setValue(object value)
            {
                if (value is ICloneable cloneable)
                {
                    value = cloneable.Clone();
                }
                _graphCtrl._effect.setProp(_name, value);
                _graphCtrl.updatePropertyItem(_name);
            }
            public override void undo()
            {
                setValue(_beforeValue);
            }
            public override void redo()
            {
                setValue(_afterValue);
            }
            private string _name;
            private object _beforeValue;
            private object _afterValue;
        }
        #endregion

        #region 事件
        public event Action onAddEffectPropertyButtonClicked;
        public event Action<GeneratedEffect, string> onOpenEffectPropertyCardDefine;
        public event Action<GeneratedEffect, string, int> onOpenEffectPropertyItemCardDefine;
        #endregion

        #region 属性字段
        /// <summary>
        /// 当前正在选择的卡牌引用属性的属性名称。
        /// </summary>
        string _curCardSeekingEffectPropName;
        /// <summary>
        /// 当前正在选择的卡牌引用属性的列表索引。
        /// </summary>
        int _curCardSeekingEffectPropIndex;
        #endregion
    }
}
