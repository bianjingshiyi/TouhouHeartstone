using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using System;

namespace TouhouHeartstone
{
    public static partial class THHCard
    {
        #region 公有方法

        #region 属性关键词

        #region 获取
        public static bool isTaunt(this ICardData card, IGame game)
        {
            return card.hasKeyword(game, KEYWORD.TAUNT);
        }
        public static bool isCharge(this ICardData card, IGame game)
        {
            return card.hasKeyword(game, KEYWORD.CHARGE);
        }
        public static bool isRush(this ICardData card, IGame game)
        {
            return card.hasKeyword(game, KEYWORD.RUSH);
        }
        public static bool isDrain(this ICardData card, IGame game)
        {
            return card.hasKeyword(game, KEYWORD.DRAIN);
        }
        public static bool isPoisonous(this ICardData card, IGame game)
        {
            return card.hasKeyword(game, KEYWORD.POISONOUS);
        }
        public static bool isElusive(this ICardData card, IGame game)
        {
            return card.hasKeyword(game, KEYWORD.ELUSIVE);
        }
        public static bool isWindfury(this ICardData card, IGame game)
        {
            if (card.isMaster())
            {
                var item = card.getOwner().getItem();
                if (item != null && item.isWindfury(game))
                    return true;
            }
            return card.hasKeyword(game, KEYWORD.WINDFURY);
        }
        public static bool isImmunity(this ICardData card, IGame game)
        {
            return card.hasKeyword(game, KEYWORD.IMMUNITY);
        }
        public static bool isUnique(this ICardData card, IGame game)
        {
            return card.hasKeyword(game, KEYWORD.UNIQUE);
        }
        public static bool canBeCharged(this ICardData card, IGame game)
        {
            return card.hasKeyword(game, KEYWORD.CAN_BE_CHARGED);
        }
        public static bool isCharged(this ICardData card, IGame game)
        {
            return card.getChargedLevel(game) > 0;
        }
        public static int getChargedLevel(this ICardData card, IGame game)
        {
            return card.getProp<int>(game, CARD.CHARGED_LEVEL);
        }
        public static bool isUnplayable(this ICardData card, IGame game)
        {
            return card.hasKeyword(game, KEYWORD.UNPLAYABLE);
        }
        public static bool isModifiedUnplayable(this ICardData card, IGame game)
        {
            return card.hasModifiedKeyword(game, KEYWORD.UNPLAYABLE);
        }
        public static int getSpellDamage(this ICardData card, IGame game)
        {
            return card.getProp<int>(game, DEFINE.SPELL_DAMAGE);
        }
        public static int getDamageReduce(this ICardData card, IGame game)
        {
            int result = card.getProp<int>(game, DEFINE.DAMAGE_REDUCE);
            if (result < 0)
                return 0;
            return result;
        }
        #endregion

        #region 设置
        public static void setRush(this Card card, IGame game, bool value)
        {
            if (value)
                card.addKeywords(game, KEYWORD.RUSH);
            else
                card.removeKeywords(game, KEYWORD.RUSH);
        }
        public static void setDrain(this Card card, IGame game, bool value)
        {
            if (value)
                card.addKeywords(game, KEYWORD.DRAIN);
            else
                card.removeKeywords(game, KEYWORD.DRAIN);
        }
        public static void setPoisonous(this Card card, IGame game, bool value)
        {
            if (value)
                card.addKeywords(game, KEYWORD.POISONOUS);
            else
                card.removeKeywords(game, KEYWORD.POISONOUS);
        }
        public static void setElusive(this Card card, IGame game, bool value)
        {
            if (value)
                card.addKeywords(game, KEYWORD.ELUSIVE);
            else
                card.removeKeywords(game, KEYWORD.ELUSIVE);
        }
        public static void setWindfury(this Card card, IGame game, bool value)
        {
            if (value)
                card.addKeywords(game, KEYWORD.WINDFURY);
            else
                card.removeKeywords(game, KEYWORD.WINDFURY);
        }
        public static void setImmunity(this Card card, IGame game, bool value)
        {
            if (value)
                card.addKeywords(game, KEYWORD.IMMUNITY);
            else
                card.removeKeywords(game, KEYWORD.IMMUNITY);
        }
        public static void setUnique(this Card card, IGame game, bool value)
        {
            if (value)
                card.addKeywords(game, KEYWORD.UNIQUE);
            else
                card.removeKeywords(game, KEYWORD.UNIQUE);
        }
        public static Task<EventArg> setChargeLevel(this Card card, CardEngine game, int value)
        {
            var beforeValue = card.getChargedLevel(game);
            if (value == beforeValue)
                return Task.FromResult<EventArg>(null);
            return BeChargedEventDefine.doEvent(game, card, value);
        }
        public static Task<EventArg> setSpellDamage(this Card card, IGame game, int value)
        {
            return card.setProp(game, DEFINE.SPELL_DAMAGE, value);
        }
        public static Task<EventArg> setDamageReduce(this Card card, IGame game, int value)
        {
            return card.setProp(game, DEFINE.DAMAGE_REDUCE, value);
        }
        #endregion

        #endregion

        #region 动作关键词
        [Obsolete]
        public static async Task<EventArg[]> silenceObsoleteMass(this IEnumerable<Card> cards, CardEngine game)
        {
            List<EventArg> eventArgs = new List<EventArg>();
            foreach (var card in cards)
            {
                eventArgs.Add(await silence(card, game));
            }
            return eventArgs.ToArray();
        }
        public static Task<EventArg> silence(this IEnumerable<Card> cards, CardEngine game)
        {
            return MassSealEventDefine.doEvent(game, cards.ToArray());
        }
        public static Task<EventArg> silence(this Card card, CardEngine game)
        {
            return SilenceEventDefine.doEvent(game, card);
        }
        public static bool isSilenced(this ICardData card, IGame game)
        {
            return card.getProp<bool>(game, CARD.SILENCE);
        }
        #endregion


        #region 可消耗关键词
        /// <summary>
        /// 初始化卡牌的可消耗关键字，如隐匿、护盾。
        /// </summary>
        /// <param name="card"></param>
        /// <param name="game"></param>
        public static async Task InitConsumableKeywords(this Card card, IGame game)
        {
            if (card.hasKeyword(game, KEYWORD.SHIELD))
            {
                await card.addKeywords(game, KEYWORD.HAS_SHIELD);
            }
            if (card.hasKeyword(game, KEYWORD.STEALTH))
            {
                await card.addKeywords(game, KEYWORD.HAS_STEALTH);
            }
            if (card.hasKeyword(game, KEYWORD.FROZEN))
            {
                await card.addKeywords(game, KEYWORD.IS_FROZEN);
            }
        }
        /// <summary>
        /// 更新卡牌的可消耗关键字，如隐匿、护盾。
        /// 如果不再具有护盾或隐匿的条件，把表示当前关键字状态的关键字也取消。
        /// </summary>
        /// <param name="card"></param>
        /// <param name="game"></param>
        public static async Task updateConsumableKeywords(this Card card, IGame game)
        {
            List<string> removeKeywords = new List<string>();
            if (!card.hasKeyword(game, KEYWORD.SHIELD) && card.hasKeyword(game, KEYWORD.HAS_SHIELD, true))
            {
                removeKeywords.Add(KEYWORD.HAS_SHIELD);
            }
            if (!card.hasKeyword(game, KEYWORD.STEALTH) && card.hasKeyword(game, KEYWORD.HAS_STEALTH, true))
            {
                removeKeywords.Add(KEYWORD.HAS_STEALTH);
            }
            if (!card.hasKeyword(game, KEYWORD.FROZEN) && card.hasKeyword(game, KEYWORD.IS_FROZEN, true))
            {
                removeKeywords.Add(KEYWORD.IS_FROZEN);
            }
            if (removeKeywords.Count > 0)
            {
                await card.removeKeywords(game, removeKeywords.ToArray());
            }
        }
        /// <summary>
        /// 清除卡牌的可消耗关键字，如隐匿、护盾。
        /// </summary>
        /// <param name="card"></param>
        /// <param name="game"></param>
        public static async Task clearConsumableKeywords(this Card card, IGame game)
        {
            await card.removeKeywords(game, KEYWORD.HAS_SHIELD, KEYWORD.HAS_STEALTH, KEYWORD.IS_FROZEN);
        }

        public static bool isShield(this ICardData card, IGame game)
        {
            return card.hasKeyword(game, KEYWORD.SHIELD) && card.hasKeyword(game, KEYWORD.HAS_SHIELD);
        }
        public static bool isStealth(this ICardData card, IGame game)
        {
            return (card.hasKeyword(game, KEYWORD.STEALTH) && card.hasKeyword(game, KEYWORD.HAS_STEALTH)) || card.hasKeyword(game, KEYWORD.PERMANENTSTEALTH);
        }
        public static bool isFreeze(this ICardData card, IGame game)
        {
            return (card.hasKeyword(game, KEYWORD.FROZEN) && card.hasKeyword(game, KEYWORD.IS_FROZEN)) || card.hasKeyword(game, KEYWORD.PERMANENT_FROZEN);
        }
        public static async Task setShield(this Card card, IGame game, bool value)
        {
            if (value)
            {
                await card.addKeywords(game, KEYWORD.SHIELD);
                await card.addKeywords(game, KEYWORD.HAS_SHIELD);
            }
            else
            {
                await card.removeKeywords(game, KEYWORD.SHIELD);
                await card.removeKeywords(game, KEYWORD.HAS_SHIELD);
            }
        }
        public static async Task setStealth(this Card card, IGame game, bool value)
        {
            if (value)
            {
                await card.addKeywords(game, KEYWORD.STEALTH);
                await card.addKeywords(game, KEYWORD.HAS_STEALTH);
            }
            else
            {
                await card.removeKeywords(game, KEYWORD.STEALTH);
                await card.removeKeywords(game, KEYWORD.HAS_STEALTH);
            }
        }
        public static async Task setFreeze(this Card card, CardEngine game, bool value)
        {
            if (value)
            {
                await card.shackle(game);
            }
            else
            {
                await card.removeKeywords(game, KEYWORD.FROZEN);
                await card.removeKeywords(game, KEYWORD.IS_FROZEN);
            }
        }
        public static Task<EventArg> shackle(this Card card, CardEngine game)
        {
            return ShackleEventDefine.doEvent(game, card);
        }
        #endregion

        #region 基本获取/设置
        public static bool hasKeyword(this ICard card, IGame game, string keyword, bool raw)
        {
            string[] keywords = card.getKeywords(game, raw);
            if (keywords == null)
            {
                return false;
            }
            return keywords.Contains(keyword);
        }
        public static bool hasKeyword(this ICardData card, IGame game, string keyword)
        {
            string[] keywords = card.getKeywords(game);
            if (keywords == null)
            {
                return false;
            }
            return keywords.Contains(keyword);
        }
        public static bool hasModifiedKeyword(this ICardData card, IGame game, string keyword)
        {
            var keywords = card.modifyProp(game, DEFINE.KEYWORDS, Array.Empty<string>());
            if (keywords == null)
                return false;
            return keywords.Contains(keyword);
        }
        public static async Task<EventArg> setKeywords(this Card card, IGame game, string[] keywords)
        {
            if (keywords == null)
                keywords = new string[0];
            string[] oldKeywords = card.getKeywords(game);
            if (oldKeywords != null)
            {
                foreach (var keyword in oldKeywords)
                {
                    await card.setProp(game, keyword, false);
                }
            }
            var result = await card.setProp(game, DEFINE.KEYWORDS, keywords);
            foreach (var keyword in card.getKeywords(game))
            {
                await card.setProp(game, keyword, true);
            }
            await card.updateConsumableKeywords(game);
            return result;
        }
        /// <summary>
        /// 获取关键词数组。
        /// </summary>
        /// <param name="card">卡牌。</param>
        /// <param name="game">游戏。</param>
        /// <param name="raw">是否忽略修改器。</param>
        /// <returns>关键词列表。</returns>
        public static string[] getKeywords(this ICard card, IGame game, bool raw)
        {
            if (card is ICard c)
            {
                return c.getProp<string[]>(game, DEFINE.KEYWORDS, raw) ?? Array.Empty<string>();
            }
            return card.getKeywords(game);
        }
        /// <summary>
        /// 获取关键词数组。
        /// </summary>
        /// <param name="card">卡牌。</param>
        /// <param name="game">游戏。</param>
        /// <returns>关键词列表。</returns>
        public static string[] getKeywords(this ICardData card, IGame game)
        {
            return card.getProp<string[]>(game, DEFINE.KEYWORDS) ?? Array.Empty<string>();
        }
        public static Task<EventArg> addKeywords(this Card card, IGame game, params string[] keywords)
        {
            string[] oldKeywords = card.getKeywords(game, true);
            if (oldKeywords == null)
                oldKeywords = keywords;
            else
                foreach (string key in keywords)
                {
                    if (!card.hasKeyword(game, key))
                    {
                        oldKeywords = oldKeywords.Append(key).Distinct().ToArray();
                    }
                }

            return card.setKeywords(game, oldKeywords);
        }
        public static Task<EventArg> removeKeywords(this Card card, IGame game, params string[] keywords)
        {
            List<string> keywordList = new List<string>(card.getKeywords(game, true));
            foreach (var keyword in keywords)
            {
                if (keywordList.Contains(keyword))
                {
                    keywordList.Remove(keyword);
                }
            }
            return card.setKeywords(game, keywordList.ToArray());
        }
        #endregion

        #endregion
    }
}