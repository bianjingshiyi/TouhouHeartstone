using System;
using System.Threading.Tasks;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    [EventChildren(typeof(MoveCardEventDefine))]
    public class EquipItemEventDefine : EventDefine, IDescribableEventDefine, ICardEventDefine
    {
        public EquipItemEventDefine()
        {
            beforeVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)},
                new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card)},
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)},
                new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card)},
            };
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setVar(EVENTVAR.PLAYER, arg);
            record.setCardState(EVENTVAR.CARD, arg);
            record.setVar(EVENTVAR.FROM_PILE, arg);
        }
        public override async Task execute(IEventArg arg)
        {
            var game = arg.game;
            var player = arg.getVar<Player>(EVENTVAR.PLAYER);
            var card = arg.getVar<Card>(EVENTVAR.CARD);
            var from = arg.getVar<Pile>(EVENTVAR.FROM_PILE);
            var to = player[PILE.ITEM];
            if (to.isFull)
            {
                game.logger.logTrace($"由于{player}的道具栏已满，摧毁当前道具");
                await player.getItem().destroy(game);
            }
            var moveEvent = await from.moveTo(game, card, to);
            if (moveEvent == null || !moveEvent.getVar<bool>(MoveCardEventDefine.VAR_SUCCESS))
            {
                arg.isCanceled = true;
                return;
            }
            game.logger.logTrace($"{player}从{from}装备了{from}");
        }

        public override string toString(EventArg arg)
        {
            return string.Format(localizeTemplateString(), localizeStringArgs(arg));
        }
        public static Task<EventArg> doEvent(CardEngine game, Player player, Pile from, Card card)
        {
            var define = game.getEventDefine<EquipItemEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(EVENTVAR.PLAYER, player);
            arg.setVar(EVENTVAR.CARD, card);
            arg.setVar(EVENTVAR.FROM_PILE, from);
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
            return new object[] { player.name, card.getFormatString(), };
        }

        #endregion

        [MukioI18n.TranslateMsg("{0}是玩家，{1}是卡")]
        const string TEMPLATE = "{0} 装备了 {1}";
        [TranslateMsg("游戏事件日志项名称")]
        private const string LOG_ITEM_NAME = "装备";
    }
}
