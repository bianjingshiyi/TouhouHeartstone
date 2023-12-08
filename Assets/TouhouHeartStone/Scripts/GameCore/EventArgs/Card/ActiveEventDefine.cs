using System;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class ActiveEventDefine : EventDefine, IDescribableEventDefine, ICardEventDefine
    {
        public ActiveEventDefine()
        {
            beforeVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = EVENTVAR.CARD, type = typeof(Card)},
                new EventVariableInfo() { name = EVENTVAR.PLAYER,type = typeof(Player)},
                new EventVariableInfo() { name = EVENTVAR.TARGET_CARD,type = typeof(Card[])}
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = EVENTVAR.CARD, type = typeof(Card)},
                new EventVariableInfo() { name = EVENTVAR.PLAYER,type = typeof(Player)},
                new EventVariableInfo() { name = EVENTVAR.TARGET_CARD,type = typeof(Card[])}
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
            record.setCardStates(EVENTVAR.TARGET_CARD, arg);
        }
        public override async Task execute(IEventArg arg)
        {
            var game = arg.game;
            var card = arg.getVar<Card>(EVENTVAR.CARD);
            var effectRef = arg.getVar<EffectReference>(VAR_EFFECT_REF);
            var effect = effectRef.getEffect();
            if (effect is IActiveEffect active)
            {
                var effectEnv = new EffectEnv(game, card, effectRef.cardDefine, effectRef.buff, arg as EventArg, effect);
                if (active.checkCondition(effectEnv))
                    await active.execute(effectEnv);
            }
        }
        public static Task<EventArg> doEvent(CardEngine game, Card source, Card[] targets, CardDefine sourceCardDefine, Buff buff, IEffect effect)
        {
            var arg = newEvent(game, source, targets);

            arg.setVar(VAR_EFFECT_REF, EffectReference.fromAny(sourceCardDefine, buff, effect));
            return game.triggers.doEvent(arg);
        }
        public static EventArg newEvent(CardEngine game, Card card, Card[] targets)
        {
            var define = game.getEventDefine<ActiveEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(EVENTVAR.PLAYER, card.getOwner());
            arg.setVar(EVENTVAR.CARD, card);
            arg.setVar(EVENTVAR.TARGET_CARD, targets);
            return arg;
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
            var targets = eventArg.getVar<Card[]>(EVENTVAR.TARGET_CARD);
            return targets.OfType<Card>().Select(t => (DescribableCard)t).ToArray();
        }
        Player IDescribableEventDefine.getPlayer(IGame game, EventArg eventArg)
        {
            return eventArg.getVar<Player>(EVENTVAR.PLAYER);
        }
        string IDescribableEventDefine.localizeTemplateString(IGame game, EventArg eventArg, IPlayer viewer) => localizeTemplateString(eventArg);
        object[] IDescribableEventDefine.localizeStringArgs(IGame game, EventArg eventArg, IPlayer viewer) => localizeStringArgs(eventArg);
        string IDescribableEventDefine.localizeLogItemNameTemplate(IGame game, EventArg eventArg)
        {
            return null;
        }
        object[] IDescribableEventDefine.localizeLogItemNameArgs(IGame game, EventArg eventArg)
        {
            return null;
        }
        string IDescribableEventDefine.toString(IGame game, EventArg eventArg, IPlayer viewer) => toString(eventArg);
        
        ICard ICardEventDefine.getCard(IEventArg arg)
        {
            var card = arg.getVar<Card>(EVENTVAR.CARD);
            return card;
        }
        #endregion

        private object[] localizeStringArgs(EventArg eventArg)
        {
            var player = eventArg.getVar<Player>(EVENTVAR.PLAYER);
            var card = eventArg.getVar<Card>(EVENTVAR.CARD);
            var targets = eventArg.getVar<Card[]>(EVENTVAR.TARGET_CARD);
            var cardDefine = eventArg.getVar<CardDefine>(EVENTVAR.CARD_DEFINE);
            return new object[] {
                player.name,
                card.getFormatString(),
                string.Join(", ", targets.Select(t => t is Card c ? c.getFormatString() : t.ToString())),
                cardDefine.getFormatString(),
            };
        }

        private string localizeTemplateString(EventArg eventArg)
        {
            var targets = eventArg.getVar<Card[]>(EVENTVAR.TARGET_CARD);
            var card = eventArg.getVar<Card>(EVENTVAR.CARD);
            var cardDefine = eventArg.getVar<CardDefine>(EVENTVAR.CARD_DEFINE);
            bool isSameCardDefine = cardDefine == null || card.define == cardDefine;
            if (targets == null || targets.Length < 1)
                return isSameCardDefine ? TEMPLATE_NO_TARGET : TEMPLATE_OTHER_NO_TARGET;
            else
                return isSameCardDefine ? TEMPLATE_HAS_TARGET : TEMPLATE_OTHER_HAS_TARGET;
        }

        #endregion
        [MukioI18n.TranslateMsg("{0}是玩家，{1}是卡，{2}是目标列表")]
        const string TEMPLATE_HAS_TARGET = "{0} 对 {2} 发动 {1} 的效果";
        [MukioI18n.TranslateMsg("{0}是玩家，{1}是卡")]
        const string TEMPLATE_NO_TARGET = "{0} 发动了 {1} 的效果";
        [MukioI18n.TranslateMsg("{0}是玩家，{1}是卡，{2}是目标列表，{3}是效果的卡牌定义来源")]
        const string TEMPLATE_OTHER_HAS_TARGET = "{0} 使用 {1} 对 {2} 发动 {3} 的效果";
        [MukioI18n.TranslateMsg("{0}是玩家，{1}是卡，{3}是效果的卡牌定义来源")]
        const string TEMPLATE_OTHER_NO_TARGET = "{0} 使用 {1} 发动了 {3} 的效果";
        public const string VAR_EFFECT_REF = "效果引用";
    }
}
