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
        public async Task display(GeneratedEffect effect)
        {
            _effect = effect;
            gameObject.SetActive(true);
            await _actionNodeGraphCtrl.display(effect);
            updateEventTriggerTimes();
        }
        public void updateEventTriggerTimes()
        {
            _ui.setEventTypeNames(eventTriggerTimeEditorNames);
        }
        public GeneratedEffect getCurrentEffect()
        {
            return _effect;
        }
        public void hideActionGraph()
        {
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
            var effect = game.card.createEffectByIndex(CardManager.EVENT_EFFECT_IDX);
            var triggerTime = eventTriggerTimes[index];
            var eventDefine = game.card.getEventDefineInEnv(triggerTime.defineRef, game.editor.getEnvCardPoolRefs());
            effect.graph.createEventEntryNode(eventDefine, triggerTime.type);

            onCreateEffect?.Invoke(effect);
            _ui.displayEventEffectDialog(false);
        }
        #endregion

        private GeneratedEffect createEffect(int index)
        {
            var effect = game.card.createEffectByIndex(index);
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
        GeneratedEffect _effect;
        private long curCardPoolId => game.editor.currentCardPoolId;
        private EventTriggerTime[] eventTriggerTimes => scene.editor.eventTriggerTimes;
        private string[] eventTriggerTimeEditorNames => scene.editor.eventTriggerTimeEditorNames;
        #endregion
    }
}
