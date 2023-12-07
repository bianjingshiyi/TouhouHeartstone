using System;
using System.Threading.Tasks;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    [EventChildren(typeof(CardPropChangeEventDefine))]
    public class ShackleEventDefine : EventDefine, IDescribableEventDefine, ICardEventDefine
    {
        public ShackleEventDefine()
        {
            beforeVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = EVENTVAR.CARD,type = typeof(Card)}
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = EVENTVAR.CARD,type = typeof(Card)}
            };
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setCardState(EVENTVAR.CARD, arg);
        }
        public override async Task execute(IEventArg arg)
        {
            var game = arg.game;
            Card card = arg.getVar<Card>(EVENTVAR.CARD);
            await card.addKeywords(game, KEYWORD.FROZEN);
            await card.addKeywords(game, KEYWORD.IS_FROZEN);
        }
        public override string toString(EventArg arg)
        {
            return string.Format(localizeTemplateString(), localizeStringArgs(arg));
        }
        public static Task<EventArg> doEvent(CardEngine game, Card card)
        {
            var define = game.getEventDefine<ShackleEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(EVENTVAR.CARD, card);
            return game.triggers.doEvent(arg);
        }
        #region 私有方法

        #region 接口实现
        ICard ICardEventDefine.getCard(IEventArg arg)
        {
            return arg.getVar<Card>(EVENTVAR.CARD);
        }
        DescribableCard IDescribableEventDefine.getCard(IGame game, EventArg eventArg, IPlayer viewer)
        {
            return eventArg.getVar<Card>(EVENTVAR.CARD);
        }
        DescribableCard[] IDescribableEventDefine.getTargets(IGame game, EventArg eventArg, IPlayer viewer)
        {
            return null;
        }
        string IDescribableEventDefine.toString(IGame game, EventArg eventArg, IPlayer viewer) => toString(eventArg);
        string IDescribableEventDefine.localizeTemplateString(IGame game, EventArg eventArg, IPlayer viewer) => localizeTemplateString();
        object[] IDescribableEventDefine.localizeStringArgs(IGame game, EventArg eventArg, IPlayer viewer) => localizeStringArgs(eventArg);
        Player IDescribableEventDefine.getPlayer(IGame game, EventArg eventArg) => eventArg.getVar<Card>(EVENTVAR.CARD).getOwner();
        string IDescribableEventDefine.localizeLogItemNameTemplate(IGame game, EventArg eventArg) => LOG_ITEM_NAME;
        object[] IDescribableEventDefine.localizeLogItemNameArgs(IGame game, EventArg eventArg) => null;
        #endregion

        private string localizeTemplateString()
        {
            return TEMPLATE;
        }
        private object[] localizeStringArgs(EventArg eventArg)
        {
            var card = eventArg.getVar<Card>(EVENTVAR.CARD);
            return new object[] { card.getFormatString() };
        }

        #endregion
        [TranslateMsg("{0}是卡牌列表")]
        const string TEMPLATE = "{0} 被禁锢";
        [TranslateMsg("游戏事件日志项名称")]
        private const string LOG_ITEM_NAME = "禁锢";
    }
}
