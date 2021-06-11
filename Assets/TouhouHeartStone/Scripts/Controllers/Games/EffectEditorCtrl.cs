using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using BJSYGameCore;
using System.Linq;

namespace Controllers
{
    public partial class EffectEditorCtrl : IController<MainCtrl>
    {
        public EffectEditorCtrl(IAppManager app, MainCtrl main, EffectEditorPanel compo)
        {
            this.app = app;
            this.main = main;
            _compo = compo;
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
            _compo.onClickEditActiveRangeButton += onClickEditActiveRangeButton;
            _compo.onCloseActiveRangeToggleGroup += onCloseActiveRangeToggleGroup;
            _compo.onClickEditEnableActionButton += onClickEditEnableActionButton;
            _compo.onClickEditDisableActionButton += onClickEditDisableActionButton;
            _compo.onClickAddTriggerButton += onClickAddTriggerButton;
            _compo.onClickRemoveTriggerButton += onClickRemoveTriggerButton;
            _compo.onClickEditTriggerButton += onClickEditTriggerButton;
        }
        public void displayEffect(GeneratedEffect effect)
        {
            _effect = effect;
        }
        #region UI回调
        void onClickEditActiveRangeButton()
        {
            _compo.displayActiveRangeToggleGroup(_selectableRanges, _selectableRanges.Select(p => _effect.pileList.Contains(p)).ToArray());
        }
        void onCloseActiveRangeToggleGroup(bool[] values)
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
            //TODO:加载动作图信息和显示动作图
        }
        void onClickEditDisableActionButton()
        {
            //TODO:加载动作图信息和显示动作图
        }
        void onClickAddTriggerButton()
        {
            _effect.triggerList.Add(new TriggerGraph());
        }
        void onClickRemoveTriggerButton(int index)
        {
            _effect.triggerList.RemoveAt(index);
        }
        void onClickEditTriggerButton(int index)
        {
            //TODO:加载动作图信息和显示动作图
        }
        #endregion
        public IAppManager app { get; }
        public MainCtrl main { get; }
        EffectEditorPanel _compo;
        GeneratedEffect _effect;
        string[] _selectableRanges;
    }
}
