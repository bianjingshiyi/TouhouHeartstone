using System;
using System.Threading.Tasks;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    [EventChildren(typeof(EnterFieldEventDefine))]
    public class RebornEventDefine : EventDefine, IDescribableEventDefine, ICardEventDefine
    {
        public RebornEventDefine()
        {
            beforeVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)},
                new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card)},
                new EventVariableInfo(){name = EVENTVAR.POSITION,type=typeof(int)}
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)},
                new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card)},
                new EventVariableInfo(){name = EVENTVAR.POSITION,type=typeof(int)}
            };
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setVar(EVENTVAR.PLAYER, arg);
            record.setCardState(EVENTVAR.CARD, arg);
            record.setVar(EVENTVAR.POSITION, arg);
        }
        public override Task execute(IEventArg arg)
        {
            var game = arg.game;
            var player = arg.getVar<Player>(EVENTVAR.PLAYER);
            var card = arg.getVar<Card>(EVENTVAR.CARD);
            var position = arg.getVar<int>(EVENTVAR.POSITION);
            game.logger.logTrace($"{player}复活{card.define.GetType().Name}位于{position}");
            return player.tryEnterField(game, player.getGrave(), card, position);
        }

        public override string toString(EventArg arg)
        {
            return string.Format(localizeTemplateString(), localizeStringArgs(arg));
        }
        public static Task<EventArg> doEvent(CardEngine game, Player player, Card card, int position)
        {
            var define = game.getEventDefine<RebornEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(EVENTVAR.PLAYER, player);
            arg.setVar(EVENTVAR.CARD, card);
            arg.setVar(EVENTVAR.POSITION, position);
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
            return new object[]
            {
                player.name,
                card.getFormatString()
            };
        }

        #endregion

        [TranslateMsg("{0}是玩家，{1}是卡")]
        const string TEMPLATE = "{0} 复活了 {1}";
        [TranslateMsg("游戏事件日志项名称")]
        private const string LOG_ITEM_NAME = "复活";
    }
}
