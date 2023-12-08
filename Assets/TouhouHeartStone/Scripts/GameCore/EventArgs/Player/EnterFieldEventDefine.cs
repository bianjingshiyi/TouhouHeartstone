using System;
using System.Threading.Tasks;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    [EventChildren(typeof(MoveCardEventDefine))]
    public class EnterFieldEventDefine : EventDefine, IDescribableEventDefine, ICardEventDefine
    {
        public EnterFieldEventDefine()
        {
            beforeVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)},
                new EventVariableInfo(){name = VAR_CARD,type=typeof(Card)},
                new EventVariableInfo(){name = EVENTVAR.POSITION,type=typeof(int)}
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)},
                new EventVariableInfo(){name = VAR_CARD,type=typeof(Card)},
                new EventVariableInfo(){name = EVENTVAR.POSITION,type=typeof(int)}
            };
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setVar(EVENTVAR.PLAYER, arg);
            record.setCardState(VAR_CARD, arg);
            record.setVar(EVENTVAR.POSITION, arg);
            record.setVar(EVENTVAR.FROM_PILE, arg);
        }
        public override async Task execute(IEventArg arg)
        {
            var game = arg.game;
            var player = arg.getVar<Player>(EVENTVAR.PLAYER);
            var card = arg.getVar<Card>(VAR_CARD);
            var from = arg.getVar<Pile>(EVENTVAR.FROM_PILE);
            var position = arg.getVar<int>(EVENTVAR.POSITION);


            if (from != null)
                game.logger.logTrace($"{player}将{card}从{from}置入战场，位于{position}");//从某pile召唤随从
            else
                game.logger.logTrace($"{player}将{card}置入战场，位于{position}");//虚空生成随从

            var field = player.getField();
            if (position < 0)
                position = field.count;
            var moveEvent = await card.pile.moveTo(game, card, field, position);
            if (moveEvent == null || !moveEvent.getVar<bool>(MoveCardEventDefine.VAR_SUCCESS))
            {
                arg.isCanceled = true;
                return;
            }
        }

        public override string toString(EventArg arg)
        {
            return string.Format(localizeTemplateString(), localizeStringArgs(arg));
        }
        public static Task<EventArg> doEvent(CardEngine game, Player player, Pile from, Card card, int position)
        {
            var define = game.getEventDefine<EnterFieldEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(EVENTVAR.PLAYER, player);
            arg.setVar(VAR_CARD, card);
            arg.setVar(EVENTVAR.FROM_PILE, from);
            arg.setVar(EVENTVAR.POSITION, position);
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
        string IDescribableEventDefine.localizeTemplateString(IGame game, EventArg eventArg, IPlayer viewer) => TEMPLATE_NO_POSITION;
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
            var card = eventArg.getVar<Card>(VAR_CARD);
            var position = eventArg.getVar<int>(EVENTVAR.POSITION);
            return new object[]
            {
                player.name,
                card.getFormatString(),
                position
            };
        }

        #endregion

        [TranslateMsg("{0}是玩家，{1}是卡，{2}是位置")]
        const string TEMPLATE = "{0} 将{1} 置入战场，位于 {2}";
        [TranslateMsg("{0}是玩家，{1}是卡")]
        const string TEMPLATE_NO_POSITION = "{0} 将{1} 置入战场";
        [TranslateMsg("游戏事件日志项名称")]
        private const string LOG_ITEM_NAME = "召唤";
        public static string VAR_CARD => EVENTVAR.CARD;
    }
}
