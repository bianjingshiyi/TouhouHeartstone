using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine.Interfaces;
using TouhouCardEngine;

namespace TouhouHeartstone
{
    public static class THHPile
    {
        public static Task add(this Pile pile, IGame game, Card card)
        {
            return pile.insert(game, card, pile.count);
        }
        public static async Task add(this Pile pile, IGame game, IEnumerable<Card> cards)
        {
            foreach (var card in cards)
            {
                await pile.add(game, card);
            }
        }
        public static Task remove(this Pile pile, IGame game, Card card)
        {
            return moveTo(game, card, pile, null, 0);
        }
        public static Task insert(this Pile pile, IGame game, Card card, int position)
        {
            return moveTo(game, card, null, pile, position);
        }
        /// <summary>
        /// 将位于该牌堆中的一张牌移动到其他的牌堆中。
        /// </summary>
        /// <param name="card"></param>
        /// <param name="to"></param>
        /// <param name="position"></param>
        public static Task moveTo(this Pile pile, IGame game, Card card, Pile to, int position)
        {
            return moveTo(game, card, pile, to, position);
        }
        public static Task moveTo(this Pile pile, IGame game, Card card, Pile targetPile)
        {
            return moveTo(game, card, pile, targetPile, targetPile.count);
        }
        public static async Task moveTo(this Pile pile, IGame game, IEnumerable<Card> cards, Pile targetPile)
        {
            foreach (var card in cards.Reverse())
            {
                await pile.moveTo(game, card, targetPile);
            }
        }
        public static async Task moveTo(this Pile pile, IGame game, IEnumerable<Card> cards, Pile targetPile, int position)
        {
            foreach (var card in cards.Reverse())
            {
                await moveTo(game, card, pile, targetPile, position);
            }
        }
        public static async Task moveTo(IGame game, Card card, Pile from, Pile to, int position)
        {
            var eventArg = new MoveCardEventArg(from, to, card, position);
            if (game != null)
            {
                async Task func(MoveCardEventArg arg)
                {
                    var argFrom = arg.from;
                    var argTo = arg.to;
                    var argCard = arg.card;
                    var argPosition = arg.position;

                    var success = Pile.moveCard(game, argCard, argFrom, argTo, argPosition, arg);
                    arg.success = success;
                    if (success)
                    {
                        await tryResetCardPropsAfterMove(game, arg.resetProps, card, from, to);
                    }
                }
                await game.triggers.doEvent(eventArg, func);
            }
            else
            {
                var success = Pile.moveCard(card, from, to, position, eventArg);
                eventArg.success = success;
                if (success)
                {
                    await tryResetCardPropsAfterMove(game, eventArg.resetProps, card, from, to);
                }
            }
        }
        public static async Task replace(this Pile pile, IGame game, Card origin, Card target)
        {
            int originIndex = pile.indexOf(origin);
            int targetIndex = target.pile.indexOf(target);
            await moveTo(game, origin, pile, target.pile, targetIndex);
            await moveTo(game, target, target.pile, pile, originIndex);
        }
        /// <summary>
        /// 将牌堆中的一些牌与目标牌堆中随机的一些牌相替换。
        /// </summary>
        /// <param name="engine">用于提供随机功能的引擎</param>
        /// <param name="originalCards">要进行替换的卡牌</param>
        /// <param name="pile">目标牌堆</param>
        /// <returns>返回替换原有卡牌的卡牌数组，顺序与替换的顺序相同</returns>
        public static async Task<Card[]> replaceByRandom(this Pile pile, CardEngine engine, Card[] originalCards, Pile to)
        {
            Card[] replaced = new Card[originalCards.Length];
            for (int i = 0; i < originalCards.Length; i++)
            {
                replaced[i] = to.getCardByRandom(engine);
                await pile.replace(engine, originalCards[i], replaced[i]);
            }
            return replaced;
        }
        /// <summary>
        /// 牌堆是否是“广义战场”。
        /// “广义战场”包括战场、英雄、延迟区、技能、道具和暂存区。
        /// </summary>
        /// <param name="pile"></param>
        /// <returns></returns>
        public static bool isGeneralizedBattlefield(this Pile pile)
        {
            switch (pile?.name)
            {
                case PILE.FIELD:
                case PILE.MASTER:
                case PILE.HEAD:
                case PILE.SKILL:
                case PILE.ITEM:
                case PILE.WARP:
                    return true;
            }
            return false;
        }
        private static int getPilePropResetPriority(this Pile pile)
        {
            switch (pile?.name)
            {
                case PILE.GRAVE:
                    return 0;
                case PILE.DECK:
                    return 1;
                case PILE.HAND:
                    return 2;
                case PILE.FIELD:
                case PILE.MASTER:
                case PILE.HEAD:
                case PILE.SKILL:
                case PILE.ITEM:
                case PILE.WARP:
                    return 3;
            }
            return -1;
        }
        private static bool shouldCardMoveResetCardProp(Pile from, Pile to)
        {
            int fromPriority = from.getPilePropResetPriority();
            int toPriority = to.getPilePropResetPriority();
            if (fromPriority < 0 || toPriority < 0)
                return false;
            return fromPriority > toPriority;
        }
        /// <summary>
        /// 在卡牌从牌堆间移动成功后，尝试重置卡牌的属性。
        /// </summary>
        /// <param name="game">游戏实例。</param>
        /// <param name="resetPropsForPriorities">是否会在牌堆逆向移动时重置所有属性？</param>
        /// <param name="card">卡牌。</param>
        /// <param name="from">移动源牌堆。</param>
        /// <param name="to">移动目标牌堆。</param>
        /// <returns></returns>
        private static async Task tryResetCardPropsAfterMove(IGame game, bool resetPropsForPriorities, Card card, Pile from, Pile to)
        {
            bool shouldCheckBattlefieldProps = true;
            if (resetPropsForPriorities)
            {
                bool reseted = await resetCardMoveProps(game, card, from, to);
                if (reseted)
                {
                    shouldCheckBattlefieldProps = false;
                }
            }
            if (shouldCheckBattlefieldProps)
            {
                await checkBattlefieldProps(game as CardEngine, card, from, to);
            }
        }
        private static async Task<bool> resetCardMoveProps(IGame game, Card card, Pile from, Pile to)
        {
            if (!shouldCardMoveResetCardProp(from, to))
                return false;
            await card.resetCard(game as CardEngine);
            return true;
        }
        /// <summary>
        /// 检查卡牌的“战场专有属性”是否需要被重置。
        /// “战场专有属性”只会在“广义战场”内移动时不会被重置。
        /// </summary>
        /// <param name="game"></param>
        /// <param name="card"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        private static async Task checkBattlefieldProps(CardEngine game, Card card, Pile from, Pile to)
        {
            bool fromField = from.isGeneralizedBattlefield();
            bool toField = to.isGeneralizedBattlefield();
            if (fromField && toField) // 广义战场区域内移动。
                return;
            // 从广义战场移动到其他牌堆，或者从其他牌堆移动到广义战场。
            // 重置战场属性。
            await card.resetBattlefieldProps(game);
            // 进入战场后才会初始化一次性关键词。
            if (toField)
            {
                await card.InitConsumableKeywords(game);
            }
        }
        public class MoveCardEventArg : EventArg
        {
            public MoveCardEventArg()
            {
            }
            public MoveCardEventArg(Pile from, Pile to, Card card, int position, bool resetProps = true)
            {
                this.from = from;
                this.to = to;
                this.card = card;
                this.position = position;
                this.resetProps = resetProps;
            }
            public override string ToString()
            {
                return $"将{card}从{from}移动到{to}的{position}";
            }
            public override void Record(IGame game, EventRecord record)
            {
                record.setVar(VAR_FROM_PILE, from);
                record.setVar(VAR_TO_PILE, to);
                record.setCardState(EVENTVAR.CARD, card);
                record.setVar(VAR_POSITION, position);
                record.setVar(VAR_SUCCESS, success);
            }
            public override EventVariableInfo[] getBeforeEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo() { name = VAR_FROM_PILE, type = typeof(Pile) },
                    new EventVariableInfo() { name = VAR_TO_PILE, type = typeof(Pile) },
                    new EventVariableInfo() { name = EVENTVAR.CARD, type = typeof(Card) },
                    new EventVariableInfo() { name = VAR_POSITION, type = typeof(int) },
                };
            }
            public override EventVariableInfo[] getAfterEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo() { name = VAR_FROM_PILE, type = typeof(Pile) },
                    new EventVariableInfo() { name = VAR_TO_PILE, type = typeof(Pile) },
                    new EventVariableInfo() { name = EVENTVAR.CARD, type = typeof(Card) },
                    new EventVariableInfo() { name = VAR_POSITION, type = typeof(int) },
                    new EventVariableInfo() { name = VAR_SUCCESS, type = typeof(bool) },
                };
            }
            public Pile from
            {
                get => getVar<Pile>(VAR_FROM_PILE);
                set => setVar(VAR_FROM_PILE, value);
            }
            public Pile to
            {
                get => getVar<Pile>(VAR_TO_PILE);
                set => setVar(VAR_TO_PILE, value);
            }
            public Card card
            {
                get => getVar<Card>(EVENTVAR.CARD);
                set => setVar(EVENTVAR.CARD, value);
            }
            public int position
            {
                get => getVar<int>(VAR_POSITION);
                set => setVar(VAR_POSITION, value);
            }
            public bool success
            {
                get => getVar<bool>(VAR_SUCCESS);
                set => setVar(VAR_SUCCESS, value);
            }
            public bool resetProps;

            public const string VAR_FROM_PILE = "源牌堆";
            public const string VAR_TO_PILE = "目标牌堆";
            public const string VAR_POSITION = "目标位置";
            public const string VAR_SUCCESS = "是否成功";
        }
    }
}
