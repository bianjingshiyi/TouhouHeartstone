using System.Threading.Tasks;
using Controllers;
using MukioI18n;
using TouhouCardEngine;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// 用于编辑卡池的编辑器UI基类。
    /// </summary>
    abstract class CardPoolEditor : GameObjectCtrl<MainSceneCtrl>
    {
        public abstract Task display(CardPoolDisplayPage info);
        public virtual void hide()
        {
            gameObject.SetActive(false);
        }
        public virtual void markUnsaved()
        {
            hasUnsavedChanges = true;
            if (displayInfo != null)
                displayInfo.unsaved = true;
        }
        public virtual void clearUnsaved()
        {
            hasUnsavedChanges = false;
            if (displayInfo != null)
                displayInfo.unsaved = false;
        }
        #region 事件回调
        #endregion
        #region 效果
        protected abstract void setUIEffects(string[] names, int activeIndex);
        protected void updateEffectList(GeneratedEffect[] effects)
        {
            var currentEffect = _effectEditorCtrl.getCurrentEffect();
            int activeIndex = -1;
            string[] names = new string[effects.Length];
            for (int i = 0; i < effects.Length; i++)
            {
                var effect = effects[i];
                if (string.IsNullOrEmpty(effect.name))
                {
                    effect.name = getEffectDefaultName(effect, i + 1);
                }
                names[i] = effect.name;
                if (currentEffect == effect)
                {
                    activeIndex = i;
                }
            }
            setUIEffects(names, activeIndex);
        }
        protected string getEffectDefaultName(GeneratedEffect effect, int num)
        {
            var names = game.card.getEffectNames();
            var index = game.card.getEffectTypeIndex(effect);
            return game.lang._(CARD_EFFECT_DEFAULT_NAME_TEMPLATE, names?[index] ?? CARD_EFFECT_DEFAULT_NAME, num);
        }
        #endregion
        public bool hasUnsavedChanges { get; protected set; }
        public CardPoolDisplayPage displayInfo { get; protected set; }
        [TranslateMsg("效果默认名称")]
        public const string CARD_EFFECT_DEFAULT_NAME = "效果";
        [TranslateMsg("效果默认名称的模板（{0}为效果名，{1}为效果序号）")]
        public const string CARD_EFFECT_DEFAULT_NAME_TEMPLATE = "{0}{1}";
        [SerializeField]
        protected EffectEditorCtrl _effectEditorCtrl;
        [SerializeField]
        protected GraphEditorPanelCtrl _graphEditor;
    }
    public class CardPoolDisplayPage
    {
        public long cardPoolId;
        public CardPoolDisplayObject displayObj;
        public object[] skins;
        public bool unsaved;
    }
    public class CardPoolDisplayObject
    {
        public CardDefine cardDefine { get; private set; }
        public GeneratedBuffDefine buffDefine { get; private set; }
        public GeneratedActionDefine actionDefine { get; private set; }
        public CardPoolDisplayObject(CardDefine card)
        {
            cardDefine = card;
        }
        public CardPoolDisplayObject(GeneratedBuffDefine buff)
        {
            buffDefine = buff;
        }
        public CardPoolDisplayObject(GeneratedActionDefine action)
        {
            actionDefine = action;
        }
        public bool compare(CardPoolDisplayObject obj)
        {
            if (cardDefine is CardDefine card1 && obj.cardDefine is CardDefine card2)
            {
                return card1.cardPoolId == card2.cardPoolId && card1.id == card2.id;
            }
            else if (buffDefine is GeneratedBuffDefine buff1 && obj.buffDefine is GeneratedBuffDefine buff2)
            {
                return buff1.cardPoolId == buff2.cardPoolId && buff1.id == buff2.id;
            }
            else if (actionDefine is GeneratedActionDefine action1 && obj.actionDefine is GeneratedActionDefine action2)
            {
                return action1.cardPoolId == action2.cardPoolId && action1.defineId == action2.defineId;
            }
            return false;
        }
        public bool compareObj(object obj)
        {
            if (cardDefine is CardDefine card1 && obj is CardDefine card2)
            {
                return card1.cardPoolId == card2.cardPoolId && card1.id == card2.id;
            }
            else if (buffDefine is GeneratedBuffDefine buff1 && obj is GeneratedBuffDefine buff2)
            {
                return buff1.cardPoolId == buff2.cardPoolId && buff1.id == buff2.id;
            }
            else if (actionDefine is GeneratedActionDefine action1 && obj is GeneratedActionDefine action2)
            {
                return action1.cardPoolId == action2.cardPoolId && action1.defineId == action2.defineId;
            }
            return false;
        }
    }
}
