using System;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    /// <summary>
    /// 群体变形事件
    /// </summary>
    [EventChildren(typeof(SetCardDefineEventDefine))]
    public class MassSetCardDefineEventDefine : EventDefine, IMassCardEventDefine
    {
        public MassSetCardDefineEventDefine()
        {
            beforeVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = VAR_CARD, type = typeof(Card[]) },
                new EventVariableInfo() { name = VAR_TARGET_CARD_DEFINE, type = typeof(CardDefine) },
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = VAR_CARD, type = typeof(Card[]) },
                new EventVariableInfo() { name = VAR_TARGET_CARD_DEFINE, type = typeof(CardDefine) },
            };
        }
        public override async Task execute(IEventArg arg)
        {
            var game = arg.game;
            var cards = arg.getVar<Card[]>(VAR_CARD);
            var cardDefine = arg.getVar<CardDefine>(VAR_TARGET_CARD_DEFINE);
            foreach (var card in cards)
            {
                await card.setDefine(game, cardDefine, true);
            }
        }
        public override string toString(EventArg arg)
        {
            var cards = arg.getVar<Card[]>(VAR_CARD);
            var cardDefine = arg.getVar<CardDefine>(VAR_TARGET_CARD_DEFINE);
            var cardString = string.Join<Card>(",", cards);
            return $"{cardString} 变形为 {cardDefine}";
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setCardStates(VAR_CARD, arg);
            record.setVar(VAR_TARGET_CARD_DEFINE, arg);
        }
        ICard[] IMassCardEventDefine.getCards(IEventArg eventArg)
        {
            return eventArg.getVar<Card[]>(VAR_CARD);
        }
        public const string VAR_CARD = EVENTVAR.CARD;
        public const string VAR_TARGET_CARD_DEFINE = EVENTVAR.TARGET_CARD_DEFINE;
    }
}
