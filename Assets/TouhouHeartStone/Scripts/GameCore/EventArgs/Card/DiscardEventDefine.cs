using System;
using System.Threading.Tasks;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    [EventChildren(typeof(MoveCardEventDefine))]
    public class DiscardEventDefine : EventDefine, IDescribableEventDefine, ICardEventDefine
    {
        public DiscardEventDefine()
        {
            beforeVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)},
                new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card)}
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)},
                new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card)}
            };
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setCardState(EVENTVAR.CARD, arg);
            record.setVar(EVENTVAR.PLAYER, arg);
        }
        public override Task execute(IEventArg arg)
        {
            var game = arg.game;
            var player = arg.getVar<Player>(EVENTVAR.PLAYER);
            var card = arg.getVar<Card>(EVENTVAR.CARD);
            if (player == null || card == null)
                return Task.CompletedTask;
            return player.getHand().moveTo(game, card, player.getGrave());
        }
        public override string toString(EventArg arg)
        {
            return string.Format(localizeTemplateString(), localizeStringArgs(arg));
        }
        public static Task<EventArg> doEvent(CardEngine game, Card card)
        {
            var define = game.getEventDefine<DiscardEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(EVENTVAR.CARD, card);
            arg.setVar(EVENTVAR.PLAYER, card.getOwner());
            return game.triggers.doEvent(arg);
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
        string IDescribableEventDefine.localizeTemplateString(IGame game, EventArg eventArg, IPlayer viewer) => localizeTemplateString();
        object[] IDescribableEventDefine.localizeStringArgs(IGame game, EventArg eventArg, IPlayer viewer) => localizeStringArgs(eventArg);
        Player IDescribableEventDefine.getPlayer(IGame game, EventArg eventArg) => eventArg.getVar<Player>(EVENTVAR.PLAYER);
        string IDescribableEventDefine.localizeLogItemNameTemplate(IGame game, EventArg eventArg) => LOG_ITEM_NAME;
        object[] IDescribableEventDefine.localizeLogItemNameArgs(IGame game, EventArg eventArg) => null;
        #endregion

        private string localizeTemplateString()
        {
            return TEMPLATE;
        }
        private object[] localizeStringArgs(EventArg eventArg)
        {
            var player = eventArg.getVar<Player>(EVENTVAR.PLAYER);
            var card = eventArg.getVar<Card>(EVENTVAR.CARD);
            return new object[] { player.name, card.getFormatString() };
        }

        #endregion

        [TranslateMsg("{0}是玩家，{1}是卡牌列表")]
        const string TEMPLATE = "{0} 丢弃 {1}";
        [TranslateMsg("游戏事件日志项名称")]
        private const string LOG_ITEM_NAME = "弃牌";
    }
}
