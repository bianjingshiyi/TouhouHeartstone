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
                new EventVariableInfo() { name = VAR_CARD, type = typeof(Card) },
                new EventVariableInfo() { name = VAR_BEFORE_VALUE, type = typeof(int) },
                new EventVariableInfo() { name = VAR_VALUE, type = typeof(int) }
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = VAR_CARD, type = typeof(Card) },
                new EventVariableInfo() { name = VAR_BEFORE_VALUE, type = typeof(int) },
                new EventVariableInfo() { name = VAR_VALUE, type = typeof(int) }
            };
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setCardState(VAR_CARD, arg);
            record.setVar(VAR_VALUE, arg);
            record.setVar(VAR_BEFORE_VALUE, arg);
        }
        public override Task execute(IEventArg arg)
        {
            var game = arg.game;
            var card = arg.getVar<Card>(VAR_CARD);
            var value = arg.getVar<int>(VAR_VALUE);
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
            arg.setVar(VAR_CARD, card);
            arg.setVar(VAR_VALUE, value);
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
            return eventArg.getVar<Card>(VAR_CARD);
        }
        DescribableCard[] IDescribableEventDefine.getTargets(IGame game, EventArg eventArg, IPlayer viewer)
        {
            return null;
        }
        ICard ICardEventDefine.getCard(IEventArg eventArg)
        {
            return eventArg.getVar<Card>(VAR_CARD);
        }
        string IDescribableEventDefine.toString(IGame game, EventArg eventArg, IPlayer viewer) => toString(eventArg);
        string IDescribableEventDefine.localizeTemplateString(IGame game, EventArg eventArg, IPlayer viewer) => localizeTemplateString(eventArg);
        object[] IDescribableEventDefine.localizeStringArgs(IGame game, EventArg eventArg, IPlayer viewer) => localizeStringArgs(eventArg);
        Player IDescribableEventDefine.getPlayer(IGame game, EventArg eventArg) => eventArg.getVar<Card>(VAR_CARD).getOwner();
        string IDescribableEventDefine.localizeLogItemNameTemplate(IGame game, EventArg eventArg) => Translations.EVENT_LOG_ITEM_NAME_ARMOR;
        object[] IDescribableEventDefine.localizeLogItemNameArgs(IGame game, EventArg eventArg) => new object[] { eventArg.getVar<int>(VAR_VALUE) };
        #endregion

        private string localizeTemplateString(EventArg eventArg)
        {
            return TEMPLATE;
        }
        private object[] localizeStringArgs(EventArg eventArg)
        {
            var card = eventArg.getVar<Card>(VAR_CARD);
            var value = eventArg.getVar<int>(VAR_VALUE);
            return new object[] {
                card.getFormatString(),
                value
            };
        }
        #endregion

        [TranslateMsg("{0}是卡，{1}是护甲值")]
        const string TEMPLATE = "{0} 的护甲变为 {1}";

        public const string VAR_CARD = EVENTVAR.CARD;
        public const string VAR_VALUE = EVENTVAR.VALUE;
        public const string VAR_BEFORE_VALUE = EVENTVAR.BEFORE_VALUE;
    }
}
