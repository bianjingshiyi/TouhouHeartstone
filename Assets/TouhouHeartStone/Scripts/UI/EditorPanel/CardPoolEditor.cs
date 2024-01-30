using System;
using System.Linq;
using System.Threading.Tasks;
using Controllers;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Shared;
using TouhouHeartstone;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// 用于编辑卡池的编辑器UI基类。
    /// </summary>
    abstract class CardPoolEditor : GameObjectCtrl<MainSceneCtrl>
    {
        #region 公有方法
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
        #endregion

        #region 私有方法

        #region 生命周期
        protected virtual void Awake()
        {
            _cardPoolViewPanelCtrl.onClickCard += onClickCardPoolViewCard;
            _cancelCardPoolSelectButton.onClick.AddListener(onClickCancelCardPoolSelectButton);
        }
        #endregion

        #region 事件回调
        private void onClickCardPoolViewCard(CardDefine cardDefine)
        {
            _cardPoolViewPanelObj.SetActive(false);
            if (_cardSelectSource == null)
                return;
            onCardSelected(_cardSelectSource, cardDefine);
            _cardSelectSource = null;
        }
        private void onClickCancelCardPoolSelectButton()
        {
            //打开卡牌选择界面
            _cardPoolViewPanelObj.SetActive(false);
            _cardSelectSource = null;
        }
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

        #region 卡池环境
        protected ICardPoolReference[] getCardPoolReferenceEnv()
        {
            return game.editor.getEnvCardPoolRefs();
        }
        protected ICardPoolReference getCardPoolReference(long cardPoolId)
        {
            return game.card.getCardPoolReferenceInEnv(cardPoolId, getCardPoolReferenceEnv());
        }
        protected ICardPoolReference getCurrentCardPoolRef()
        {
            return getCardPoolReference(_curCardPoolId);
        }
        #endregion

        #region 选卡
        protected virtual void onCardSelected(ICardSelectSource source, CardDefine cardDefine)
        {

        }
        protected async Task displayCardSelectPanel(ICardSelectSource source, long[] cardPoolFiltersId = null, DefineReference[] characterRefs = null, Predicate<CardDefine> cardFilter = null)
        {
            _cardSelectSource = source;
            var cardPoolsRange = getCardPoolReferenceEnv()
                .Where(poolRef => game.card.isLocalCardPoolRefExists(poolRef))
                .Select(r => r.CardPoolID)
                .ToArray();
            await _cardPoolViewPanelCtrl.display(cardPoolsRange, cardPoolFiltersId, characterRefs, cardFilter);
            _cardPoolViewPanelObj.SetActive(true);
        }
        #endregion

        #endregion

        #region 属性字段
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
        [SerializeField]
        GameObject _cardPoolViewPanelObj;
        [SerializeField]
        CardPoolViewPanelCtrl _cardPoolViewPanelCtrl;
        [SerializeField]
        Button _cancelCardPoolSelectButton;
        private ICardSelectSource _cardSelectSource;

        protected long _curCardPoolId;
        #endregion
    }
    public interface ICardSelectSource
    {
    }
    public struct CardSelectSourceCustomProperty : ICardSelectSource
    {
        /// <summary>
        /// 当前正在选择的卡牌引用属性的属性名称。
        /// </summary>
        public string propName;
        /// <summary>
        /// 当前正在选择的卡牌引用属性的列表索引。
        /// </summary>
        public int[] propIndexes;
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
        public GeneratedRuleLogic ruleLogic { get; private set; }
        public Type type { get; private set; }
        public CardPoolDisplayObject(CardDefine card)
        {
            cardDefine = card;
            type = Type.Card;
        }
        public CardPoolDisplayObject(GeneratedBuffDefine buff)
        {
            buffDefine = buff;
            type = Type.Buff;
        }
        public CardPoolDisplayObject(GeneratedActionDefine action)
        {
            actionDefine = action;
            type = Type.Action;
        }
        public CardPoolDisplayObject(GeneratedRuleLogic logic)
        {
            ruleLogic = logic;
            type = Type.RuleLogic;
        }
        public object getObj()
        {
            switch (type)
            {
                case Type.Card:
                    return cardDefine;
                case Type.Buff:
                    return buffDefine;
                case Type.Action:
                    return actionDefine;
                case Type.RuleLogic:
                    return ruleLogic;
            }
            return null;
        }
        public bool compare(CardPoolDisplayObject obj)
        {
            return compareObj(obj.getObj());
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
            else if (ruleLogic is GeneratedRuleLogic logic1 && obj is GeneratedRuleLogic logic2)
            {
                return logic1.cardPoolId == logic2.cardPoolId && logic1.id == logic2.id;
            }
            return false;
        }
        public enum Type
        {
            Card,
            Buff,
            Action,
            RuleLogic
        }
    }
}
