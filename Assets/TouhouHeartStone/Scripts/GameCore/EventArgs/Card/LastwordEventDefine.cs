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
            record.setVar(VAR_EFFECT_REF, arg);
            record.setCardState(EVENTVAR.CARD, arg);
        }
        public override async Task execute(IEventArg arg)
        {
            var game = arg.game;
            var card = arg.getVar<Card>(EVENTVAR.CARD);
            var effectRef = arg.getVar<EffectReference>(VAR_EFFECT_REF);
            var effect = effectRef.getEffect();
            if (effect is IPassiveEffect genPassive)
            {
                var effectEnv = new EffectEnv(game, card, effectRef.cardDefine, effectRef.buff, arg as EventArg, effect);
                if (genPassive.checkCondition(effectEnv))
                    await genPassive.execute(effectEnv);
            }
        }
        public static Task<EventArg> doEvent(CardEngine game, Card card, CardDefine sourceCardDefine, Buff buff, IEffect effect)
        {
            var define = game.getEventDefine<LastwordEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(EVENTVAR.PLAYER, card.getOwner());
            arg.setVar(EVENTVAR.CARD, card);
            arg.setVar(VAR_EFFECT_REF, EffectReference.fromAny(sourceCardDefine, buff, effect));
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
        #endregion

        [TranslateMsg("{0}是玩家，{1}是卡")]
        const string TEMPLATE_SELF = "{0} 的 {1} 触发遗言";
        [TranslateMsg("{0}是玩家，{1}是卡，{2}是效果的卡牌定义来源")]
        const string TEMPLATE_OTHER = "{0} 使用 {1} 触发 {2} 的遗言";
        [TranslateMsg("游戏事件日志项名称")]
        private const string LOG_ITEM_NAME = "遗言";
        public const string VAR_EFFECT_REF = "效果引用";
    }
}
