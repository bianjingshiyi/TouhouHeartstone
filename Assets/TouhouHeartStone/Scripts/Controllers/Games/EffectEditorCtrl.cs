using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using BJSYGameCore;
using System.Linq;
using Game;
using UnityEngine;
using System;

namespace Controllers
{
    partial class EffectEditorCtrl : GameObjectCtrl<MainSceneCtrl>
    {
        public void displayEffect(GeneratedEffect effect)
        {
            _ui.gameObject.SetActive(true);
            _effect = effect;
            bool[] activeRangeValue = new bool[_selectableRanges.Length];
            for (int i = 0; i < activeRangeValue.Length; i++)
            {
                activeRangeValue[i] = _effect.pileList.Contains(_selectableRanges[i]);
            }
            _ui.setActiveRange(activeRangeValue);
            updateTriggerList();
        }
        #region 私有方法
        #region 生命周期
        protected void Awake()
        {
            if (_ui == null)
            {
                _ui = GetComponentInChildren<EffectEditorPanel>();
            }
            _selectableRanges = new string[]
            {
                PILE.FIELD,
                PILE.HAND,
                PILE.DECK,
                PILE.GRAVE,
                PILE.MASTER,
                PILE.SKILL,
                PILE.ITEM,
                PILE.HEAD
            };
            _ui.setActiveRanges(_selectableRanges);
            _ui.onActiveRangeDropdownValueChanged += onActiveRangeDropdownValueChanged;
            _ui.onClickEditEnableActionButton += onClickEditEnableActionButton;
            _ui.onClickEditDisableActionButton += onClickEditDisableActionButton;
            _ui.onClickAddTriggerButton += onClickAddTriggerButton;
            _ui.onClickRemoveTriggerButton += onClickRemoveTriggerButton;
            _ui.onClickEditTriggerButton += onClickEditTriggerButton;
            _ui.onClickReturnButton += onClickReturnButton;
        }
        #endregion
        #region UI回调
        void onActiveRangeDropdownValueChanged(bool[] values)
        {
            for (int i = 0; i < _selectableRanges.Length; i++)
            {
                if (values[i] && !_effect.pileList.Contains(_selectableRanges[i]))
                    _effect.pileList.Add(_selectableRanges[i]);
                else
                    _effect.pileList.Remove(_selectableRanges[i]);
            }
        }
        void onClickEditEnableActionButton()
        {
            scene.actionNodeGraphCtrl.openOnEnableActionGraph(_effect);
        }
        void onClickEditDisableActionButton()
        {
            scene.actionNodeGraphCtrl.openOnDisableActionGraph(_effect);
        }
        void onClickAddTriggerButton()
        {
            _effect.triggerList.Add(new TriggerGraph());
            updateTriggerList();
        }
        void onClickRemoveTriggerButton(int index)
        {
            _effect.triggerList.RemoveAt(index);
        }
        void onClickEditTriggerButton(int index)
        {
            scene.actionNodeGraphCtrl.openTriggerGraph(_effect.triggerList[index]);
        }
        void onClickReturnButton()
        {
            _ui.gameObject.SetActive(false);
        }
        #endregion
        private void updateTriggerList()
        {
            string[] triggerNames = new string[_effect.triggerList.Count];
            for (int i = 0; i < triggerNames.Length; i++)
            {
                triggerNames[i] = "触发器" + (i + 1);
            }
            _ui.updateTriggerList(triggerNames);
        }
        #endregion
        [Obsolete("使用game作为替代")]
        public IAppManager app { get; }
        EffectEditorPanel _ui;
        GeneratedEffect _effect;
        string[] _selectableRanges;
    }
}
