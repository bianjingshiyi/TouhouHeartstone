using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    /// <summary>
    /// 群体添加增益事件
    /// </summary>
    [EventChildren(typeof(AddBuffEventDefine))]
    public class MassAddBuffEventDefine : EventDefine, IMassCardEventDefine
    {
        public MassAddBuffEventDefine()
        {
            beforeVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = VAR_CARD, type = typeof(Card[]) },
                new EventVariableInfo() { name = VAR_BUFF_DEFINE, type = typeof(BuffDefine) },
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = VAR_CARD, type = typeof(Card[]) },
                new EventVariableInfo() { name = VAR_BUFF_DEFINE, type = typeof(BuffDefine) },
                new EventVariableInfo() { name = VAR_BUFF, type = typeof(Buff[]) },
            };
        }
        ICard[] IMassCardEventDefine.getCards(IEventArg arg)
        {
            return arg.getVar<Card[]>(VAR_CARD);
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setCardStates(VAR_CARD, arg);
            record.setVar(VAR_BUFF_DEFINE, arg);
            record.setVar(VAR_BUFF, arg);
        }
        public override async Task execute(IEventArg arg)
        {
            var game = arg.game;
            var cards = arg.getVar<Card[]>(VAR_CARD);
            var buffDefine = arg.getVar<BuffDefine>(VAR_BUFF_DEFINE);
            List<Buff> buffList = new List<Buff>();
            foreach (var card in cards)
            {
                if (card == null)
                    continue;
                Buff buff = new Buff(buffDefine);
                buffList.Add(buff);
                await card.addBuff(game, buff);
            }
            arg.setVar(VAR_BUFF, buffList.ToArray());
        }
        public override string toString(EventArg arg)
        {
            var cards = arg.getVar<Card[]>(VAR_CARD);
            var buffDefine = arg.getVar<BuffDefine>(VAR_BUFF_DEFINE);
            var cardString = string.Join<Card>(",", cards);
            return $"为{cardString}添加增益{buffDefine}";
        }
        public static Task<EventArg> doEvent(CardEngine game, Card[] cards, BuffDefine buffDefine)
        {
            var define = game.getEventDefine<MassAddBuffEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(VAR_CARD, cards);
            arg.setVar(VAR_BUFF_DEFINE, buffDefine);
            return game.triggers.doEvent(arg);
        }
        public const string VAR_CARD = EVENTVAR.CARD;
        public const string VAR_BUFF = EVENTVAR.BUFF;
        public const string VAR_BUFF_DEFINE = EVENTVAR.BUFF_DEFINE;
    }
}
