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
        #region 公有方法

        #region 卡牌移动
        public static Task<EventArg> add(this Pile pile, CardEngine game, Card card)
        {
            return pile.insert(game, card, pile.count);
        }
        public static Task<EventArg> add(this Pile pile, CardEngine game, IEnumerable<Card> cards)
        {
            return MassMoveCardEventDefine.doEvent(game, cards.ToArray(), pile);
        }
        public static Task<EventArg> remove(this Pile pile, CardEngine game, Card card)
        {
            return moveTo(game, card, pile, null, 0);
        }
        public static Task<EventArg> insert(this Pile pile, CardEngine game, Card card, int position)
        {
            return moveTo(game, card, null, pile, position);
        }
        /// <summary>
        /// 将位于该牌堆中的一张牌移动到其他的牌堆中。
        /// </summary>
        /// <param name="card"></param>
        /// <param name="to"></param>
        /// <param name="position"></param>
        public static Task<EventArg> moveTo(this Pile pile, CardEngine game, Card card, Pile to, int position)
        {
            return moveTo(game, card, pile, to, position);
        }
        public static Task<EventArg> moveTo(this Pile pile, CardEngine game, Card card, Pile targetPile)
        {
            return moveTo(game, card, pile, targetPile, targetPile.count);
        }
        public static Task<EventArg> moveTo(this Pile pile, CardEngine game, IEnumerable<Card> cards, Pile targetPile)
        {
            return MassMoveCardEventDefine.doEvent(game, cards.Reverse().ToArray(), pile, targetPile);
        }
        public static Task<EventArg> moveTo(this Pile pile, CardEngine game, IEnumerable<Card> cards, Pile targetPile, int position)
        {
            return MassMoveCardEventDefine.doEvent(game, cards.Reverse().ToArray(), pile, targetPile, position);
        }
        public static async Task<EventArg> moveTo(CardEngine game, Card card, Pile from, Pile to, int position)
        {
            if (!Pile.canMove(card, from, to))
                return null;
            // 目标牌堆和源牌堆不是牌堆，更新卡牌可见性。
            if (from != to)
            {
                // 这里的一个例外是陷阱，陷阱在被置入暂存区或者延迟区时不会取消它对某个玩家的可见性，用于透露某个已经被知晓的陷阱被使用。
                // 但是一个本身不可见的陷阱，在被挂载后需要开启对自己的可见性，以防通过衍生物创建的陷阱不能被自己查看。
                // 也就是说，挂载陷阱这个行为不能向任意玩家隐藏它的可见性，但是可以向其拥有者显示自己。
                bool isUsingTrap = card.isTrap(game) && (to?.name == PILE.WARP || to?.name == PILE.HEAD);
                foreach (var player in game.players)
                {
                    bool shouldVisible = to.isVisiblePileTo(player);
                    if (isUsingTrap)
                    {
                        if (!shouldVisible)
                            continue;
                        if (player != to?.owner)
                            continue;
                    }
                    await card.setCardVisibleTo(game, player, shouldVisible);
                }
            }
            return await MoveCardEventDefine.doEvent(game, from, to, card, position, true);
        }
        public static async Task replace(this Pile pile, CardEngine game, Card origin, Card target)
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
        #endregion

        #region 牌堆判断

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

        #endregion

        #region 属性重置
        /// <summary>
        /// 检测一次卡牌移动是否会重置卡牌的所有属性。
        /// </summary>
        /// <param name="from">源牌堆。</param>
        /// <param name="to">目标牌堆。</param>
        /// <returns></returns>
        public static bool shouldCardMoveResetCardProp(Pile from, Pile to)
        {
            int fromPriority = from.getPilePropResetPriority();
            int toPriority = to.getPilePropResetPriority();
            if (fromPriority < 0 || toPriority < 0)
                return false;
            return fromPriority > toPriority;
        }
        /// <summary>
        /// 检测一次卡牌移动是否会重置战场属性。
        /// </summary>
        /// <param name="from">源牌堆。</param>
        /// <param name="to">目标牌堆。</param>
        /// <returns></returns>
        public static bool shouldCardMoveResetBattlefieldProp(Pile from, Pile to)
        {
            bool fromField = from.isGeneralizedBattlefield();
            bool toField = to.isGeneralizedBattlefield();
            // 广义战场区域内移动不重置。
            // 从广义战场移动到其他牌堆，或者从其他牌堆移动到广义战场，就重置战场属性。
            return !fromField || !toField;
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
        public static async Task tryResetCardPropsAfterMove(IGame game, bool resetPropsForPriorities, Card card, Pile from, Pile to)
        {
            var engine = game as CardEngine;
            if (resetPropsForPriorities && shouldCardMoveResetCardProp(from, to))
            {
                await card.resetCard(engine);
                return;
            }
            if (shouldCardMoveResetBattlefieldProp(from, to))
            {
                // 重置战场属性。
                await card.resetBattlefieldProps(engine);
                // 进入战场后才会初始化一次性关键词。
                if (to.isGeneralizedBattlefield())
                {
                    await card.InitConsumableKeywords(game);
                }
            }
        }
        #endregion

        /// <summary>
        /// 牌堆内的卡牌是否应当对某玩家可见。
        /// </summary>
        /// <param name="pile"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool isVisiblePileTo(this Pile pile, Player player)
        {
            if (pile.isGeneralizedBattlefield() || pile?.name == PILE.GRAVE || pile?.name == PILE.WARP)
            {
                return true;
            }
            else if (pile?.name == PILE.HAND || pile?.name == PILE.INIT)
            {
                return player == pile.owner;
            }
            return false;
        }

        #endregion

        #region 私有方法

        #region 属性重置
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
        #endregion

        #endregion

        #region 属性字段
        public static string[] vanillaPileNames = new string[]
        {
            PILE.MASTER,
            PILE.INIT,
            PILE.HAND,
            PILE.SKILL,
            PILE.DECK,
            PILE.FIELD,
            PILE.GRAVE,
            PILE.WARP,
            PILE.ITEM,
            PILE.HEAD
        };
        #endregion
    }
}
