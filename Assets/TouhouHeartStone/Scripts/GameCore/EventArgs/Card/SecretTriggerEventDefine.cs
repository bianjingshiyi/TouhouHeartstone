using System;
using System.Threading.Tasks;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    [EventChildren(typeof(MoveCardEventDefine))]
    public class SecretTriggerEventDefine : EventDefine, IDescribableEventDefine, ICardEventDefine
    {
        public SecretTriggerEventDefine()
        {
            beforeVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo(){name = VAR_CARD,type=typeof(Card)}
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo(){name = VAR_CARD,type=typeof(Card)}
            };
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setCardState(VAR_CARD, arg);
            record.setVar(VAR_CARD_DEFINE, arg);
            record.setVar(VAR_BUFF, arg);
            record.setVar(VAR_EFFECT_INDEX, arg);
            record.setVar(VAR_EVENT_ARG, arg);
        }
        public override async Task execute(IEventArg arg)
        {
            var game = arg.game;
            var card = arg.getVar<Card>(VAR_CARD);
            var cardDefine = arg.getVar<CardDefine>(VAR_CARD_DEFINE);
            var buff = arg.getVar<Buff>(VAR_BUFF);
            var eventArg = arg.getVar<EventArg>(VAR_EVENT_ARG);
            var effectIndex = arg.getVar<int>(VAR_EFFECT_INDEX);
            var effects = getEffectArray(cardDefine, buff);
            if (effects == null || effectIndex < 0 || effectIndex >= effects.Length)
                return;
            var effect = effects[effectIndex];
            if (effect is ITriggerEventEffect triggerEvent)
            {
                game.logger.logTrace($"触发陷阱{card}");
                await triggerEvent.execute(game, card, buff, eventArg);
            }

            Player player = card.getOwner();
            await player.getHead().moveTo(game, card, player.getGrave());
        }
        public override string toString(EventArg arg)
        {
            return string.Format(localizeTemplateString(), localizeStringArgs(arg));
        }
        public static Task<EventArg> doEvent(CardEngine game, Card source, CardDefine cardDefine, EventArg eventArg, ITriggerEventEffect effect)
        {
            var arg = newEvent(game, source, eventArg);

            arg.setVar(VAR_CARD_DEFINE, cardDefine);
            arg.setVar(VAR_EFFECT_INDEX, Array.IndexOf(cardDefine.getEffects(), effect));
            return game.triggers.doEvent(arg);
        }
        public static Task<EventArg> doEvent(CardEngine game, Card source, Buff buff, EventArg eventArg, ITriggerEventEffect effect)
        {
            if (buff == null) // 没有设置增益，直接使用卡牌的定义中的效果。
                return doEvent(game, source, source.define, eventArg, effect);

            var arg = newEvent(game, source, eventArg);

            arg.setVar(VAR_BUFF, buff);
            arg.setVar(VAR_EFFECT_INDEX, Array.IndexOf(buff.getEffects(), effect));
            return game.triggers.doEvent(arg);
        }
        public static EventArg newEvent(CardEngine game, Card card, EventArg eventArg)
        {
            var define = game.getEventDefine<SecretTriggerEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(VAR_CARD, card);
            arg.setVar(VAR_EVENT_ARG, eventArg);
            return arg;
        }
        #region 私有方法

        #region 接口实现
        ICard ICardEventDefine.getCard(IEventArg arg)
        {
            return arg.getVar<Card>(VAR_CARD);
        }
        DescribableCard IDescribableEventDefine.getCard(IGame game, EventArg eventArg, IPlayer viewer)
        {
            return eventArg.getVar<Card>(VAR_CARD);
        }
        DescribableCard[] IDescribableEventDefine.getTargets(IGame game, EventArg eventArg, IPlayer viewer)
        {
            return null;
        }
        string IDescribableEventDefine.toString(IGame game, EventArg eventArg, IPlayer viewer) => toString(eventArg);
        string IDescribableEventDefine.localizeTemplateString(IGame game, EventArg eventArg, IPlayer viewer) => localizeTemplateString();
        object[] IDescribableEventDefine.localizeStringArgs(IGame game, EventArg eventArg, IPlayer viewer) => localizeStringArgs(eventArg);
        Player IDescribableEventDefine.getPlayer(IGame game, EventArg eventArg) => eventArg.getVar<Card>(VAR_CARD).getOwner();
        string IDescribableEventDefine.localizeLogItemNameTemplate(IGame game, EventArg eventArg) => LOG_ITEM_NAME;
        object[] IDescribableEventDefine.localizeLogItemNameArgs(IGame game, EventArg eventArg) => null;
        #endregion

        private string localizeTemplateString()
        {
            return TEMPLATE;
        }
        private object[] localizeStringArgs(EventArg eventArg)
        {
            var card = eventArg.getVar<Card>(VAR_CARD);
            return new object[] { card.getFormatString() };
        }
        private IEffect[] getEffectArray(CardDefine cardDefine, Buff buff)
        {
            return buff != null ? buff.getEffects() : cardDefine.getEffects();
        }

        #endregion

        [TranslateMsg("{0}是卡")]
        const string TEMPLATE = "陷阱 {0} 被触发";
        [TranslateMsg("游戏事件日志项名称")]
        private const string LOG_ITEM_NAME = "陷阱";
        public static string VAR_CARD => EVENTVAR.CARD;
        public const string VAR_BUFF = "效果所属增益";
        public const string VAR_CARD_DEFINE = "效果所属卡牌定义";
        public const string VAR_EFFECT_INDEX = "效果索引";
        public const string VAR_EVENT_ARG = "事件";
    }
}
