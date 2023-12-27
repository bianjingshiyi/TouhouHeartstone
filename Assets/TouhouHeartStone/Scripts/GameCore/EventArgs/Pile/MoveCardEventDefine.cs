using System;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{

    [EventChildren(typeof(SetCardPropEventDefine), typeof(ChangeCardVisibilityEventDefine))]
    public class MoveCardEventDefine : EventDefine, ICardEventDefine
    {
        public MoveCardEventDefine()
        {
            beforeVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = VAR_FROM_PILE, type = typeof(Pile) },
                new EventVariableInfo() { name = VAR_TO_PILE, type = typeof(Pile) },
                new EventVariableInfo() { name = VAR_CARD, type = typeof(Card) },
                new EventVariableInfo() { name = VAR_POSITION, type = typeof(int) },
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = VAR_FROM_PILE, type = typeof(Pile) },
                new EventVariableInfo() { name = VAR_TO_PILE, type = typeof(Pile) },
                new EventVariableInfo() { name = VAR_CARD, type = typeof(Card) },
                new EventVariableInfo() { name = VAR_POSITION, type = typeof(int) },
                new EventVariableInfo() { name = VAR_SUCCESS, type = typeof(bool) },
            };
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setVar(VAR_FROM_PILE, arg);
            record.setVar(VAR_TO_PILE, arg);
            record.setCardState(VAR_CARD, arg);
            record.setVar(VAR_POSITION, arg);
            record.setVar(VAR_SUCCESS, arg);
        }
        public override async Task execute(IEventArg arg)
        {
            var game = arg.game;
            var from = arg.getVar<Pile>(VAR_FROM_PILE);
            var to = arg.getVar<Pile>(VAR_TO_PILE);
            var card = arg.getVar<Card>(VAR_CARD);
            var position = arg.getVar<int>(VAR_POSITION);
            var resetProps = arg.getVar<bool>(VAR_RESET_PROPS);

            var success = Pile.moveCard(game, card, from, to, position);
            arg.setVar(VAR_SUCCESS, success);
            if (success)
            {
                await THHPile.tryResetCardPropsAfterMove(game, resetProps, card, from, to);
            }
            else
            {
                arg.isCanceled = true;
            }
        }
        public override string toString(EventArg eventArg)
        {
            var from = eventArg.getVar<Pile>(VAR_FROM_PILE);
            var to = eventArg.getVar<Pile>(VAR_TO_PILE);
            var card = eventArg.getVar<Card>(VAR_CARD);
            var position = eventArg.getVar<int>(VAR_POSITION);
            return $"将{card}从{from}移动到{to}的{position}";
        }
        public static Task<EventArg> doEvent(CardEngine game, Pile from, Pile to, Card card, int position, bool resetProps = true)
        {
            var define = game.getEventDefine<MoveCardEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(VAR_FROM_PILE, from);
            arg.setVar(VAR_TO_PILE, to);
            arg.setVar(VAR_CARD, card);
            arg.setVar(VAR_POSITION, position);
            arg.setVar(VAR_RESET_PROPS, resetProps);
            return game.triggers.doEvent(arg);
        }
        ICard ICardEventDefine.getCard(IEventArg eventArg)
        {
            return eventArg.getVar<Card>(VAR_CARD);
        }
        public static string VAR_CARD => EVENTVAR.CARD;
        public static string VAR_FROM_PILE => EVENTVAR.FROM_PILE;
        public static string VAR_POSITION => EVENTVAR.POSITION;
        public const string VAR_TO_PILE = "目标牌堆";
        public const string VAR_SUCCESS = "是否成功";
        public const string VAR_RESET_PROPS = "重置属性";
    }
}
