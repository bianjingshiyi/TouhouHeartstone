using System;
using System.Collections.Generic;
using MukioI18n;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;

namespace Controllers
{
    partial class ActionNodeGraphCtrl
    {
        public void addEffectProperty(PropertyItemInfo item)
        {
            createEffectProperty(item.propertyName, item.type, item.value, item.isArray);
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
        public void updatePropertyItem(string name)
        {
            var item = GetEffectPropertyItem(name);
            var nodeItem = getEffectEntryNodeItem();
            if (item != null)
            {
                nodeItem.updateEffectPropertyItem(name, item, game.card);
            }
        }
        #region 事件回调
        private void onClickAddEffectPropertyButtonCallback()
        {
            if (isReadonly)
                return;
            onAddEffectPropertyButtonClicked?.Invoke();
        }
        private void onEffectPropertyItemValueChangedCallback(string name, int index, object value)
        {
            if (isReadonly)
                return;
            var prop = _effect.getProp(name);
            if (prop is object[] array)
            {
                var list = new List<object>(array);
                list[index] = value;
                setEffectProp(name, list.ToArray());
                addRecordBreakpoint();
            }
        }
        private void onClickEffectPropertyItemButtonCallback(string name, int index)
        {
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
            await displayCardSelectPanel(poolId );
            _curCardSeekingEffectPropName = name;
            _curCardSeekingEffectPropIndex = index;
        }
        private void onClickEffectPropertyItemCardEditButtonCallback(string name, int index)
        {
            onOpenEffectPropertyItemCardDefine?.Invoke(_effect, name, index);
        }
        private void onClickEffectPropertyAddItemButtonCallback(string name)
        {
            if (isReadonly)
                return;
            var array = _effect.getProp<object[]>(name);
            if (array == null)
                return;
            var propItem = GetEffectPropertyItem(name);
            var list = new List<object>(array)
            {
                getDefaultValue(propItem.type, false)
            };
            setEffectProp(name, list.ToArray());
            addRecordBreakpoint();
        }

        private void onClickEffectPropertyRemoveItemButtonCallback(string name, int index)
        {
            if (isReadonly)
                return;
            var array = _effect.getProp<object[]>(name);
            if (array == null)
                return;
            var list = new List<object>(array);
            list.RemoveAt(index);
            setEffectProp(name, list.ToArray());
            addRecordBreakpoint();
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
        private void onClickEffectPropertyButtonCallback(string name)
        {
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
        private void updateEffectProperties()
        {
            var items = GetEffectPropItems();
            var item = getEffectEntryNodeItem();
            if (item != null)
            {
                item.updateEffectProperties(items, game.card);
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
            bool isArray = valueType.IsArray;
            if (isArray)
            {
                valueType = valueType.GetElementType();
            }
            // 防止出现什么Array类型的参数，GetElementType会返回null。
            if (valueType == null)
                return null;

            if (!_effectPropTypeDict.TryGetValue(valueType, out ActionNodeValueType type))
                return null;

            var item = new PropertyItemInfo(name, type, isArray)
            {
                value = value
            };
            return item;
        }
        private object getDefaultValue(ActionNodeValueType valueType, bool isArray)
        {
            if (!_effectPropDefaultValues.TryGetValue(valueType, out var func))
                return null;
            var value = func();
            if (!isArray)
            {
                // 不是数组
                return value;
            }

            // 数组
            Type type = null;
            foreach (var pair in _effectPropTypeDict)
            {
                if (pair.Value == valueType)
                {
                    type = pair.Key;
                    break;
                }
            }
            if (type == null)
                return null;
            var array = Array.CreateInstance(type, 1);
            array.SetValue(value, 0);
            return array;
        }
        private void createEffectProperty(string name, ActionNodeValueType type, object value, bool isArray)
        {
            if (value == null)
            {
                value = getDefaultValue(type, isArray);
            }
            _effect.setProp(name, value);
            addOperationRecord(new AddEffectPropertyOperation(this, name, type, value, isArray));
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
                this(graphCtrl, propItem.propertyName, propItem.type, propItem.value, propItem.isArray)
            {
            }
            public AddEffectPropertyOperation(ActionNodeGraphCtrl graphCtrl, string propertyName, ActionNodeValueType type, object value, bool isArray) : base(graphCtrl)
            {
                _propertyName = propertyName;
                _type = type;
                _value = value;
                _isArray = isArray;
            }
            public override void undo()
            {
                _graphCtrl._effect.removeProp(_propertyName);
                _graphCtrl.updateEffectProperties();
            }
            public override void redo()
            {
                _graphCtrl.createEffectProperty(_propertyName, _type, _value, _isArray);
                _graphCtrl.updateEffectProperties();
            }
            private string _propertyName;
            private ActionNodeValueType _type;
            private object _value;
            private bool _isArray;
        }
        class RemoveEffectPropertyOperation : GraphOperation
        {
            public RemoveEffectPropertyOperation(ActionNodeGraphCtrl graphCtrl, PropertyItemInfo propItem) : base(graphCtrl)
            {
                _propertyName = propItem.propertyName;
                _type = propItem.type;
                _value = propItem.value;
                _isArray = propItem.isArray;
            }
            public override void undo()
            {
                _graphCtrl.createEffectProperty(_propertyName, _type, _value, _isArray);
                _graphCtrl.updateEffectProperties();
            }
            public override void redo()
            {
                _graphCtrl._effect.removeProp(_propertyName);
                _graphCtrl.updateEffectProperties();
            }
            private string _propertyName;
            private ActionNodeValueType _type;
            private object _value;
            private bool _isArray;
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

        static Dictionary<Type, ActionNodeValueType> _effectPropTypeDict => CardEditorCtrl.customPropTypeDict;
        static Dictionary<ActionNodeValueType, Func<object>> _effectPropDefaultValues => CardEditorCtrl.customPropDefaultValues;
        #endregion
    }
}
