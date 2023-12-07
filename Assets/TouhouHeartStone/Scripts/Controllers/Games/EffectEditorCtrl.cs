using System;
using System.Threading.Tasks;
using BJSYGameCore.UI;
using Game;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using UnityEngine;

namespace Controllers
{
    partial class EffectEditorCtrl : GameObjectCtrl<MainSceneCtrl>
    {
        #region 公有方法
        public async Task display(long cardPoolId, GeneratedEffect effect)
        {
            _cardPoolId = cardPoolId;
            _effect = effect;
            gameObject.SetActive(true);
            await _actionNodeGraphCtrl.display(new ActionNodeGraphCtrl.DisplayInfo(cardPoolId, effect));
            updateEventTriggerTimes(_cardPoolId);
        }
        public void updateEventTriggerTimes(long cardPoolId)
        {
            var poolRef = game.card.getLocalCardPoolReference(cardPoolId);
            _eventTriggerTimes = game.card.getAllEventDefines(includeHidden: false, poolRef).getEventTriggerTimes();
            _ui.setEventTypeNames(_eventTriggerTimes.getEventTriggerTimeNamesForEditor(game));
        }
        public GeneratedEffect getCurrentEffect()
        {
            return _effect;
        }
        public void hideActionGraph()
        {
            _cardPoolId = 0;
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
            var triggerTime = _eventTriggerTimes[index];
            var eventDefine = game.card.getLocalEventDefine(triggerTime.defineRef);
            effect.graph.createEventEntryNode(eventDefine, triggerTime.type);

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
