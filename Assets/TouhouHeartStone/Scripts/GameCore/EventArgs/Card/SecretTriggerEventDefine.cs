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
        }
        public override async Task execute(IEventArg arg)
        {
            var game = arg.game;
            var card = arg.getVar<Card>(VAR_CARD);
            var buff = arg.getVar<Buff>(VAR_BUFF);
            var effect = arg.getVar<Effect>(VAR_EFFECT);
            var envEvent = arg.getVar<EventArg>(VAR_EVENT);
            var effectEnv = new EffectEnv(game, card, buff, envEvent, effect);
            await EffectTriggerEventDefine.doEvent(effectEnv);

            Player player = card.getOwner();
            await player.getHead().moveTo(game, card, player.getGrave());
        }
        public override string toString(EventArg arg)
        {
            return string.Format(localizeTemplateString(), localizeStringArgs(arg));
        }
        public static Task<EventArg> doEvent(EffectEnv env)
        {
            var game = env.game;
            var define = game.getEventDefine<SecretTriggerEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(VAR_CARD, env.card);
            arg.setVar(VAR_BUFF, env.buff);
            arg.setVar(VAR_EFFECT, env.effect);
            arg.setVar(VAR_EVENT, env.eventArg);
            return game.triggers.doEvent(arg);
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

        #endregion

        [TranslateMsg("{0}是卡")]
        const string TEMPLATE = "陷阱 {0} 被触发";
        [TranslateMsg("游戏事件日志项名称")]
        private const string LOG_ITEM_NAME = "陷阱";
        public static string VAR_CARD => EVENTVAR.CARD;

        public const string VAR_BUFF = "增益";
        public const string VAR_EFFECT = "效果";
        public const string VAR_EVENT = "事件";
    }
}
