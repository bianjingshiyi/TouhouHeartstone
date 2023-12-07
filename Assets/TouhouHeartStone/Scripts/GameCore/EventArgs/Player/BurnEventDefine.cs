using System;
using System.Threading.Tasks;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    [EventChildren(typeof(MoveCardEventDefine))]
    public class BurnEventDefine : EventDefine, IDescribableEventDefine, ICardEventDefine
    {
        public BurnEventDefine()
        {
            beforeVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() {name = EVENTVAR.PLAYER,type=typeof(Player)},
                new EventVariableInfo() {name = EVENTVAR.CARD,type=typeof(Card)}
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() {name = EVENTVAR.PLAYER,type=typeof(Player)},
                new EventVariableInfo() {name = EVENTVAR.CARD,type=typeof(Card)}
            };
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setVar(EVENTVAR.PLAYER, arg);
            record.setCardState(EVENTVAR.CARD, arg);
        }
        public override Task execute(IEventArg arg)
        {
            var game = arg.game;
            var player = arg.getVar<Player>(EVENTVAR.PLAYER);
            var card = arg.getVar<Card>(EVENTVAR.CARD);
            game.logger.logTrace($"{player}的手牌已经满了，{card}被送入墓地");
            return player.getDeck().moveTo(game, card, player.getGrave(), player.getGrave().count);
        }

        public override string toString(EventArg arg)
        {
            return string.Format(localizeTemplateString(), localizeStringArgs(arg));
        }
        public static EventArg newEvent(CardEngine game, Player player, Card card)
        {
            var define = game.getEventDefine<BurnEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(EVENTVAR.PLAYER, player);
            arg.setVar(EVENTVAR.CARD, card);
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
            return null;
        }
        ICard ICardEventDefine.getCard(IEventArg arg)
        {
            var card = arg.getVar<Card>(EVENTVAR.CARD);
            return card;
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

        [TranslateMsg("{0}是玩家，{1}是手牌")]
        const string TEMPLATE = "{0}抽{1}，但是由于手牌已满它被送入墓地";
        [TranslateMsg("游戏事件日志项名称")]
        private const string LOG_ITEM_NAME = "爆牌";
    }
}
