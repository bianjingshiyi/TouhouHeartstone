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
        #region 事件回调
        private void onClickAddEffectPropertyButtonCallback()
        {
            _ui.displayEffectPropertyDialog(true);
        }
        private void onConfirmCreateEffectPropertyCallback(EffectPropertyItem item)
        {
            if (string.IsNullOrEmpty(item.propertyName))
            {
                string errorMessage = game.lang._(NODE_EFFECT_PROP_NAME_EMPTY);
                _ui.showEffectPropertyDialogError(errorMessage);
                return;
            }
            if (isPropertyNameTaken(item.propertyName))
            {
                string errorMessage = game.lang._(NODE_EFFECT_PROP_NAME_TAKEN);
                _ui.showEffectPropertyDialogError(errorMessage);
                return;
            }

            createEffectProperty(item.propertyName, item.type, item.value, item.isArray);

            addOperationRecord(new AddEffectPropertyOperation(this, item));
            addRecordBreakpoint();

            OnEdited();
            updateEffectProperties();
            _ui.displayEffectPropertyDialog(false);
        }
        private void onEffectPropertyItemValueChangedCallback(string name, int index, object value)
        {
            var prop = _effect.getProp(name);
            if (prop is Array array)
            {
                var beforeValue = array.Clone();
                array.SetValue(value, index);

                addOperationRecord(new ChangeEffectPropertyValueOperation(this, name, beforeValue, array.Clone()));
                addRecordBreakpoint();

                OnEdited();
                updatePropertyItem(name);
            }
        }
        private async void onClickEffectPropertyItemButtonCallback(string name, int index)
        {
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
            await _cardPoolViewPanelCtrl.display(cardPoolFiltersId: new long[] { poolId }, cardFilter: card => true);
            _ui.displayCardSelectPanel(true);
            _curCardSeekingEffectPropName = name;
            _curCardSeekingEffectPropIndex = index;
        }
        private void onClickEffectPropertyAddItemButtonCallback(string name)
        {
            var array = _effect.getProp<Array>(name);
            if (array == null)
                return;
            var beforeValue = array.Clone();
            var arrayType = array.GetType();
            var elementType = arrayType.GetElementType();
            if (elementType == null)
                return;
            var newArray = Array.CreateInstance(elementType, array.Length + 1);
            array.CopyTo(newArray, 0);


            if (_effectPropTypeDict.TryGetValue(elementType, out var key))
            {
                newArray.SetValue(getDefaultValue(key, false), newArray.Length - 1);
            }
            _effect.setProp(name, newArray);

            addOperationRecord(new ChangeEffectPropertyValueOperation(this, name, beforeValue, newArray.Clone()));
            addRecordBreakpoint();

            OnEdited();
            updatePropertyItem(name);
        }

        private void onClickEffectPropertyRemoveItemButtonCallback(string name, int index)
        {
            var array = _effect.getProp<Array>(name);
            if (array == null)
                return;
            var beforeValue = array.Clone();
            var arrayType = array.GetType();
            var elementType = arrayType.GetElementType();
            if (elementType == null)
                return;
            var newArray = Array.CreateInstance(elementType, array.Length - 1);
            int newIndex = 0;
            for (int i = 0; i < array.Length; i++)
            {
                if (i == index)
                    continue;
                newArray.SetValue(array.GetValue(i), newIndex);
                newIndex++;
            }
            _effect.setProp(name, newArray);

            addOperationRecord(new ChangeEffectPropertyValueOperation(this, name, beforeValue, newArray.Clone()));
            addRecordBreakpoint();

            OnEdited();
            updatePropertyItem(name);
        }
        private void onEffectPropertyValueChangedCallback(string name, object value)
        {
            var beforeValue = _effect.getProp(name);
            if (beforeValue is ICloneable beforeCloneable)
            {
                beforeValue = beforeCloneable.Clone();
            }
            if (value is ICloneable cloneable)
            {
                value = cloneable.Clone();
            }
            _effect.setProp(name, value);

            if (beforeValue != value)
            {
                addOperationRecord(new ChangeEffectPropertyValueOperation(this, name, beforeValue, value));
                addRecordBreakpoint();
            }

            OnEdited();
            updatePropertyItem(name);
        }
        private async void onClickEffectPropertyButtonCallback(string name)
        {
            var prop = _effect.getProp<DefineReference>(name);
            if (prop == null)
                return;
            //打开卡牌选择界面
            long poolId = _curCardPoolId;
            if (prop.defineId != 0)
            {
                poolId = prop.cardPoolId;
            }
            await _cardPoolViewPanelCtrl.display(cardPoolFiltersId: new long[] { poolId }, cardFilter: card => true);
            _ui.displayCardSelectPanel(true);
            _curCardSeekingEffectPropName = name;
            _curCardSeekingEffectPropIndex = -1;
        }

        private void onDeleteEffectPropertyCallback(string name)
        {
            var value = GetEffectPropertyItem(name);
            _effect.removeProp(name);

            addOperationRecord(new RemoveEffectPropertyOperation(this, value));
            addRecordBreakpoint();

            OnEdited();
            updateEffectProperties();
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
        /// <summary>
        /// 更新一项属性。
        /// </summary>
        /// <param name="name">属性名称。</param>
        private void updatePropertyItem(string name)
        {
            var item = GetEffectPropertyItem(name);
            var nodeItem = getEffectEntryNodeItem();
            if (item != null)
            {
                nodeItem.updateEffectPropertyItem(name, item, game.card);
            }
        }
        private EffectPropertyItem[] GetEffectPropItems()
        {
            var names = _effect.getPropNames();
            List<EffectPropertyItem> items = new List<EffectPropertyItem>();
            foreach (string name in names)
            {
                var item = GetEffectPropertyItem(name);
                if (item == null)
                    continue;
                items.Add(item);
            }
            return items.ToArray();
        }
        private EffectPropertyItem GetEffectPropertyItem(string name)
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

            var item = new EffectPropertyItem(name, type, isArray)
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
        /// <summary>
        /// 属性名是否已经被使用。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private bool isPropertyNameTaken(string name)
        {
            if (_effect.hasProp(name) && _effect.getProp(name) != null)
                return true;
            return false;
        }
        private void createEffectProperty(string name, ActionNodeValueType type, object value, bool isArray)
        {
            if (value == null)
            {
                value = getDefaultValue(type, isArray);
            }
            _effect.setProp(name, value);
        }
        private void selectCardForEffectProperty(CardDefine cardDefine)
        {
            object beforeValue;
            object value;
            if (_curCardSeekingEffectPropIndex >= 0)
            {
                var array = _effect.getProp<DefineReference[]>(_curCardSeekingEffectPropName);
                var defRef = cardDefine.getDefineRef();
                if (array == null || _curCardSeekingEffectPropIndex >= array.Length)
                {
                    var newArray = new DefineReference[_curCardSeekingEffectPropIndex + 1];
                    array.CopyTo(newArray, 0);
                    array = newArray;
                    _effect.setProp(_curCardSeekingEffectPropName, array);
                }
                beforeValue = array.Clone();
                array[_curCardSeekingEffectPropIndex] = defRef;
                value = array.Clone();
            }
            else
            {
                beforeValue = _effect.getProp(_curCardSeekingEffectPropName);
                var defRef = cardDefine.getDefineRef();
                _effect.setProp(_curCardSeekingEffectPropName, defRef);
                value = defRef;
            }
            if (beforeValue != value)
            {
                addOperationRecord(new ChangeEffectPropertyValueOperation(this, _curCardSeekingEffectPropName, beforeValue, value));
                addRecordBreakpoint();
            }
            updatePropertyItem(_curCardSeekingEffectPropName);
            OnEdited();
        }
        #endregion

        #region 内嵌类
        class AddEffectPropertyOperation : GraphOperation
        {
            public AddEffectPropertyOperation(ActionNodeGraphCtrl graphCtrl, EffectPropertyItem propItem) : base(graphCtrl)
            {
                _propertyName = propItem.propertyName;
                _type = propItem.type;
                _value = propItem.value;
                _isArray = propItem.isArray;
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
            public RemoveEffectPropertyOperation(ActionNodeGraphCtrl graphCtrl, EffectPropertyItem propItem) : base(graphCtrl)
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

        #region 属性字段
        [TranslateMsg("尝试添加重复名称的自定义属性的错误提示")]
        public const string NODE_EFFECT_PROP_NAME_TAKEN = "该属性名已被使用！";
        [TranslateMsg("尝试添加空名称的自定义属性的错误提示")]
        public const string NODE_EFFECT_PROP_NAME_EMPTY = "请输入属性名！";
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
