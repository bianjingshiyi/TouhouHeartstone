using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using BJSYGameCore;
using System.Linq;
using Game;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace Controllers
{
    partial class EffectEditorCtrl : GameObjectCtrl<MainSceneCtrl>
    {
        public void displayEffect(GeneratedEffect effect, ActionNodeGraphInfo graphInfo)
        {
            _ui.gameObject.SetActive(true);
            _effect = effect;
            this.graphInfo = graphInfo;
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
            string[] activeRanges = new string[_selectableRanges.Length];
            for (int i = 0; i < activeRanges.Length; i++)
            {
                activeRanges[i] = game.langManager.getString(_pileOptionNameDict[_selectableRanges[i]]);
            }
            _ui.setActiveRanges(activeRanges);
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
        #region 属性字段
        public ActionNodeGraphInfo graphInfo { get; private set; }
        [SerializeField]
        EffectEditorPanel _ui;
        [Obsolete("使用game作为替代")]
        public IAppManager app { get; }
        GeneratedEffect _effect;
        string[] _selectableRanges;
        readonly Dictionary<string, string> _pileOptionNameDict = new Dictionary<string, string>()
        {
            { PILE.FIELD, StringTable.GAME_FIELD }  ,
            { PILE.HAND, StringTable.GAME_HAND },
            { PILE.DECK, StringTable.GAME_DECK },
            { PILE.GRAVE, StringTable.GAME_GRAVE },
            { PILE.MASTER, StringTable.GAME_MASTER } ,
            { PILE.SKILL, StringTable.GAME_SKILL },
            { PILE.ITEM, StringTable.GAME_ITEM },
            { PILE.HEAD, StringTable.GAME_DELAY }
        };
        #endregion
    }
}
