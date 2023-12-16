using System;
using System.Threading.Tasks;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    [EventChildren(typeof(SetCardPropEventDefine))]
    public class SetArmorEventDefine : EventDefine, IDescribableEventDefine, ICardEventDefine
    {
        public SetArmorEventDefine()
        {
            beforeVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = EVENTVAR.CARD, type = typeof(Card) },
                new EventVariableInfo() { name = VAR_BEFORE_VALUE, type = typeof(int) },
                new EventVariableInfo() { name = EVENTVAR.VALUE, type = typeof(int) }
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = EVENTVAR.CARD, type = typeof(Card) },
                new EventVariableInfo() { name = VAR_BEFORE_VALUE, type = typeof(int) },
                new EventVariableInfo() { name = EVENTVAR.VALUE, type = typeof(int) }
            };
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setCardState(EVENTVAR.CARD, arg);
            record.setVar(EVENTVAR.VALUE, arg);
            record.setVar(VAR_BEFORE_VALUE, arg);
        }
        public override Task execute(IEventArg arg)
        {
            var game = arg.game;
            var card = arg.getVar<Card>(EVENTVAR.CARD);
            var value = arg.getVar<int>(EVENTVAR.VALUE);
            return card.setProp(game, CARD.ARMOR, value);
        }
        public override string toString(EventArg arg)
        {
            return string.Format(localizeTemplateString(arg), localizeStringArgs(arg));
        }
        public static Task<EventArg> doEvent(CardEngine game, Card card, int value, int beforeValue)
        {
            var define = game.getEventDefine<SetArmorEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(EVENTVAR.CARD, card);
            arg.setVar(EVENTVAR.VALUE, value);
            arg.setVar(VAR_BEFORE_VALUE, beforeValue);
            return game.triggers.doEvent(arg);
        }
        public static Task<EventArg> doEvent(CardEngine game, Card card, int value)
        {
            var beforeValue = card.getArmor(game);
            return doEvent(game, card, value, beforeValue);
        }
        #region 私有方法

        #region 接口实现
        DescribableCard IDescribableEventDefine.getCard(IGame game, EventArg eventArg, IPlayer viewer)
        {
            return eventArg.getVar<Card>(EVENTVAR.CARD);
        }
        DescribableCard[] IDescribableEventDefine.getTargets(IGame game, EventArg eventArg, IPlayer viewer)
        {
            return null;
        }
        ICard ICardEventDefine.getCard(IEventArg eventArg)
        {
            return eventArg.getVar<Card>(EVENTVAR.CARD);
        }
        string IDescribableEventDefine.toString(IGame game, EventArg eventArg, IPlayer viewer) => toString(eventArg);
        string IDescribableEventDefine.localizeTemplateString(IGame game, EventArg eventArg, IPlayer viewer) => localizeTemplateString(eventArg);
        object[] IDescribableEventDefine.localizeStringArgs(IGame game, EventArg eventArg, IPlayer viewer) => localizeStringArgs(eventArg);
        Player IDescribableEventDefine.getPlayer(IGame game, EventArg eventArg) => eventArg.getVar<Card>(EVENTVAR.CARD).getOwner();
        string IDescribableEventDefine.localizeLogItemNameTemplate(IGame game, EventArg eventArg) => LOG_ITEM_NAME;
        object[] IDescribableEventDefine.localizeLogItemNameArgs(IGame game, EventArg eventArg) => new object[] { eventArg.getVar<int>(EVENTVAR.VALUE) };
        #endregion

        private string localizeTemplateString(EventArg eventArg)
        {
            return TEMPLATE;
        }
        private object[] localizeStringArgs(EventArg eventArg)
        {
            var card = eventArg.getVar<Card>(EVENTVAR.CARD);
            var value = eventArg.getVar<int>(EVENTVAR.VALUE);
            return new object[] {
                card.getFormatString(),
                value
            };
        }
        #endregion

        [TranslateMsg("{0}是卡，{1}是护甲值")]
        const string TEMPLATE = "{0} 的护甲变为 {1}";
        public const string VAR_BEFORE_VALUE = "原数值";
        [TranslateMsg("游戏事件日志项名称-护甲值更改事件（{0}为当前护甲值）")]
        private const string LOG_ITEM_NAME = "=>{0}";
    }
}
