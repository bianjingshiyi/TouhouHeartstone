using System;
using System.Linq;
using System.Threading.Tasks;
using BJSYGameCore;
using BJSYGameCore.UI;
using Game;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using UnityEngine;

namespace Controllers
{
    partial class EffectEditorCtrl : GameObjectCtrl<MainSceneCtrl>, IUICtrl
    {
        #region 公有方法
        public Task display(long cardPoolId, CardDefine cardDefine, GeneratedEffect effect)
        {
            return display(new DisplayInfo(cardPoolId, cardDefine, effect, new ActionNodeGraphCtrl.DisplayInfo(
                        cardPoolId,
                        effect,
                        null,
                        game.card.getOrCreateCardEffectActionNodeGraphInfo(cardPoolId, cardDefine.id, cardDefine.getEffects().indexOf(effect)))
                ));
        }
        public Task display(long cardPoolId, GeneratedBuffDefine buffDefine, GeneratedEffect effect)
        {
            return display(new DisplayInfo(cardPoolId, buffDefine, effect, new ActionNodeGraphCtrl.DisplayInfo(
                        cardPoolId,
                        effect,
                        null,
                        game.card.getOrCreateBuffEffectActionNodeGraphInfo(cardPoolId, buffDefine.id, buffDefine.effectList.IndexOf(effect))
                )));
        }
        public async Task display(UIDisplayInfo displayInfo)
        {
            if (displayInfo is DisplayInfo info)
            {
                _cardPoolId = info.cardPoolId;
                _cardDefine = info.cardDefine;
                _buffDefine = info.buffDefine;
                _effect = info.effect;
                gameObject.SetActive(true);
                if (info.actionNodeGraphDisplayInfo != null)
                    await _actionNodeGraphCtrl.display(info.actionNodeGraphDisplayInfo);
                else
                    await _actionNodeGraphCtrl.hide();
                updateEventTriggerTimes(_cardPoolId);
            }
        }
        public void updateEventTriggerTimes(long cardPoolId)
        {
            _eventTriggerTimes = game.card.getAllEventDefines(cardPoolId).getEventTriggerTimes();
            _ui.setEventTypeNames(_eventTriggerTimes.getEventTriggerTimeNamesForEditor(game));
        }
        public UIDisplayInfo getCurrentDisplayInfo()
        {
            if (_buffDefine != null)
            {
                return new DisplayInfo(_cardPoolId, _buffDefine, _effect,
                    _actionNodeGraphCtrl.getCurrentDisplayInfo() as ActionNodeGraphCtrl.DisplayInfo);
            }
            else
            {
                return new DisplayInfo(_cardPoolId, _cardDefine, _effect,
                    _actionNodeGraphCtrl.getCurrentDisplayInfo() as ActionNodeGraphCtrl.DisplayInfo);
            }
        }
        public GeneratedEffect getCurrentEffect()
        {
            return _effect;
        }
        public Task hide()
        {
            hideActionGraph();
            gameObject.SetActive(false);
            return Task.CompletedTask;
        }
        public void hideActionGraph()
        {
            _cardPoolId = 0;
            _cardDefine = null;
            _buffDefine = null;
            _effect = null;
            _actionNodeGraphCtrl.hide();
        }

        public void createEffectByIndex(int index)
        {
            // 事件效果。
            if (index == CardManager.EVENT_EFFECT_IDX)
            {
                _ui.setEventTypeValue(0);
                _ui.displayEventEffectDialog(true);
                return;
            }
            createEffect(index);
        }
        #endregion

        #region 私有方法

        #region 生命周期
        public void Awake()
        {
            _ui.onConfirmCreateEventEffect += onConfirmCreateEventEffectCallback;
            _actionNodeGraphCtrl.OnGraphEdited += OnGraphEditedCallback;
        }
        #endregion

        #region 事件回调
        private void OnGraphEditedCallback()
        {
            OnGraphEdited?.Invoke();
        }
        
        private void onConfirmCreateEventEffectCallback(int index)
        {
            var effect = game.card.createEffectByIndex(_cardPoolId, CardManager.EVENT_EFFECT_IDX);
            effect.graph.createEventEntryNode(_eventTriggerTimes[index]);
            onCreateEffect?.Invoke(effect);
            _ui.displayEventEffectDialog(false);
        }
        #endregion

        private GeneratedEffect createEffect(int index)
        {
            var effect = game.card.createEffectByIndex(_cardPoolId, index);
            onCreateEffect?.Invoke(effect);
            return effect;
        }

        #endregion

        #region 事件
        public event Action<GeneratedEffect> onCreateEffect;
        public event Action OnGraphEdited;
        #endregion

        #region 属性字段
        [SerializeField]
        EffectEditorPanel _ui;

        [SerializeField]
        ActionNodeGraphCtrl _actionNodeGraphCtrl;
        long _cardPoolId;
        CardDefine _cardDefine;
        GeneratedBuffDefine _buffDefine;
        GeneratedEffect _effect;
        EventTriggerTime[] _eventTriggerTimes;
        #endregion

        #region 内部类
        public class DisplayInfo : UIDisplayInfo
        {
            public DisplayInfo(long cardPoolId, CardDefine cardDefine, GeneratedEffect effect, ActionNodeGraphCtrl.DisplayInfo actionNodeGraphDisplayInfo)
            {
                this.cardPoolId = cardPoolId;
                this.cardDefine = cardDefine;
                this.effect = effect;
                this.actionNodeGraphDisplayInfo = actionNodeGraphDisplayInfo;
            }
            public DisplayInfo(long cardPoolId, GeneratedBuffDefine buffDefine, GeneratedEffect effect, ActionNodeGraphCtrl.DisplayInfo actionNodeGraphDisplayInfo)
            {
                this.cardPoolId = cardPoolId;
                this.buffDefine = buffDefine;
                this.effect = effect;
                this.actionNodeGraphDisplayInfo = actionNodeGraphDisplayInfo;
            }
            public long cardPoolId;
            public CardDefine cardDefine;
            public GeneratedBuffDefine buffDefine;
            public GeneratedEffect effect;
            public ActionNodeGraphCtrl.DisplayInfo actionNodeGraphDisplayInfo;
        }
        #endregion
    }
}
