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

            if (item.value == null)
            {
                item.value = getDefaultValue(item.type, item.isArray);
            }
            _effect.setProp(item.propertyName, item.value);

            OnEdited();
            updateEffectProperties();
            _ui.displayEffectPropertyDialog(false);
        }
        private void onEffectPropertyItemValueChangedCallback(string name, int index, object value)
        {
            var prop = _effect.getProp(name);
            if (prop is Array array)
            {
                array.SetValue(value, index);
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
            bool hideBuiltinCards = true;
            var define = prop?[index];
            if (define != null && define.defineId != 0)
            {
                poolId = define.cardPoolId;
            }
            if (game.card.isBuiltinCardPool(poolId))
            {
                hideBuiltinCards = false;
            }
            await _cardPoolViewPanelCtrl.display(new long[] { poolId }, hideBuiltinCards: hideBuiltinCards, cardFilter: card => true);
            _ui.displayCardSelectPanel(true);
            _curCardSeekingEffectPropName = name;
            _curCardSeekingEffectPropIndex = index;
        }
        private void onClickEffectPropertyAddItemButtonCallback(string name)
        {
            var array = _effect.getProp<Array>(name);
            if (array == null)
                return;
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
            OnEdited();
            updatePropertyItem(name);
        }

        private void onClickEffectPropertyRemoveItemButtonCallback(string name, int index)
        {
            var array = _effect.getProp<Array>(name);
            if (array == null)
                return;
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
            OnEdited();
            updatePropertyItem(name);
        }
        private void onEffectPropertyValueChangedCallback(string name, object value)
        {
            var prop = _effect.getProp(name);
            if (prop is Array array)
            {
                array.SetValue(value, 0);
            }
            else
            {
                _effect.setProp(name, value);
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
            bool hideBuiltinCards = true;
            if (prop.defineId != 0)
            {
                poolId = prop.cardPoolId;
            }
            if (game.card.isBuiltinCardPool(poolId))
            {
                hideBuiltinCards = false;
            }
            await _cardPoolViewPanelCtrl.display(new long[] { poolId }, hideBuiltinCards: hideBuiltinCards, cardFilter: card => true);
            _ui.displayCardSelectPanel(true);
            _curCardSeekingEffectPropName = name;
            _curCardSeekingEffectPropIndex = -1;
        }

        private void onDeleteEffectPropertyCallback(string name)
        {
            _effect.removeProp(name);
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
            item.updateEffectProperties(items, game.card);
        }
        /// <summary>
        /// 更新一项属性。
        /// </summary>
        /// <param name="name">属性名称。</param>
        private void updatePropertyItem(string name)
        {
            var item = GetEffectPropertyItem(name);
            var nodeItem = getEffectEntryNodeItem();
            nodeItem.updateEffectPropertyItem(name, item, game.card);
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
