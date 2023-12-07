using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    /// <summary>
    /// 分配伤害事件
    /// </summary>
    [EventChildren(typeof(DamageEventDefine))]
    public class SplitDamageEventDefine : EventDefine, IMassCardEventDefine
    {
        public SplitDamageEventDefine()
        {
            beforeVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = EVENTVAR.CARD, type = typeof(Card[]) },
                new EventVariableInfo() { name = VAR_SOURCE, type = typeof(Card) },
                new EventVariableInfo() { name = EVENTVAR.VALUE, type = typeof(int) },
            };
            afterVariableInfos = new EventVariableInfo[]
            {
                new EventVariableInfo() { name = EVENTVAR.CARD, type = typeof(Card[]) },
                new EventVariableInfo() { name = VAR_SOURCE, type = typeof(Card) },
                new EventVariableInfo() { name = EVENTVAR.VALUE, type = typeof(int) },
                new EventVariableInfo() { name = VAR_DAMAGED_CARDS, type = typeof(Card[]) },
            };
        }
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
            record.setCardStates(EVENTVAR.CARD, arg);
            record.setCardState(VAR_SOURCE, arg);
            record.setVar(EVENTVAR.VALUE, arg);
            record.setCardStates(VAR_DAMAGED_CARDS, arg);
        }
        public override async Task execute(IEventArg arg)
        {
            var game = arg.game;
            var source = arg.getVar<Card>(VAR_SOURCE);
            var cards = arg.getVar<Card[]>(EVENTVAR.CARD);
            var value = arg.getVar<int>(EVENTVAR.VALUE);
            List<Card> hit = new List<Card>();
            for (int i = 0; i < value; i++)
            {
                Card target = cards.Where(c => !c.isDead(game)).random(game);
                if (target == null)
                    continue;
                var damageArg = await target.damage(game, source, 1);
                hit.Add(damageArg.getVar<Card>(EVENTVAR.CARD));
            }
            arg.setVar(VAR_DAMAGED_CARDS, hit.ToArray());
        }
        public override string toString(EventArg arg)
        {
            var source = arg.getVar<Card>(VAR_SOURCE);
            var cards = arg.getVar<Card[]>(EVENTVAR.CARD);
            var value = arg.getVar<int>(EVENTVAR.VALUE);
            var cardString = string.Join<Card>(",", cards);
            return $"{source} 对 {cardString} 随机分配 {value} 点伤害";
        }
        public static Task<EventArg> doEvent(CardEngine game, Card source, Card[] targets, int value)
        {
            var define = game.getEventDefine<SplitDamageEventDefine>();
            var arg = new EventArg(game, define);
            arg.setVar(VAR_SOURCE, source);
            arg.setVar(EVENTVAR.CARD, targets);
            arg.setVar(EVENTVAR.VALUE, value);
            return game.triggers.doEvent(arg);
        }
        ICard[] IMassCardEventDefine.getCards(IEventArg eventArg)
        {
            return eventArg.getVar<Card[]>(VAR_DAMAGED_CARDS);
        }
        public const string VAR_SOURCE = "伤害源";
        public const string VAR_DAMAGED_CARDS = "击中的卡牌";
    }
}
