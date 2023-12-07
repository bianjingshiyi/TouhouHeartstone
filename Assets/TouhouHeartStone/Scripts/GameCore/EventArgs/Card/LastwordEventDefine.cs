using System;
using System.Threading.Tasks;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class LastwordEventDefine : EventDefine, IDescribableEventDefine, ICardEventDefine
    {
        public LastwordEventDefine()
        {
            beforeVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = EVENTVAR.CARD, type = typeof(Card)},
                new EventVariableInfo() { name = EVENTVAR.PLAYER,type = typeof(Player)},
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = EVENTVAR.CARD, type = typeof(Card)},
                new EventVariableInfo() { name = EVENTVAR.PLAYER,type = typeof(Player)},
            };
        }
        public override string toString(EventArg arg)
        {
            return string.Format(localizeTemplateString(arg), localizeStringArgs(arg));
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setVar(EVENTVAR.PLAYER, arg);
            record.setVar(VAR_BUFF, arg);
            record.setVar(VAR_EFFECT_INDEX, arg);
            record.setCardState(EVENTVAR.CARD, arg);
            record.setVar(EVENTVAR.CARD_DEFINE, arg);
        }
        public override async Task execute(IEventArg arg)
        {
            var game = arg.game;
            var card = arg.getVar<Card>(EVENTVAR.CARD);
            var cardDefine = arg.getVar<CardDefine>(EVENTVAR.CARD_DEFINE);
            var buff = arg.getVar<Buff>(VAR_BUFF);
            var effectIndex = arg.getVar<int>(VAR_EFFECT_INDEX);

            var effects = getEffectArray(cardDefine, buff);
            if (effects == null || effectIndex < 0 || effectIndex >= effects.Length)
                return;
            var effect = effects[effectIndex];
            if (effect is IPassiveEffect genPassive)
            {
                if (genPassive.checkCondition(game, card, buff, arg))
                    await genPassive.execute(game, card, buff, arg);
            }
        }
        public static Task<EventArg> doEvent(CardEngine game, Card source, CardDefine cardDefine, IEffect effect)
        {
            var define = game.getEventDefine<LastwordEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(EVENTVAR.PLAYER, source.getOwner());
            arg.setVar(EVENTVAR.CARD, source);

            arg.setVar(EVENTVAR.CARD_DEFINE, cardDefine);
            arg.setVar(VAR_EFFECT_INDEX, Array.IndexOf(cardDefine.getEffects(), effect));
            return game.triggers.doEvent(arg);
        }
        public static Task<EventArg> doEvent(CardEngine game, Card source, Buff buff, IEffect effect)
        {
            if (buff == null)// 没有设置增益，直接使用卡牌的定义中的效果。
                return doEvent(game, source, source.define, effect);

            var define = game.getEventDefine<LastwordEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(EVENTVAR.PLAYER, source.getOwner());
            arg.setVar(EVENTVAR.CARD, source);

            arg.setVar(VAR_BUFF, buff);
            arg.setVar(VAR_EFFECT_INDEX, Array.IndexOf(buff.getEffects(), effect));
            return game.triggers.doEvent(arg);
        }
        #region 私有方法

        #region 接口实现
        DescribableCard IDescribableEventDefine.getCard(IGame game, EventArg eventArg, IPlayer viewer)
        {
            var card = eventArg.getVar<Card>(EVENTVAR.CARD);
            return card;
        }
        DescribableCard[] IDescribableEventDefine.getTargets(IGame game, EventArg eventArg, IPlayer viewer)
        {
            return null;
        }
        string IDescribableEventDefine.localizeTemplateString(IGame game, EventArg eventArg, IPlayer viewer) => localizeTemplateString(eventArg);
        object[] IDescribableEventDefine.localizeStringArgs(IGame game, EventArg eventArg, IPlayer viewer) => localizeStringArgs(eventArg);
        string IDescribableEventDefine.toString(IGame game, EventArg eventArg, IPlayer viewer) => toString(eventArg);
        ICard ICardEventDefine.getCard(IEventArg arg)
        {
            var card = arg.getVar<Card>(EVENTVAR.CARD);
            return card;
        }
        Player IDescribableEventDefine.getPlayer(IGame game, EventArg eventArg) => eventArg.getVar<Card>(EVENTVAR.CARD).getOwner();
        string IDescribableEventDefine.localizeLogItemNameTemplate(IGame game, EventArg eventArg) => LOG_ITEM_NAME;
        object[] IDescribableEventDefine.localizeLogItemNameArgs(IGame game, EventArg eventArg) => null;
        #endregion

        private object[] localizeStringArgs(EventArg eventArg)
        {
            var player = eventArg.getVar<Player>(EVENTVAR.PLAYER);
            var card = eventArg.getVar<Card>(EVENTVAR.CARD);
            var cardDefine = eventArg.getVar<CardDefine>(EVENTVAR.CARD_DEFINE);
            return new object[] {
                player.name,
                card.getFormatString(),
                cardDefine.getFormatString(),
            };
        }

        private string localizeTemplateString(EventArg eventArg)
        {
            var card = eventArg.getVar<Card>(EVENTVAR.CARD);
            var cardDefine = eventArg.getVar<CardDefine>(EVENTVAR.CARD_DEFINE);
            bool isSameCardDefine = cardDefine == null || card.define == cardDefine;
            return isSameCardDefine ? TEMPLATE_SELF : TEMPLATE_OTHER;
        }
        private IEffect[] getEffectArray(CardDefine cardDefine, Buff buff)
        {
            return buff != null ? buff.getEffects() : cardDefine.getEffects();
        }
        #endregion

        [TranslateMsg("{0}是玩家，{1}是卡")]
        const string TEMPLATE_SELF = "{0} 的 {1} 触发遗言";
        [TranslateMsg("{0}是玩家，{1}是卡，{2}是效果的卡牌定义来源")]
        const string TEMPLATE_OTHER = "{0} 使用 {1} 触发 {2} 的遗言";
        [TranslateMsg("游戏事件日志项名称")]
        private const string LOG_ITEM_NAME = "遗言";
        public const string VAR_BUFF = "增益";
        public const string VAR_EFFECT_INDEX = "效果索引";
    }
}
