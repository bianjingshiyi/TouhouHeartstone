using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using System;
using MukioI18n;

namespace TouhouHeartstone
{
    public static class THHCard
    {
        public static bool isItem(this ICardData card)
        {
            return card.define.type == DEFINE.ITEM;
        }
        public static bool isSkill(this ICardData card)
        {
            return card.define.type == DEFINE.SKILL;
        }
        public static bool isServant(this ICardData card)
        {
            return card.define.type == DEFINE.SERVANT;
        }
        public static bool isMaster(this ICardData card)
        {
            return card.define.type == DEFINE.MASTER;
        }
        /// <summary>
        /// 这是不是一张法术牌
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public static bool isSpell(this ICardData card)
        {
            return card.define.type == DEFINE.SPELL;
        }
        public static Player getOwner(this ICardData card)
        {
            return card.owner;
        }
        public static int getCost(this ICardData card, IGame game)
        {
            int result = card.getProp<int>(game, DEFINE.COST);
            if (result < 0)
                return 0;
            return result;
        }
        // public static Task<IPropChangeEventArg> setCost(this Card card, IGame game, int value)
        // {
        //     return card.setProp(game, nameof(ServantCardDefine.cost), value);
        // }

        public static int getAttack(this Card card, IGame game)
        {
            int result = card.getProp<int>(game, DEFINE.ATTACK);
            if (game is CardEngine engine && card == engine.getCurrentPlayer()?.getMaster())
            {
                if (card.getOwner().getItem() != null)
                    result += card.getOwner().getItem().getAttack(game);
            }
            if (result < 0)
                return 0;
            return result;
        }
        public static int getAttack(this ICardData card, IGame game)
        {
            int result = card.getProp<int>(game, DEFINE.ATTACK);
            if (result < 0)
                return 0;
            return result;
        }

        [Obsolete("Use buff or modifier instead")]
        public static Task<IPropChangeEventArg> setAttack(this Card card, IGame game, int value)
        {
            return card.setProp(game, DEFINE.ATTACK, value);
        }
        public static int getLife(this ICardData card, IGame game)
        {
            return card.getProp<int>(game, DEFINE.LIFE);
        }

        [Obsolete("Use buff or modifier instead")]
        public static Task<IPropChangeEventArg> setLife(this Card card, IGame game, int value)
        {
            return card.setProp(game, DEFINE.LIFE, value);
        }

        /// <summary>
        /// 一个随从/物品/英雄是否已经死了？除了生命值小于等于0以外，还有一个isDead的属性来控制卡片是否在死亡结算当中。
        /// </summary>
        /// <param name="card">目标卡牌。</param>
        /// <param name="game">游戏对象。</param>
        /// <param name="checkHealth">是否检查卡牌生命值小于等于0？</param>
        /// <param name="checkKilled">是否检查卡牌处于待摧毁状态？</param>
        /// <returns></returns>
        public static bool isDead(this Card card, IGame game, bool checkHealth = true, bool checkKilled = true)
        {
            if (checkHealth)
            {
                if (card.getCurrentLife(game) <= 0)
                    return true;
                if (card.getLife(game) <= 0)
                    return true;
            }
            if (checkKilled && card.getProp<bool>(game, CARD.ISDEAD))
                return true;
            return false;
        }
        /// <summary>
        /// 杀死一张卡牌。
        /// 实际作用为将其设置为已死亡。
        /// </summary>
        /// <param name="card">卡牌。</param>
        /// <param name="game">游戏对象。</param>
        public static async Task kill(this Card card, IGame game)
        {
            await setDead(card, game, true);
        }
        /// <summary>
        /// 杀死多张卡牌。
        /// 实际作用为将其设置为已死亡。
        /// </summary>
        /// <param name="cards">卡牌列表。</param>
        /// <param name="game">游戏对象。</param>
        public static async Task kill(this IEnumerable<Card> cards, IGame game)
        {
            foreach (var card in cards)
            {
                if (cards == null)
                    continue;
                await card.kill(game);
            }
        }
        /// <summary>
        /// 设置为true则随从会在下一次死亡结算中视作阵亡。
        /// </summary>
        /// <param name="card"></param>
        /// <param name="value"></param>
        public static Task<IPropChangeEventArg> setDead(this Card card, IGame game, bool value)
        {
            return card.setProp(game, CARD.ISDEAD, value);
        }
        public static async Task<IPropChangeEventArg[]> setDead(this IEnumerable<Card> cards, CardEngine game, bool value)
        {
            List<IPropChangeEventArg> argList = new List<IPropChangeEventArg>();
            foreach (var card in cards)
            {
                argList.Add(await card.setProp(game, CARD.ISDEAD, value));
            }
            return argList.ToArray();
        }
        public static int getArmor(this ICardData card, IGame game)
        {
            return card.getProp<int>(game, CARD.ARMOR);
        }
        public static Task<SetArmorEventArg> gainArmor(this Card card, CardEngine game, int value)
        {
            return setArmor(card, game, card.getArmor(game) + value);
        }
        public static Task<SetArmorEventArg> loseArmor(this Card card, CardEngine game, int value)
        {
            return card.setArmor(game, card.getArmor(game) - value);
        }
        public static Task<SetArmorEventArg> setArmor(this Card card, CardEngine game, int value)
        {
            int beforeValue = card.getArmor(game);
            return game.triggers.doEvent(new SetArmorEventArg() { card = card, beforeValue = beforeValue, value = value }, arg =>
            {
                card = arg.card;
                value = arg.value;
                if (value < 0)
                    value = 0;
                card.setProp(game, CARD.ARMOR, value);
                return Task.CompletedTask;
            });
        }
        public class SetArmorEventArg : EventArg, IDescribableEventArg
        {
            ICard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return card;
            }
            ICard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return null;
            }

            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                IDescribableEventArg arg = this;
                return string.Format(arg.localizeTemplateString(game, viewer), arg.localizeStringArgs(game, viewer));
            }

            public override void Record(IGame game, EventRecord record)
            {
                record.setCardState(EVENTVAR.CARD, card);
                record.setVar(EVENTVAR.VALUE, value);
                record.setVar(VAR_BEFORE_VALUE, beforeValue);
            }
            [TranslateMsg("{0}是卡，{1}是护甲值")]
            const string TEMPLATE = "{0} 的护甲变为 {1}";
            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                return TEMPLATE;
            }
            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                return new object[] {
                    card.getFormatString(),
                    value
                };
            }
            public override EventVariableInfo[] getBeforeEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo() { name = EVENTVAR.CARD, type = typeof(Card) },
                    new EventVariableInfo() { name = VAR_BEFORE_VALUE, type = typeof(int) },
                    new EventVariableInfo() { name = EVENTVAR.VALUE, type = typeof(int) }
                };
            }
            public override EventVariableInfo[] getAfterEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo() { name = EVENTVAR.CARD, type = typeof(Card) },
                    new EventVariableInfo() { name = VAR_BEFORE_VALUE, type = typeof(int) },
                    new EventVariableInfo() { name = EVENTVAR.VALUE, type = typeof(int) }
                };
            }
            public Card card
            {
                get { return getVar<Card>(EVENTVAR.CARD); }
                set { setVar(EVENTVAR.CARD, value); }
            }
            public int beforeValue
            {
                get { return getVar<int>(VAR_BEFORE_VALUE); }
                set { setVar(VAR_BEFORE_VALUE, value); }
            }
            public int value
            {
                get { return getVar<int>(EVENTVAR.VALUE); }
                set { setVar(EVENTVAR.VALUE, value); }
            }
            public const string VAR_BEFORE_VALUE = "原数值";
        }
        public static int getCurrentLife(this ICardData card, IGame game)
        {
            return card.getProp<int>(game, CARD.CURRENT_LIFE);
        }
        public static Task<IPropChangeEventArg> setCurrentLife(this Card card, IGame game, int value)
        {
            return card.setProp(game, CARD.CURRENT_LIFE, value);
        }
        public static bool isReady(this Card card, IGame game)
        {
            return card.getProp<bool>(game, CARD.ISREADY);
        }
        public static Task<IPropChangeEventArg> setReady(this Card card, IGame game, bool value)
        {
            return card.setProp(game, CARD.ISREADY, value);
        }
        public static int getAttackTimes(this Card card, IGame game)
        {
            return card.getProp<int>(game, CARD.ATTACK_TIMES);
        }
        public static Task<IPropChangeEventArg> setAttackTimes(this Card card, IGame game, int value)
        {
            return card.setProp(game, CARD.ATTACK_TIMES, value);
        }
        public static int getMaxAttackTimes(this Card card, IGame game)
        {
            if (card.isWindfury(game))
                return 2;
            else
                return 1;
        }

        /// <summary>
        /// 这个角色能否进行攻击？
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public static bool canAttack(this Card card, CardEngine game)
        {
            if (card.hasKeyword(game, KEYWORD.CANTATTACK))//不能攻击
                return false;
            if (card.getAttack(game) <= 0)//没有攻击力
                return false;
            if (card.pile == null)
            {
                UberDebug.LogError($"卡片{card}的牌堆是空的，这很不正常");
                return false;
            }
            if (card.pile?.name == PILE.FIELD)
            {
                //随从限定
                if (!card.isReady(game)//还没准备好
                && !card.isCharge(game)//且没有冲锋
                && !(card.isRush(game) && game.getOpponent(card.getOwner()).getField().Any(c => card.isAttackable(game, card.getOwner(), c, out _)))//且并非有突袭且有可以攻击的敌方随从
                )
                    return false;
            }
            if (card.getAttackTimes(game) >= card.getMaxAttackTimes(game))//已经攻击过了
                return false;
            if (card.isFreeze(game))
                return false;
            if (!card.canForceAttack(game))
                return false;
            return true;
        }
        public static bool canForceAttack(this Card card, CardEngine game)
        {
            if (!(card.pile.name == PILE.FIELD || card.pile.name == PILE.MASTER))//不在战场上
                return false;
            return true;
        }

        [TranslateMsg]
        const string INFO_CANNOT_ATTACK_SELF = "你不能攻击友方角色";

        [TranslateMsg]
        const string INFO_GUARD_SERVANT = "你必须先攻击具有守卫的仆从";

        [TranslateMsg]
        const string INFO_HASTE_NOT_PREPARED = "具有速攻的仆从在没有准备好的情况下不能攻击敌方英雄";

        [TranslateMsg]
        const string INFO_TARGET_IS_STEALTH = "无法攻击隐匿的目标";

        [TranslateMsg]
        const string INFO_INVALID_TARGET = "无法攻击目标";

        /// <summary>
        /// 这个角色能否对目标进行攻击？
        /// </summary>
        /// <param name="card"></param>
        /// <param name="game"></param>
        /// <param name="player"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool isAttackable(this Card card, CardEngine game, Player player, Card target, out string tip)
        {
            if (target == player.getMaster() || player.getField().Contains(target))
            {
                tip = INFO_CANNOT_ATTACK_SELF;
                return false;
            }
            if (game.getOpponent(player).getField().Any(c => c.isTaunt(game) && !c.isStealth(game) && !c.isImmunity(game)) && !target.isTaunt(game))
            {
                tip = INFO_GUARD_SERVANT;
                return false;
            }
            if (card.isRush(game) && !card.isReady(game) && game.players.Any(p => p.getMaster() == target) && !card.isCharge(game))
            {
                tip = INFO_HASTE_NOT_PREPARED;//除非你具有冲锋
                return false;
            }
            if (target.isStealth(game))
            {
                tip = INFO_TARGET_IS_STEALTH;
                return false;
            }
            if (!card.isForceAttackable(game, player, target))
            {
                tip = INFO_INVALID_TARGET;
                return false;
            }
            tip = null;
            return true;
        }
        public static bool isForceAttackable(this Card card, CardEngine game, Player player, Card target)
        {
            if (!(target.pile.name == PILE.FIELD || target.pile.name == PILE.MASTER))//不在战场上
                return false;
            if (target.isDead(game))//已经死了
                return false;
            return true;
        }
        /// <summary>
        /// 技能是否已经使用过？
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public static bool isUsed(this ICardData card, IGame game)
        {
            return card.getProp<bool>(game, "isUsed");
        }
        /// <summary>
        /// 设置技能是否使用过。
        /// </summary>
        /// <param name="card"></param>
        /// <param name="value"></param>
        public static Task<IPropChangeEventArg> setUsed(this Card card, IGame game, bool value)
        {
            return card.setProp(game, "isUsed", value);
        }
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
        public static void setRush(this Card card, IGame game, bool value)
        {
            if (value)
                card.addKeywords(game, KEYWORD.RUSH);
            else
                card.removeKeywords(game, KEYWORD.RUSH);
        }
        public static bool isShield(this ICardData card, IGame game)
        {
            return card.hasKeyword(game, KEYWORD.SHIELD) && card.hasKeyword(game, KEYWORD.HAS_SHIELD);
        }
        public static void setShield(this Card card, IGame game, bool value)
        {
            if (value)
            {
                card.addKeywords(game, KEYWORD.SHIELD);
                card.addKeywords(game, KEYWORD.HAS_SHIELD);
            }
            else
            {
                card.removeKeywords(game, KEYWORD.SHIELD);
                card.removeKeywords(game, KEYWORD.HAS_SHIELD);
            }
        }
        public static bool isStealth(this ICardData card, IGame game)
        {
            return (card.hasKeyword(game, KEYWORD.STEALTH) && card.hasKeyword(game, KEYWORD.HAS_STEALTH)) || card.hasKeyword(game, KEYWORD.PERMANENTSTEALTH);
        }
        public static void setStealth(this Card card, IGame game, bool value)
        {
            if (value)
            {
                card.addKeywords(game, KEYWORD.STEALTH);
                card.addKeywords(game, KEYWORD.HAS_STEALTH);
            }
            else
            {
                card.removeKeywords(game, KEYWORD.STEALTH);
                card.removeKeywords(game, KEYWORD.HAS_STEALTH);
            }
        }
        public static bool isDrain(this ICardData card, IGame game)
        {
            return card.hasKeyword(game, KEYWORD.DRAIN);
        }
        public static void setDrain(this Card card, IGame game, bool value)
        {
            if (value)
                card.addKeywords(game, KEYWORD.DRAIN);
            else
                card.removeKeywords(game, KEYWORD.DRAIN);
        }
        public static bool isPoisonous(this ICardData card, IGame game)
        {
            return card.hasKeyword(game, KEYWORD.POISONOUS);
        }
        public static void setPoisonous(this Card card, IGame game, bool value)
        {
            if (value)
                card.addKeywords(game, KEYWORD.POISONOUS);
            else
                card.removeKeywords(game, KEYWORD.POISONOUS);
        }
        public static bool isElusive(this ICardData card, IGame game)
        {
            return card.hasKeyword(game, KEYWORD.ELUSIVE);
        }
        public static void setElusive(this Card card, IGame game, bool value)
        {
            if (value)
                card.addKeywords(game, KEYWORD.ELUSIVE);
            else
                card.removeKeywords(game, KEYWORD.ELUSIVE);
        }
        public static bool isFreeze(this ICardData card, IGame game)
        {
            return card.hasKeyword(game, KEYWORD.IS_FROZEN) || card.hasKeyword(game, KEYWORD.PERMANENT_FROZEN);
        }
        public static Task<IPropChangeEventArg> setFreeze(this Card card, IGame game, bool value)
        {
            if (value)
                return card.addKeywords(game, KEYWORD.IS_FROZEN);
            else
                return card.removeKeywords(game, KEYWORD.IS_FROZEN);
        }
        public static void setFreeze(this IEnumerable<Card> cards, IGame game, bool value)
        {
            foreach (var card in cards)
            {
                card.setFreeze(game, value);
            }
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
        public static void setWindfury(this Card card, IGame game, bool value)
        {
            if (value)
                card.addKeywords(game, KEYWORD.WINDFURY);
            else
                card.removeKeywords(game, KEYWORD.WINDFURY);
        }
        public static bool isImmunity(this ICardData card, IGame game)
        {
            return card.hasKeyword(game, KEYWORD.IMMUNITY);
        }
        public static void setImmunity(this Card card, IGame game, bool value)
        {
            if (value)
                card.addKeywords(game, KEYWORD.IMMUNITY);
            else
                card.removeKeywords(game, KEYWORD.IMMUNITY);
        }
        public static bool isUnique(this ICardData card, IGame game)
        {
            return card.hasKeyword(game, KEYWORD.UNIQUE);
        }
        public static void setUnique(this Card card, IGame game, bool value)
        {
            if (value)
                card.addKeywords(game, KEYWORD.UNIQUE);
            else
                card.removeKeywords(game, KEYWORD.UNIQUE);
        }
        public static async Task<IPropChangeEventArg> setKeywords(this Card card, IGame game, string[] keywords)
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
        /// <param name="raw">是否忽略修改器。</param>
        /// <returns>关键词列表。</returns>
        public static string[] getKeywords(this ICardData card, IGame game)
        {
            return card.getProp<string[]>(game, DEFINE.KEYWORDS) ?? Array.Empty<string>();
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

        public static Task<IPropChangeEventArg> addKeywords(this Card card, IGame game, params string[] keywords)
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
        public static Task<IPropChangeEventArg> removeKeywords(this Card card, IGame game, params string[] keywords)
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

        #region 蓄力
        public class BeChargedEventArg : EventArg
        {
            public Card card;
            public int value;
            public override void Record(IGame game, EventRecord record)
            {
                record.setCardState(EVENTVAR.CARD, card);
                record.setVar(EVENTVAR.VALUE, value);
            }
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
            return card.getProp<int>(game, DEFINE.CHARGED_LEVEL);
        }
        public static Task setChargeLevel(this Card card, CardEngine game, int value)
        {
            if (value == card.getChargedLevel(game))
                return Task.CompletedTask;
            return game.triggers.doEvent(new BeChargedEventArg() { card = card, value = value }, arg =>
            {
                card = arg.card;
                value = arg.value;
                game.logger.log(card + "改变蓄力层数 => " + value);
                return card.setProp(game, DEFINE.CHARGED_LEVEL, value);
            });
        }
        #endregion 蓄力
        public static int getSpellDamage(this ICardData card, IGame game)
        {
            return card.getProp<int>(game, DEFINE.SPELL_DAMAGE);
        }
        public static Task<IPropChangeEventArg> setSpellDamage(this Card card, IGame game, int value)
        {
            return card.setProp(game, DEFINE.SPELL_DAMAGE, value);
        }
        public static int getDamageReduce(this ICardData card, IGame game)
        {
            int result = card.getProp<int>(game, nameof(ServantCardDefine.damageReduce));
            if (result < 0)
                return 0;
            return result;
        }
        public static Task<IPropChangeEventArg> setDamageReduce(this Card card, IGame game, int value)
        {
            return card.setProp(game, nameof(ServantCardDefine.damageReduce), value);
        }
        public static Task<SilenceEventArg> silence(this IEnumerable<Card> cards, CardEngine game)
        {
            SilenceEventArg eventArg = new SilenceEventArg() { cards = cards.ToArray() };
            return game.triggers.doEvent(eventArg, async arg =>
            {
                cards = arg.cards;
                foreach (var card in cards)
                {
                    if (card == null)
                        continue;
                    await card.setProp(game, CARD.SILENCE, true);
                    await card.removeBuff(game, card.getBuffs());
                    foreach (var effect in card.define.getEffects().OfType<IPassiveEffect>())
                    {
                        await effect.onDisable(game, card, null);
                    }
                    await card.setKeywords(game, new string[0]);

                }
            });
        }
        public class SilenceEventArg : EventArg, IDescribableEventArg
        {
            public override void Record(IGame game, EventRecord record)
            {
                record.setCardStates(EVENTVAR.CARD, cards);
            }
            ICard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return cards.Length < 2 ? cards[0] : null;
            }

            ICard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return cards.Length > 1 ? cards : null;
            }

            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                IDescribableEventArg arg = this;
                return String.Format(arg.localizeTemplateString(game, viewer), arg.localizeStringArgs(game, viewer));
            }

            [MukioI18n.TranslateMsg("{0}是卡牌列表")]
            const string TEMPLATE = "{0} 被封印";
            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                return TEMPLATE;
            }
            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                return new object[] { string.Join("，", cards.Select(c => c.getFormatString())) };
            }

            public override EventVariableInfo[] getBeforeEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card[])}
                };
            }
            public override EventVariableInfo[] getAfterEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card[])}
                };
            }
            public Card[] cards
            {
                get { return getVar<Card[]>(EVENTVAR.CARD); }
                set { setVar(EVENTVAR.CARD, value); }
            }
        }
        public static Task<SilenceEventArg> silence(this Card card, CardEngine game)
        {
            return silence(new Card[] { card }, game);
        }
        public static bool isSilenced(this ICardData card, IGame game)
        {
            return card.getProp<bool>(game, CARD.SILENCE);
        }
        public static (IEffect effect, Buff buff)[] getBattlecries(this Card card, IGame game)
        {
            List<(IEffect effect, Buff buff)> results = new List<(IEffect effect, Buff buff)>();
            if (!card.isSilenced(game))
            {
                results.AddRange(card.define.getBattlecries()
                    .Select<IEffect, (IEffect effect, Buff buff)>(e => (e, null)));
            }
            foreach (var buff in card.getBuffs())
            {
                var effects = buff.getEffects(game as CardEngine);
                results.AddRange(effects.Where(e =>
                {
                    if (e.isBattlecry())
                        return true;
                    if (e is ActiveEffect && (card.isServant() || card.isItem()))
                        return true;
                    return false;
                })
                    .Select<IEffect, (IEffect effect, Buff buff)>(e => (e, buff)));
            }
            return results.ToArray();
        }
        public static bool hasBattlecry(this ICardData card, IGame game)
        {
            if (!card.isSilenced(game))
            {
                if (card.define.hasBattlecry())
                    return true;
            }
            foreach (var buff in card.getBuffs())
            {
                var effects = buff.getEffects(game as CardEngine);
                if (effects.Any(e => e.isBattlecry()))
                    return true;
            }
            return false;
        }
        public static (IEffect effect, Buff buff)[] getDeathRattles(this Card card, IGame game)
        {
            List<(IEffect effect, Buff buff)> results = new List<(IEffect effect, Buff buff)>();
            if (!card.isSilenced(game))
            {
                results.AddRange(card.define.getDeathRattles()
                    .Select<IEffect, (IEffect effect, Buff buff)>(e => (e, null)));
            }
            foreach (var buff in card.getBuffs())
            {
                var effects = buff.getEffects(game as CardEngine);
                results.AddRange(effects.Where(e => e.isDeathRattle())
                    .Select<IEffect, (IEffect effect, Buff buff)>(e => (e, buff)));
            }
            return results.Where(e =>
            {
                if (e.effect is DeathRattle rattle)
                {
                    return !rattle.isDisabled(game, card, e.buff);
                }
                else if (e.effect is GeneratedEffect gen)
                {
                    return !gen.isDisabled(game, card, e.buff);
                }
                return false;
            }).ToArray();
        }
        public static (IEffect effect, Buff buff)[] getAuras(this Card card, IGame game)
        {
            List<(IEffect effect, Buff buff)> results = new List<(IEffect effect, Buff buff)>();
            results.AddRange(card.define.getAuras()
                .Select<IEffect, (IEffect effect, Buff buff)>(e => (e, null)));
            foreach (var buff in card.getBuffs())
            {
                var effects = buff.getEffects(game as CardEngine);
                results.AddRange(effects.Where(e => e.isAura())
                    .Select<IEffect, (IEffect effect, Buff buff)>(e => (e, buff)));
            }
            return results.ToArray();
        }
        public static async Task updateAuras(this Card card, IGame game)
        {
            var aura = card.getAuras(game);
            foreach (var (effect, buff) in aura)
            {
                if (effect is GeneratedAura gen)
                {
                    await gen.execute(game, card, buff, null);
                }
            }
        }
        public static bool hasDeathRattle(this ICardData card, IGame game)
        {
            if (!card.isSilenced(game))
            {
                if (card.define.hasDeathRattles())
                {
                    return true;
                }
            }
            foreach (var buff in card.getBuffs())
            {
                var effects = buff.getEffects(game as CardEngine);
                if (effects.Any(e => e.isDeathRattle()))
                {
                    return true;
                }
            }
            return false;
        }
        public static bool hasTag(this ICardData card, IGame game, string tag)
        {
            string[] tags = card.getProp<string[]>(game, nameof(ServantCardDefine.tags));
            if (tags == null)
                return false;
            return tags.Contains(tag);
        }
        public static string getTag(this ICardData card, IGame game, int index)
        {
            return card.getTags(game)[index];
        }
        public static string[] getTags(this ICardData card, IGame game)
        {
            string[] tags = card.getProp<string[]>(game, nameof(DEFINE.TAGS));
            return tags ?? card.define.getTags();
        }
        public static Task<IPropChangeEventArg> setTags(this Card card, IGame game, string[] tags)
        {
            return card.setProp(game, nameof(ServantCardDefine.tags), tags);
        }
        /// <summary>
        /// 初始化卡牌的可消耗关键字，如隐匿、护盾。
        /// </summary>
        /// <param name="card"></param>
        /// <param name="game"></param>
        public static void InitConsumableKeywords(this Card card, CardEngine game)
        {
            if (card.hasKeyword(game, KEYWORD.SHIELD))
            {
                card.addKeywords(game, KEYWORD.HAS_SHIELD);
            }
            if (card.hasKeyword(game, KEYWORD.STEALTH))
            {
                card.addKeywords(game, KEYWORD.HAS_STEALTH);
            }
            card.setFreeze(game, card.hasKeyword(game, KEYWORD.FROZEN));
        }
        [TranslateMsg]
        const string NOT_YOUR_TURN = "这不是你的回合";

        [TranslateMsg]
        const string NOT_YOUR_CARD = "你不能使用不属于你的卡牌";
        [TranslateMsg]
        const string ONLY_SKILL_OR_HAND = "你只能使用技能或手牌";
        [TranslateMsg]
        const string NOT_ENOUGH_SPIRIT = "灵力值不足";
        [TranslateMsg]
        const string NO_TARGET = "没有目标";
        [TranslateMsg]
        const string SERVANT_FIELD_FULL = "位置已满";
        [TranslateMsg]
        const string NO_SPACE_FOR_TRAP = "你满头是包";
        [TranslateMsg]
        const string DUPLICATED_TRAP = "你已挂载了相同的陷阱";
        [TranslateMsg]
        const string ONLY_ONE_MISSION = "专注于你手头上的委托！";
        [TranslateMsg]
        const string SKILL_USED = "你已经使用过技能了";
        [TranslateMsg]
        const string SKILL_UNUSABLE = "技能不可用";
        [TranslateMsg]
        const string SKILL_INVALID = "技能无效";
        [TranslateMsg]
        const string UNKNOWN_CARD = "这是一张未知的卡牌";

        public static bool isUsable(this Card card, CardEngine game, Player player, out string info)
        {
            if (game.getCurrentPlayer() != player)//不是你的回合
            {
                info = NOT_YOUR_TURN;
                return false;
            }
            if (card.getOwner() != player)
            {
                info = NOT_YOUR_CARD;
                return false;
            }
            if (card.pile.name != PILE.HAND && card.pile.name != PILE.SKILL)
            {
                info = ONLY_SKILL_OR_HAND;
                return false;
            }
            if (card.define.isServant())
            {
                if (player.getGem() < card.getCost(game))//费用不够
                {
                    info = NOT_ENOUGH_SPIRIT;
                    return false;
                }
                if (player.getField().count >= player.getField().maxCount)
                {
                    info = SERVANT_FIELD_FULL;
                    return false;
                }
                info = null;
                return true;
            }
            else if (card.define.isItem())
            {
                if (player.getGem() < card.getCost(game))//费用不够
                {
                    info = NOT_ENOUGH_SPIRIT;
                    return false;
                }
                info = null;
                return true;
            }
            else if (card.define.isSpell())
            {
                if (player.getGem() < card.getCost(game))
                {
                    info = NOT_ENOUGH_SPIRIT;
                    return false;
                }
                if (card.isNeedTarget(game) && card.getAvaliableTargets(game).Length < 1)
                {
                    info = NO_TARGET;
                    return false;
                }
                if (card.hasKeyword(game, KEYWORD.SECRET))
                {
                    if (card.getOwner().getHead().count >= card.getOwner().getHead().maxCount)
                    {
                        game.logger.log(NO_SPACE_FOR_TRAP);
                        info = NO_SPACE_FOR_TRAP;
                        return false;
                    }
                    if (card.getOwner().getHead().Any(c => c.define == card.define))
                    {
                        game.logger.log(DUPLICATED_TRAP);
                        info = DUPLICATED_TRAP;
                        return false;
                    }
                }
                if (card.hasKeyword(game, KEYWORD.MISSION))
                {
                    if (card.getOwner().getHead().count >= card.getOwner().getHead().maxCount)
                    {
                        game.logger.log(NO_SPACE_FOR_TRAP);
                        info = NO_SPACE_FOR_TRAP;
                        return false;
                    }
                    if (card.getOwner().getHead().Any(c => c.define.hasKeyword(KEYWORD.MISSION)))
                    {
                        game.logger.log(ONLY_ONE_MISSION);
                        info = ONLY_ONE_MISSION;
                        return false;
                    }
                }
                info = null;
                return true;
            }
            else if (card.define.isSkill())
            {
                if (card.isUsed(game))//已经用过了
                {
                    info = SKILL_USED;
                    return false;
                }
                if (player.getGem() < card.getCost(game))//费用不够
                {
                    info = NOT_ENOUGH_SPIRIT;
                    return false;
                }
                // TODO 这个检测是坏的，无法正确判断GeneratedEffect，因此自定义技能永远无法使用
                //if (card.define.getEffectOn<HearthStoneRule.ActiveEventArg>(game.triggers) is ITriggerEffect effect)
                //{
                //    if (!effect.checkCondition(game, card, null, new object[]
                //    {
                //        new HearthStoneRule.ActiveEventArg(player,card,new object[0])
                //    }))
                //    {
                //        info = SKILL_UNUSABLE;
                //        return false;
                //    }
                //    info = null;
                //    return true;
                //}
                //if (card.define.getActiveEffect() is IActiveEffect activeEffect)
                //{
                //    if (!activeEffect.checkCondition(game, card, null))
                //    {
                //        info = SKILL_UNUSABLE;
                //        return false;
                //    }
                //    info = null;
                //    return true;
                //}
                //info = SKILL_INVALID;
                //return false;
                info = null;
                return true;
            }
            else
            {
                info = UNKNOWN_CARD;
                return false;//不知道是什么卡
            }
        }
        public static bool isNeedTarget(this Card card, CardEngine game)
        {
            if (card.isSilenced(game))
                return false;
            if (card.define.getActiveEffect() is ITargetEffect targetEffect)
                return true;
            else if (card.define.getEffects().OfType<GeneratedActiveEffect>().FirstOrDefault() is GeneratedActiveEffect activeEffect)
            {
                var entryNode = activeEffect.entryNode;
                return entryNode != null && entryNode.targetCheckerList != null && entryNode.targetCheckerList.Count > 0;
            }
            else
                return false;
        }
        /// <summary>
        /// 这张卡的主动效果的可选目标
        /// </summary>
        /// <param name="card"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        public static Card[] getAvaliableTargets(this Card card, CardEngine game)
        {
            if (card.isSilenced(game))
                return Array.Empty<Card>();
            List<Card> targetList = new List<Card>();
            if (card.define.getActiveEffect() is ITargetEffect targetEffect)
            {
                foreach (Player player in game.players)
                {
                    if (card.isValidTarget(game, player.getMaster(), targetEffect, null, null))
                        targetList.Add(player.getMaster());
                    foreach (Card servant in player.getField())
                    {
                        if (card.isValidTarget(game, servant, targetEffect, null, null))
                            targetList.Add(servant);
                    }
                }
            }
            else if (card.define.getEffects().OfType<GeneratedActiveEffect>().FirstOrDefault() is GeneratedActiveEffect generatedEffect)
            {
                var entryNode = generatedEffect.entryNode;
                if (entryNode != null && entryNode.targetCheckerList != null && entryNode.targetCheckerList.Count > 0)
                {
                    foreach (TargetChecker targetChecker in entryNode.targetCheckerList)
                    {
                        foreach (Player player in game.players)
                        {
                            if (targetChecker.targetType == DEFINE.TARGET_TYPE_CHARACTER)
                            {
                                if (card.isValidTarget(game, player.getMaster(), null, null, generatedEffect))
                                    targetList.Add(player.getMaster());
                            }
                            if (targetChecker.targetType == DEFINE.TARGET_TYPE_CHARACTER ||
                                targetChecker.targetType == DEFINE.TARGET_TYPE_SERVANT)
                            {
                                foreach (Card servant in player.getField())
                                {
                                    if (card.isValidTarget(game, servant, null, null, generatedEffect))
                                        targetList.Add(servant);
                                }
                            }
                        }
                    }
                }
            }
            return targetList.ToArray();
        }
        public static bool isValidTarget(this Card card, CardEngine game, Card target)
        {
            if (card.isSilenced(game))
                return false;
            ITargetEffect targetEffect = card.define.getActiveEffect() as ITargetEffect;
            ITriggerEffect triggerEffect = card.define.getEffectOn<HearthStoneRule.ActiveEventArg>(game.triggers);
            GeneratedActiveEffect generatedEffect = card.define.getEffects().OfType<GeneratedActiveEffect>().FirstOrDefault();
            return isValidTarget(card, game, target, targetEffect, triggerEffect, generatedEffect);
        }
        private static bool isValidTarget(this Card card, CardEngine game, Card target, ITargetEffect targetEffect, ITriggerEffect triggerEffect, GeneratedActiveEffect generatedEffect)
        {
            //目标不可作为效果目标
            if (target.isStealth(game) && target.getOwner() != card.getOwner())
                return false;
            //没有任何效果
            if (targetEffect == null && triggerEffect == null && generatedEffect == null)
                return false;
            //存在效果且不符合目标条件
            if (targetEffect != null && !targetEffect.checkTargets(game, card, null, new object[] { target }))
                return false;
            if (triggerEffect != null && triggerEffect.checkTargets(game, card, null, null, new object[] { target }))
                return false;
            HearthStoneRule.ActiveEventArg eventArg = new HearthStoneRule.ActiveEventArg(card.owner, card, new object[] { target });
            if (generatedEffect != null && !generatedEffect.checkTarget(game, card, null, eventArg, out _))
                return false;
            return true;
        }

        /// <summary>
        /// 重置卡牌
        /// </summary>
        /// <param name="card"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        public static async Task resetCard(this Card card, CardEngine game)
        {
            var define = card.define;
            await card.removeBuff(game, card.getBuffs());
            await card.setSpellDamage(game, define.getSpellDamage());
            await card.setDead(game, false);
            await card.setCurrentLife(game, define.getLife());
            // await card.setCost(game, servant.cost);
            await card.setKeywords(game, define.getKeywords());
            await card.setTags(game, define.getTags());
            await card.setReady(game, false);
            await card.setAttackTimes(game, 0);
            await card.setProp(game, CARD.SILENCE, false);
            await card.setChargeLevel(game, 0);
            if (card.isServant() && card.pile.name == PILE.FIELD)
            {
                card.InitConsumableKeywords(game);
            }
            if (card.isSkill())
            {
                await card.setUsed(game, false);
            }
        }
        public static async Task resetCard(this IEnumerable<Card> cards, CardEngine game)
        {
            foreach (var card in cards)
            {
                await resetCard(card, game);
            }
        }
        /// <summary>
        /// 发动一张卡的主动效果
        /// </summary>
        /// <param name="card"></param>
        /// <param name="game"></param>
        /// <param name="player"></param>
        /// <param name="targets"></param>
        /// <returns></returns>
        public static Task activeEffect(this Card card, CardEngine game, Player player, Card[] targets)
        {
            bool silenced = card.isSilenced(game);
            if (!silenced)
            {
                ITriggerEffect triggerEffect = card.define.getEffectOn<HearthStoneRule.ActiveEventArg>(game.triggers);
                if (triggerEffect != null)
                {
                    var eventArg = new HearthStoneRule.ActiveEventArg(player, card, targets);
                    var vars = new object[] { eventArg };
                    if (triggerEffect.checkCondition(game, card, null, vars))
                        return triggerEffect.execute(game, card, null, vars, targets);
                }
                IActiveEffect activeEffect = card.define.getActiveEffect();
                if (activeEffect != null)
                {
                    var eventArg = new HearthStoneRule.ActiveEventArg(player, card, targets);
                    if (activeEffect.checkCondition(game, card, null, eventArg))
                        return activeEffect.execute(game, card, null, eventArg);
                }
            }
            if (card.isServant() || card.isItem() || card.isMaster())
            {
                return triggerBattlecries(card, game, player, targets);
            }
            else if (card.isSpell() || card.isSkill())
            {
                if (!silenced)
                {
                    var defineActives = card.define.getEffects().OfType<GeneratedActiveEffect>();
                    foreach (var effect in defineActives)
                    {
                        var eventArg = new HearthStoneRule.ActiveEventArg(player, card, targets);
                        if (effect.checkCondition(game, card, null, eventArg))
                            return effect.execute(game, card, null, eventArg);
                    }
                }
                foreach (var buff in card.getBuffs())
                {
                    var buffActives = buff.getEffects(game).OfType<GeneratedActiveEffect>();
                    foreach (var effect in buffActives)
                    {
                        var eventArg = new HearthStoneRule.ActiveEventArg(player, card, targets);
                        if (effect.checkCondition(game, card, null, eventArg))
                            return effect.execute(game, card, null, eventArg);
                    }
                }
            }
            return Task.CompletedTask;
        }
        /// <summary>
        /// 发动特定法术或技能卡牌定义的主动效果
        /// </summary>
        public static Task activeSpellCardDefineEffect(this Card card, CardEngine game, Player player, CardDefine define, Card[] targets)
        {
            if (define.isSpell() || define.isSkill())
            {
                ITriggerEffect triggerEffect = define.getEffectOn<HearthStoneRule.ActiveEventArg>(game.triggers);
                if (triggerEffect != null)
                {
                    var eventArg = new HearthStoneRule.ActiveEventArg(player, card, targets);
                    var vars = new object[] { eventArg };
                    if (triggerEffect.checkCondition(game, card, null, vars))
                        return triggerEffect.execute(game, card, null, vars, targets);
                }
                IActiveEffect activeEffect = define.getActiveEffect();
                if (activeEffect != null)
                {
                    var eventArg = new HearthStoneRule.ActiveEventArg(player, card, targets);
                    if (activeEffect.checkCondition(game, card, null, eventArg))
                        return activeEffect.execute(game, card, null, eventArg);
                }
                var effect = define.getEffects().OfType<GeneratedEffect>().FirstOrDefault();
                if (effect != null)
                {
                    var eventArg = new HearthStoneRule.ActiveEventArg(player, card, targets);
                    if (effect.checkCondition(game, card, null, eventArg))
                        return effect.execute(game, card, null, eventArg);
                }
            }
            return Task.CompletedTask;
        }
        /// <summary> 
        /// 触发一个特定卡牌定义自带的宣言。
        /// </summary>
        /// <param name="card">施放宣言的仆从。</param>
        /// <param name="game">游戏对象。</param>
        /// <param name="player">施放宣言的玩家。</param>
        /// <param name="define">要施放宣言的卡牌定义。</param>
        /// <param name="targets">宣言的目标。</param>
        /// <returns></returns>
        public static async Task triggerCardDefineBattlecries(this Card card, CardEngine game, Player player, CardDefine define, Card[] targets)
        {
            var battlecries = define.getBattlecries();
            var eventArg = new HearthStoneRule.ActiveEventArg(player, card, targets);
            foreach (var battlecry in battlecries)
            {
                if (battlecry is IActiveEffect active)
                {
                    if (active.checkCondition(game, card, null, eventArg))
                        await active.execute(game, card, null, eventArg);
                }
            }
        }
        public static async Task triggerBattlecries(this Card card, CardEngine game, Player player, Card[] targets)
        {
            var battlecries = getBattlecries(card, game);
            var eventArg = new HearthStoneRule.ActiveEventArg(player, card, targets);
            foreach (var (effect, buff) in battlecries)
            {
                if (effect is GeneratedActiveEffect genActive)
                {
                    if (genActive.checkCondition(game, card, buff, eventArg))
                        await genActive.execute(game, card, buff, eventArg);
                }
                else if (effect is ActiveEffect active)
                {
                    if (active.checkCondition(game, card))
                        await active.execute(game, card, buff, eventArg);
                }
            }
        }
        /// <summary>
        /// 触发一个特定卡牌定义自带的遗言。
        /// </summary>
        /// <param name="card">触发遗言的仆从。</param>
        /// <param name="game">游戏对象。</param>
        /// <param name="define">要触发遗言的卡牌定义。</param>
        /// <returns></returns>
        public static async Task triggerCardDefineDeathRattles(this Card card, CardEngine game, CardDefine define)
        {
            var deathRattles = define.getDeathRattles();
            var eventArg = new HearthStoneRule.LastwordEventArg(card.getOwner(), card);
            foreach (var deathRattle in deathRattles)
            {
                if (deathRattle is GeneratedPassiveEffect genPassive)
                {
                    if (genPassive.checkCondition(game, card, null, eventArg))
                        await genPassive.execute(game, card, null, eventArg);
                }
                else if (deathRattle is DeathRattle rattle)
                {
                    await rattle.execute(game, card, null, new DeathEventArg(card));
                }
            }
        }
        public static async Task triggerDeathRattles(this Card card, CardEngine game)
        {
            var deathRattles = getDeathRattles(card, game);
            var eventArg = new HearthStoneRule.LastwordEventArg(card.getOwner(), card);
            foreach (var (effect, buff) in deathRattles)
            {
                if (effect is GeneratedPassiveEffect genPassive)
                {
                    if (genPassive.checkCondition(game, card, buff, eventArg))
                        await genPassive.execute(game, card, buff, eventArg);
                }
                else if (effect is DeathRattle rattle)
                {
                    await rattle.execute(game, card, buff, new DeathEventArg(card));
                }
            }
        }
        public static async Task<AttackEventArg> tryAttack(this Card card, CardEngine game, Player player, Card target)
        {
            if (!card.canAttack(game))
            {
                game.logger.log(card + "无法进行攻击");
                return null;
            }
            if (!card.isAttackable(game, player, target, out var reason))
            {
                game.logger.log(card + "无法攻击" + target + "，因为" + reason);
                return null;
            }
            await card.setAttackTimes(game, card.getAttackTimes(game) + 1);
            return await card.attack(game, target);
        }
        public static Task<AttackEventArg> forceAttack(this Card card, CardEngine game, Player player, Card target)
        {
            if (!card.canForceAttack(game))
            {
                game.logger.log(card + "无法进行强制攻击");
                return Task.FromResult<AttackEventArg>(null);
            }
            if (!card.isForceAttackable(game, player, target))
            {
                game.logger.log(card + "无法对" + target + "进行强制攻击");
                return Task.FromResult<AttackEventArg>(null);
            }
            return card.attack(game, target);
        }
        public static bool canAttackTest(this Card card, CardEngine game)
        {
            if (!(card.pile.name == PILE.FIELD || card.pile.name == PILE.MASTER))//不在战场上
                return false;
            if (card.isDead(game))
                return false;
            return true;
        }
        static async Task<AttackEventArg> attack(this Card card, CardEngine game, Card target)
        {
            var eventArg = await game.triggers.doEvent(new AttackEventArg() { card = card, target = target }, async arg =>
            {
                if (!card.canAttackTest(game))
                    game.logger.log(card + "无法进行攻击");
                else
                {
                    card = arg.card;
                    target = arg.target;
                    game.logger.log(card + "攻击" + target);
                    int targetAttack = target.getAttack(game);
                    int cardAttack = card.getAttack(game);
                    await card.damage(game, target, targetAttack);
                    await target.damage(game, card, cardAttack);
                    //消耗耐久
                    foreach (var p in game.players)
                    {
                        if (p.getMaster() == card && p.getItem() != null)
                        {
                            await p.getItem().damage(game, null, 1);
                            break;
                        }
                    }
                }
            });
            return eventArg;
        }
        public class AttackEventArg : EventArg, IDescribableEventArg
        {
            ICard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return card;
            }
            ICard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return new ICard[] { target };
            }
            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                IDescribableEventArg arg = this;
                return String.Format(arg.localizeTemplateString(game, viewer), arg.localizeStringArgs(game, viewer));
            }

            public override void Record(IGame game, EventRecord record)
            {
                record.setCardState(EVENTVAR.CARD, card);
                record.setCardState(EVENTVAR.TARGET_CARD, target);
            }
            [MukioI18n.TranslateMsg("{0} {1}是卡")]
            const string TEMPLATE = "{0} 攻击 {1}";
            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                return TEMPLATE;
            }
            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                return new object[] {
                    card.getFormatString(),
                    target.getFormatString()
                };
            }

            public override EventVariableInfo[] getBeforeEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card)},
                    new EventVariableInfo(){name = EVENTVAR.TARGET_CARD,type=typeof(Card)}
                };
            }
            public override EventVariableInfo[] getAfterEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card)},
                    new EventVariableInfo(){name = EVENTVAR.TARGET_CARD,type=typeof(Card)}
                };
            }
            public Card card
            {
                get { return getVar<Card>(EVENTVAR.CARD); }
                set { setVar(EVENTVAR.CARD, value); }
            }
            public Card target
            {
                get { return getVar<Card>(EVENTVAR.TARGET_CARD); }
                set { setVar(EVENTVAR.TARGET_CARD, value); }
            }
        }

        public static void setSkillRef(this CardDefine cardDefine, DefineReference defRef)
        {
            if (cardDefine is MasterCardDefine masterCardDefine)
            {
                masterCardDefine.skillID = defRef.defineId;
            }
            else
                cardDefine.setProp(DEFINE.CARD_PROP_SKILL_REF, defRef);
        }
        public static DefineReference getSkillRef(this CardDefine cardDefine)
        {
            if (cardDefine is MasterCardDefine masterCardDefine)
            {
                return new DefineReference(0, masterCardDefine.skillID);
            }
            else if (cardDefine.hasProp(DEFINE.CARD_PROP_SKILL_REF))
            {
                return cardDefine.getProp<DefineReference>(DEFINE.CARD_PROP_SKILL_REF);
            }
            return new DefineReference(cardDefine.cardPoolId, cardDefine.getProp<int>(DEFINE.CARD_PROP_SKILL_ID));
        }
        #region 动作定义
        public static Task<DamageEventArg> damage(this Card card, CardEngine game, Card source, int value)
        {
            if (card == null || value <= 0)
                return Task.FromResult<DamageEventArg>(null);
            var eventArg = new DamageEventArg(card, value, source);
            return game.triggers.doEvent(eventArg, async arg =>
            {
                Card target = arg.target;
                Card src = arg.source;
                int dmgValue = arg.damageValue;
                if (src != null && src.isStealth(game))
                {
                    src.setStealth(game, false);
                }
                if (dmgValue < 1)
                {
                    arg.isCanceled = true;
                }
                else if (target.isImmunity(game))
                {
                    arg.isCanceled = true;
                    game.logger.log(target + "免疫，不受伤害");
                }
                else if (target.isShield(game))
                {
                    arg.isCanceled = true;
                    game.logger.log(target + "受到伤害，失去圣盾");
                    target.setShield(game, false);
                }
                else
                {
                    int damageReduce = target.getDamageReduce(game);
                    int damage;
                    if (damageReduce > 0)
                    {
                        damage = dmgValue - damageReduce;
                        game.logger.log(target + "的伤害抵消" + damageReduce + "使伤害" + dmgValue + "=>" + damage);
                    }
                    else
                        damage = dmgValue;
                    if (damage > 0)
                    {
                        int armor = target.getArmor(game);
                        if (armor > 0)
                        {
                            if (armor >= damage)
                            {
                                await target.loseArmor(game, damage);
                                game.logger.log(target + "受到" + damage + "点伤害，护甲=>" + target.getArmor(game));
                                await onDealKeywords(game, src, target, damage);
                                arg.damagedValue = damage;
                            }
                            else
                            {
                                int leakDamage = damage - armor;
                                await target.loseArmor(game, armor);
                                await target.setCurrentLife(game, target.getCurrentLife(game) - leakDamage);
                                game.logger.log(target + "受到" + damage + "点伤害，护甲=>" + target.getArmor(game) + "，生命值=>" + target.getCurrentLife(game));
                                await onDealKeywords(game, src, target, damage);
                                arg.damagedValue = damage;
                            }
                        }
                        else
                        {
                            await target.setCurrentLife(game, target.getCurrentLife(game) - damage);
                            game.logger.log(target + "受到" + damage + "点伤害，生命值=>" + target.getCurrentLife(game));
                            await onDealKeywords(game, src, target, damage);
                            arg.damagedValue = damage;
                        }
                    }
                    else
                    {
                        arg.isCanceled = true;
                        game.logger.log(target + "的伤害" + dmgValue + "被减伤" + damageReduce + "抵消");
                    }
                }
            });
            async Task onDealKeywords(CardEngine g, Card s, Card c, int d)
            {
                if (s != null)
                {
                    bool poisonous = s.isPoisonous(g);
                    bool lifesteal = s.isDrain(g);
                    if (s.isMaster() && g.getCurrentPlayer() == s.getOwner())
                    {
                        var item = s.getOwner().getItem();
                        if (item != null)
                        {
                            poisonous |= item.isPoisonous(g);
                            lifesteal |= item.isDrain(g);
                        }
                    }
                    if (poisonous && c.isServant())
                        await c.kill(game);
                    if (lifesteal && (c.isServant() || c.isMaster()))
                        await s.getOwner().getMaster().heal(g, c, d);
                }
            }
        }
        #endregion

        #region 返回手牌
        /// <summary>
        /// 将一张卡牌返回到指定玩家的手牌。
        /// </summary>
        /// <param name="card">卡牌。</param>
        /// <param name="game"></param>
        /// <param name="player">移动至手牌的玩家。</param>
        /// <returns></returns>
        public static async Task returnToHand(this Card card, CardEngine game, Player player)
        {
            if (card == null || player == null)
                return;
            var from = card.pile;
            var to = player.getHand();
            if (!to.isFull) // 手牌未满
            {
                await Pile.moveTo(game, card, from, to, to.count);
                game.logger.log("将" + card + "置入" + player + "的手牌");
            }
            else // 手牌已满
            {
                if (from?.name == PILE.FIELD) // 仆从直接将其消灭
                {
                    await kill(card, game);
                }
                else //否则移入墓地。
                {
                    var grave = player.getGrave();
                    if (from != grave)
                    {
                        await Pile.moveTo(game, card, from, grave, grave.count);
                    }
                }
                game.logger.log(player + "的手牌已满，无法将" + card + "置入手牌");
            }
        }
        #endregion

        #region 弃牌

        public static Task<DiscardEventArg> discard(this Card card, CardEngine game)
        {
            if (card == null)
                return default;
            return game.triggers.doEvent(new DiscardEventArg() { player = card.getOwner(), card = card }, arg =>
            {
                Player actualPlayer = arg.player;
                Card actualCard = arg.card;
                if (actualPlayer == null)
                    return Task.CompletedTask;
                if (actualCard == null)
                    return Task.CompletedTask;
                return actualPlayer.getHand().moveTo(game, actualCard, actualPlayer.getGrave());
            });
        }
        public async static Task<DiscardEventArg[]> discard(this IEnumerable<Card> cards, CardEngine game)
        {
            List<DiscardEventArg> eventArgs = new List<DiscardEventArg>();
            foreach (Card card in cards)
            {
                if (card == null)
                    continue;
                eventArgs.Add(await card.discard(game));
            }
            return eventArgs.ToArray();
        }

        public class DiscardEventArg : EventArg, IDescribableEventArg
        {
            ICard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return null;
            }

            ICard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return new ICard[] { card };
            }

            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                IDescribableEventArg arg = this;
                return string.Format(arg.localizeTemplateString(game, viewer), arg.localizeStringArgs(game, viewer));
            }

            [MukioI18n.TranslateMsg("{0}是玩家，{1}是卡牌列表")]
            const string TEMPLATE = "{0} 丢弃 {1}";
            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                return TEMPLATE;
            }
            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                return new object[] {
                    player.name, card.getFormatString()
                };
            }

            public override EventVariableInfo[] getBeforeEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)},
                    new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card)}
                };
            }
            public override EventVariableInfo[] getAfterEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)},
                    new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card)}
                };
            }
            public override void Record(IGame game, EventRecord record)
            {
                record.setCardState(EVENTVAR.CARD, card);
                record.setVar(EVENTVAR.PLAYER, player);
            }
            public Player player
            {
                get { return getVar<Player>(EVENTVAR.PLAYER); }
                set { setVar(EVENTVAR.PLAYER, value); }
            }
            public Card card
            {
                get { return getVar<Card>(EVENTVAR.CARD); }
                set { setVar(EVENTVAR.CARD, value); }
            }
        }
        #endregion
        public static async Task<DamageEventArg[]> damage(this IEnumerable<Card> cards, CardEngine game, Card source, int value)
        {
            if (cards.Count() <= 0)
            {
                return default;
            }
            List<DamageEventArg> args = new List<DamageEventArg>();
            foreach (var target in cards.Where(p => p != null))
            {
                args.Add(await target.damage(game, source, value));
            }
            return args.ToArray();
        }
        public static async Task<DamageEventArg[]> damageByRandom(this IEnumerable<Card> cards, CardEngine game, Card source, int value)
        {
            List<DamageEventArg> list = new List<DamageEventArg>();
            for (int i = 0; i < value; i++)
            {
                Card target = cards.Where(c => !c.isDead(game)).random(game);
                if (target != null)
                {
                    list.Add(await target.damage(game, source, 1));
                }
            }
            return list.ToArray();
        }
        public static Task<DamageEventArg[]> damageByRandomCount(this IEnumerable<Card> cards, CardEngine game, Card source, int value, int count)
        {
            if (cards.Count() > count)
                cards = cards.randomTake(game, count);
            return cards.damage(game, source, value);

        }
        public class DamageEventArg : EventArg, IDescribableEventArg
        {
            public Card target
            {
                get { return getVar<Card>(EVENTVAR.CARD); }
                set { setVar(EVENTVAR.CARD, value); }
            }
            public Card source
            {
                get { return getVar<Card>(VAR_DAMAGE_SOURCE); }
                set { setVar(VAR_DAMAGE_SOURCE, value); }
            }
            public int damageValue
            {
                get { return getVar<int>(VAR_DAMAGE_VALUE); }
                set { setVar(VAR_DAMAGE_VALUE, value); }
            }
            public int damagedValue
            {
                get { return getVar<int>(VAR_DAMAGED_VALUE); }
                set { setVar(VAR_DAMAGED_VALUE, value); }
            }
            public DamageEventArg()
            {

            }
            public DamageEventArg(Card target, int value, Card source)
            {
                this.target = target;
                this.source = source;
                damageValue = value;
                damagedValue = 0;
            }
            ICard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return source;
            }
            ICard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return new ICard[] { target };
            }
            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                return source != null ?
                    source.getFormatString() + "对" + target.getFormatString() + "造成" + damagedValue + "点伤害" :
                    target.getFormatString() + "受到" + damagedValue + "点伤害";
            }

            [MukioI18n.TranslateMsg("{0}是卡，{1}是伤害值")]
            const string TEMPLATE_RECV = "{0} 受到 {1} 点伤害";
            [MukioI18n.TranslateMsg("{0}{1}是卡，{2}是伤害值")]
            const string TEMPLATE_ATK = "{0} 对 {1} 造成 {2} 点伤害";
            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                return source != null ? TEMPLATE_ATK : TEMPLATE_RECV;
            }
            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                if (source != null)
                {
                    return new object[] {
                        source.getFormatString(),
                        target.getFormatString(),
                        damagedValue
                    };
                }
                else
                {
                    return new object[] {
                        target.getFormatString(),
                        damagedValue
                    };
                }
            }
            public override EventVariableInfo[] getBeforeEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card)},
                    new EventVariableInfo(){name = VAR_DAMAGE_SOURCE,type=typeof(Card)},
                    new EventVariableInfo(){name = VAR_DAMAGE_VALUE,type=typeof(int)}
                };
            }
            public override EventVariableInfo[] getAfterEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card)},
                    new EventVariableInfo(){name = VAR_DAMAGE_SOURCE,type=typeof(Card)},
                    new EventVariableInfo(){name = VAR_DAMAGE_VALUE,type=typeof(int)},
                    new EventVariableInfo(){name = VAR_DAMAGED_VALUE,type=typeof(int)}
                };
            }
            public override void Record(IGame game, EventRecord record)
            {
                record.setCardState(EVENTVAR.CARD, target);
                record.setCardState(VAR_DAMAGE_SOURCE, source);
                record.setVar(VAR_DAMAGE_VALUE, damageValue);
                record.setVar(VAR_DAMAGED_VALUE, damagedValue);
            }
            public const string VAR_DAMAGE_SOURCE = "伤害来源";
            public const string VAR_DAMAGE_VALUE = "伤害值";
            public const string VAR_DAMAGED_VALUE = "实际伤害值";
        }

        public static bool isDamage(this Card card, CardEngine game)
        {
            if (card.getCurrentLife(game) < card.getLife(game))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static async Task<HealEventArg[]> heal(this IEnumerable<Card> cards, CardEngine game, Card sourse, int value)
        {
            List<HealEventArg> eventArgs = new List<HealEventArg>();
            foreach (var card in cards)
            {
                var arg = await heal(card, game, sourse, value);
                if (arg == null)
                {
                    continue;
                }
                eventArgs.Add(arg);
            }
            return eventArgs.ToArray();
        }
        public static Task<HealEventArg> heal(this Card card, CardEngine game, Card sourse, int value)
        {
            if (card == null)
                return null;
            if (value <= 0)
                return null;
            return game.triggers.doEvent(new HealEventArg() { card = card, source = sourse, value = value }, async arg =>
            {
                card = arg.card;
                sourse = arg.source;
                value = arg.value;
                if (card == null)
                    return;
                int healedValue = Math.Min(value, card.getLife(game) - card.getCurrentLife(game));
                if (healedValue > 0)
                {
                    await card.setCurrentLife(game, card.getCurrentLife(game) + healedValue);
                    arg.healedValue = healedValue;
                    game.logger.log(card + "恢复" + healedValue + "点生命值，生命值=>" + card.getCurrentLife(game));
                }
                else
                {
                    arg.healedValue = 0;
                    game.logger.log(card + "没有恢复生命值，生命值=>" + card.getCurrentLife(game));
                }
            });

        }
        public class HealEventArg : EventArg, IDescribableEventArg
        {

            ICard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return card;
            }

            ICard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return null;
            }

            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                return string.Format(TEMPLATE, card.getFormatString(), healedValue);
            }
            public override void Record(IGame game, EventRecord record)
            {
                record.setCardState(EVENTVAR.CARD, card);
                record.setCardState(VAR_HEAL_SOURCE, source);
                record.setVar(EVENTVAR.VALUE, value);
                record.setVar(VAR_HEALED_VALUE, healedValue);
            }
            public override EventVariableInfo[] getBeforeEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card)},
                    new EventVariableInfo(){name = VAR_HEAL_SOURCE,type=typeof(Card)},
                    new EventVariableInfo(){name = EVENTVAR.VALUE,type=typeof(int)},
                };
            }
            public override EventVariableInfo[] getAfterEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card)},
                    new EventVariableInfo(){name = VAR_HEAL_SOURCE,type=typeof(Card)},
                    new EventVariableInfo(){name = EVENTVAR.VALUE,type=typeof(int)},
                    new EventVariableInfo(){name = VAR_HEALED_VALUE,type=typeof(int)},
                };
            }
            [MukioI18n.TranslateMsg("{0}是卡，{1}是治疗量")]
            const string TEMPLATE = "治疗 {0} {1} 点生命值";
            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                return TEMPLATE;
            }
            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                return new object[] { card.getFormatString(), healedValue };
            }
            public Card source
            {
                get => getVar<Card>(VAR_HEAL_SOURCE);
                set => setVar(VAR_HEAL_SOURCE, value);
            }
            public Card card
            {
                get => getVar<Card>(EVENTVAR.CARD);
                set => setVar(EVENTVAR.CARD, value);
            }
            public int value
            {
                get => getVar<int>(EVENTVAR.VALUE);
                set => setVar(EVENTVAR.VALUE, value);
            }
            public int healedValue
            {
                get => getVar<int>(VAR_HEALED_VALUE);
                set => setVar(VAR_HEALED_VALUE, value);
            }

            public const string VAR_HEAL_SOURCE = "治疗来源";
            public const string VAR_HEALED_VALUE = "实际治疗值";
        }
        /// <summary>
        /// 将单个卡牌移除并将其置入墓地，然后触发它的遗言。
        /// </summary>
        /// <param name="card">卡牌。</param>
        /// <param name="game">游戏对象。</param>
        /// <returns>死亡事件。</returns>
        public static async Task<DeathEventArg> destroy(this Card card, CardEngine game)
        {
            var args = await new Card[] { card }.destroy(game);
            return args[0];
        }

        /// <summary>
        /// 将多个卡牌移除并将其置入墓地，然后触发它们的遗言。
        /// </summary>
        /// <param name="cards">卡牌列表。</param>
        /// <param name="game">游戏对象。</param>
        /// <returns>死亡事件。</returns>
        public static async Task<DeathEventArg[]> destroy(this IEnumerable<Card> cards, CardEngine game)
        {
            List<Player> remainPlayerList = new List<Player>(game.players);

            // 禁用卡牌不在墓地中触发的效果。
            // 用于解决“在一个友方仆从死亡后”，拥有诸如此类效果的仆从，如果和其他仆从同时死亡，这个也会触发的问题。
            foreach (Card card in cards)
            {
                // 卡牌定义自带的效果。
                foreach (var effect in card.define.getEffects().OfType<IPileRangedEffect>().Where(e => !e.piles.Contains(PILE.GRAVE) && !e.isDeathRattle()))
                {
                    await effect.onDisable(game, card, null);
                }
                // BUFF给予的效果。
                foreach (var buff in card.getBuffs())
                {
                    foreach (var effect in buff.getEffects(game).OfType<IPileRangedEffect>().Where(e => !e.piles.Contains(PILE.GRAVE) && !e.isDeathRattle()))
                    {
                        await effect.onDisable(game, card, buff);
                    }
                }
            }
            // 触发死亡事件。
            List<DeathEventArg> eventArgs = new List<DeathEventArg>();
            List<Card> successfullyDied = new List<Card>();
            foreach (Card card in cards)
            {
                var eventArg = await game.triggers.doEvent(new DeathEventArg(card), onDie);
                async Task onDie(DeathEventArg arg)
                {
                    Card death = arg.card;
                    if (death.pile.name == PILE.MASTER)//英雄阵亡
                    {
                        remainPlayerList.Remove(death.getOwner());
                        game.logger.log(death.getOwner() + "失败");
                    }
                    else if (death.pile.name == PILE.FIELD)//随从阵亡
                    {
                        await death.pile.moveTo(game, death, death.getOwner().getGrave());
                        game.logger.log(death + "阵亡");
                    }
                    else if (death.pile.name == PILE.ITEM)//装备摧毁
                    {
                        await death.pile.moveTo(game, death, death.getOwner().getGrave());
                        game.logger.log(death + "摧毁");
                    }
                    successfullyDied.Add(death);
                }
                eventArgs.Add(eventArg);
            }

            // 触发遗言。
            try
            {
                foreach (Card card in successfullyDied)
                {
                    // 只在战场、墓地或武器栏位触发遗言。
                    if ((card.pile?.name == PILE.GRAVE || card.pile?.name == PILE.FIELD || card.pile?.name == PILE.ITEM) && card.hasDeathRattle(game))
                    {
                        await card.triggerDeathRattles(game);
                        await card.resetCard(game);
                    }
                }
            }
            catch (Exception e)
            {
                game.logger?.logError("Effect", "触发遗言时引发异常：" + e);
            }
            if (remainPlayerList.Count != game.players.Length)
            {
                if (remainPlayerList.Count > 0)
                    await game.getRule().gameEnd(game, remainPlayerList.ToArray()); //TODO: 把这个方法移动到HearthStoneRule之后记得把getRule删了
                else
                    await game.getRule().gameEnd(game, new Player[0]);//TODO: 把这个方法移动到HearthStoneRule之后记得把getRule删了
            }
            return eventArgs.ToArray();
        }
        public class DeathEventArg : EventArg, IDescribableEventArg
        {

            public DeathEventArg()
            {
            }
            public DeathEventArg(Card card)
            {
                this.card = card;
                player = card.getOwner();
                position = card.pile.indexOf(card);
            }
            ICard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return card;
            }

            ICard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return card;
            }

            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                IDescribableEventArg arg = this;
                return string.Format(arg.localizeTemplateString(game, viewer), arg.localizeStringArgs(game, viewer));
            }

            [TranslateMsg("{0}是卡牌列表")]
            const string TEMPLATE = "{0} 死亡";
            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                return TEMPLATE;
            }
            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                return new object[] {
                    card.getFormatString()
                };
            }

            public override EventVariableInfo[] getBeforeEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card)},
                    new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)},
                    new EventVariableInfo(){name = EVENTVAR.POSITION,type=typeof(int)}
                };
            }
            public override EventVariableInfo[] getAfterEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card)},
                    new EventVariableInfo(){name = EVENTVAR.PLAYER,type=typeof(Player)},
                    new EventVariableInfo(){name = EVENTVAR.POSITION,type=typeof(int)}
                };
            }
            public override void Record(IGame game, EventRecord record)
            {
                record.setCardState(EVENTVAR.CARD, card);
                record.setVar(EVENTVAR.PLAYER, player);
                record.setVar(EVENTVAR.POSITION, position);
            }
            public Card card
            {
                get => getVar<Card>(EVENTVAR.CARD);
                set => setVar(EVENTVAR.CARD, value);
            }
            public Player player
            {
                get => getVar<Player>(EVENTVAR.PLAYER);
                set => setVar(EVENTVAR.PLAYER, value);
            }
            public int position
            {
                get => getVar<int>(EVENTVAR.POSITION);
                set => setVar(EVENTVAR.POSITION, value);
            }
        }
        public static Card[] getNearbyCards(this Card card)
        {
            if (card.pile == null || card.pile.count < 2)
                return new Card[0];
            int index = card.pile.indexOf(card);
            if (index == 0 && card.pile.count > 1)
                return new Card[] { card.pile[1] };
            if (index == card.pile.count - 1 && card.pile.count > 1)
                return new Card[] { card.pile[card.pile.count - 2] };
            return new Card[] { card.pile[index - 1], card.pile[index + 1] };
        }

        /// <summary>
        /// 这个随从的可选攻击目标（法术应该不会在场上出现吧
        /// 本来是写给恋恋的，现在只能弃用了
        /// </summary>
        /// <param name="card"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        public static List<Card> getAttackableTargets(this Card card, CardEngine game)
        {
            var targets = new List<Card>();
            if (card.pile != card.getOwner().getField() && !card.isMaster())
            {
                return targets;
            }

            var oppo = game.getOpponent(card.getOwner());
            var allEnem = oppo.getField().ToList().Append(oppo.getMaster()).Where(c => !c.isStealth(game)).ToList();
            if (allEnem.Any(c => c.hasKeyword(game, KEYWORD.TAUNT)))
            {
                targets.AddRange(allEnem.Where(c => c.hasKeyword(game, KEYWORD.TAUNT)));
            }
            else
            {
                targets.AddRange(allEnem);
            }

            return targets;
        }

        public static Task fieldToHand(this Card card, CardEngine game)
        {
            return fieldToHand(new Card[] { card }, game);
        }
        public static async Task fieldToHand(this IEnumerable<Card> cards, CardEngine game)
        {
            foreach (IGrouping<Player, Card> g in cards.GroupBy(c => c.getOwner()))
            {
                foreach (var card in g)
                {
                    await card.returnToHand(game, g.Key);
                }
            }
        }
        /// <summary>
        /// 获得一个仆从的控制权。
        /// </summary>
        /// <param name="card"></param>
        /// <param name="game"></param>
        /// <param name="player"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static async Task takeControl(this Card card, CardEngine game, Player player, int position = -1)
        {
            if (card.pile?.name != PILE.FIELD) // 不在场上
                return;
            if (card.pile == player.getField())// 目标和源相同
                return;
            if (player.getField().isFull)//没位置了
            {
                await card.kill(game);
                return;
            }

            if (position < 0)
                position = player.getField().count;
            await card.pile.moveTo(game, card, player.getField(), position);
            await card.setReady(game, false);
        }

        /// <summary>
        /// 场上回牌库
        /// </summary>
        /// <param name="card"></param>
        /// <param name="game"></param>
        /// <param name="isYourDeck"></param>
        /// <param name="random">若为 false, 将移动到牌堆顶</param>
        /// <returns></returns>
        public static Task fieldToDeck(this Card card, CardEngine game, bool random)
        {
            return fieldToDeck(new Card[] { card }, game, random);
        }
        /// <summary>
        /// 场上回牌库
        /// </summary>
        /// <param name="cards"></param>
        /// <param name="game"></param>
        /// <param name="isYourDeck"></param>
        /// <param name="random">若为 false, 将移动到牌堆顶</param>
        /// <returns></returns>
        public static async Task fieldToDeck(this IEnumerable<Card> cards, CardEngine game, bool random)
        {
            foreach (var g in cards.GroupBy(c => c.getOwner()))
            {
                // 场上随从没死 = 没重置，所以这里手动重置
                g.resetCard(game);
                foreach (var card in g)
                {
                    int position = random ? game.randomInt(0, g.Key.getDeck().count) : g.Key.getDeck().count;
                    g.Key.getField().moveTo(game, card, g.Key.getDeck(), position);
                }
            }
            //foreach(Card c in cards)
            //{              
            //    THHPlayer player = c.getOwner();
            //    if (!isYourDeck)
            //    {
            //        player = game.getOpponent(c.getOwner());
            //    }
            //    index = game.randomInt(0, player.deck.count);
            //    await c.resetCard(game);
            //    await c.getOwner().field.moveTo(game, c, player.deck, index);
            //}
            // foreach (IGrouping<THHPlayer, Card> g in cards.GroupBy(c => c.getOwner()))
            // {
            //     int restCount = g.Key.field.count;
            //     THHPlayer player = g.Key;
            //     if (!isYourDeck)
            //     {
            //         player = game.getOpponent(g.Key);
            //     }
            //     index = game.randomInt(0, player.deck.count);
            //     var c = g.Take(restCount);
            //     await c.resetCard(game);
            //     await g.Key.field.moveTo(game, c, player.deck, index);
            // }

        }

        /// <summary>
        /// 墓地回手
        /// </summary>
        /// <param name="card"></param>
        /// <param name="game"></param>
        /// <param name="random">若为 false, 将移动到牌堆顶</param>
        /// <returns></returns>
        public static Task graveToHand(this Card card, CardEngine game)
        {
            return graveToHand(new Card[] { card }, game);
        }

        /// <summary>
        /// 墓地回手
        /// </summary>
        /// <param name="cards"></param>
        /// <param name="game"></param>
        /// <param name="random">若为 false, 将移动到牌堆顶</param>
        /// <returns></returns>
        public static async Task graveToHand(this IEnumerable<Card> cards, CardEngine game)
        {
            foreach (IGrouping<Player, Card> g in cards.GroupBy(c => c.getOwner()))
            {
                foreach (var card in g)
                {
                    await card.returnToHand(game, g.Key);
                }
            }
        }

        /// <summary>
        /// 墓地回牌库
        /// </summary>
        /// <param name="card"></param>
        /// <param name="game"></param>
        /// <param name="random">若为 false, 将移动到牌堆顶</param>
        /// <returns></returns>
        public static Task graveToDeck(this Card card, CardEngine game, bool random)
        {
            return graveToDeck(new Card[] { card }, game, random);
        }

        /// <summary>
        /// 墓地回牌库
        /// </summary>
        /// <param name="cards"></param>
        /// <param name="game"></param>
        /// <param name="random">若为 false, 将移动到牌堆顶</param>
        /// <returns></returns>
        public static async Task graveToDeck(this IEnumerable<Card> cards, CardEngine game, bool random)
        {
            foreach (IGrouping<Player, Card> g in cards.GroupBy(c => c.getOwner()))
            {
                foreach (var card in g)
                {
                    int position = random ? game.randomInt(0, g.Key.getDeck().count) : g.Key.getDeck().count;
                    g.Key.getGrave().moveTo(game, g.Take(g.Count()), g.Key.getDeck(), position);
                }
            }
        }

        public static async Task removeBuff(this IEnumerable<Card> cards, IGame game, int buffId)
        {
            foreach (var card in cards)
            {
                await card.removeBuff(game, card.getBuffs(b => b.id == buffId));
            }
        }
        /// <summary>
        /// 触发奥秘
        /// </summary>
        /// <param name="card"></param>
        /// <param name="game"></param>
        /// <param name="effects"></param>
        /// <returns></returns>
        public static Task<SecretTriggerEventArg> TriggerSecret(this Card card, IGame game)
        {
            return game.triggers.doEvent(new SecretTriggerEventArg() { card = card }, async arg =>
            {
                game.logger.log("触发陷阱" + card);
                Player player = arg.card.getOwner();
                await player.getHead().moveTo(game, arg.card, player.getGrave());
            });
        }
        public class SecretTriggerEventArg : EventArg, IDescribableEventArg
        {
            ICard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return card;
            }
            ICard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return null;
            }
            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                IDescribableEventArg arg = this;
                return String.Format(arg.localizeTemplateString(game, viewer), arg.localizeStringArgs(game, viewer));
            }

            [MukioI18n.TranslateMsg("{0}是卡")]
            const string TEMPLATE = "奥秘 {0} 被触发";
            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                return TEMPLATE;
            }
            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                return new object[] { card.getFormatString() };
            }

            public override EventVariableInfo[] getBeforeEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card)}
                };
            }
            public override EventVariableInfo[] getAfterEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card)}
                };
            }
            public override void Record(IGame game, EventRecord record)
            {
                record.setCardState(EVENTVAR.CARD, card); 
            }
            public Card card
            {
                get { return getVar<Card>(EVENTVAR.CARD); }
                set { setVar(EVENTVAR.CARD, value); }
            }
        }

        /// <summary>
        /// 任务完成
        /// </summary>
        /// <param name="card"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        public static Task<MissionCompleteEventArg> MissionComplete(this Card card, IGame game)
        {
            return game.triggers.doEvent(new MissionCompleteEventArg() { card = card }, async arg =>
            {
                game.logger.log("任务完成 : " + card);
                Player player = arg.card.getOwner();
                await player.getHead().moveTo(game, arg.card, player.getGrave());
            });
        }
        public class MissionCompleteEventArg : EventArg, IDescribableEventArg
        {
            public Card card;
            ICard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return card;
            }
            ICard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return null;
            }
            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                IDescribableEventArg arg = this;
                return String.Format(arg.localizeTemplateString(game, viewer), arg.localizeStringArgs(game, viewer));
            }
            public override void Record(IGame game, EventRecord record)
            {
                record.setCardState(EVENTVAR.CARD, card);
            }

            [MukioI18n.TranslateMsg("{0}是卡")]
            const string TEMPLATE = "任务 {0} 完成";
            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                return TEMPLATE;
            }
            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                return new object[] { card.getFormatString() };
            }

            public override EventVariableInfo[] getBeforeEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card)}
                };
            }
            public override EventVariableInfo[] getAfterEventVarInfos()
            {
                return new EventVariableInfo[]
                {
                    new EventVariableInfo(){name = EVENTVAR.CARD,type=typeof(Card)}
                };
            }
        }

        /// <summary>
        /// 变形
        /// </summary>
        /// <param name="card"></param>
        /// <param name="game"></param>
        /// <param name="define"></param>
        /// <param name="isreset"></param>
        public static async Task<Card.SetDefineEventArg> setDefine(this Card card, CardEngine game, CardDefine define, bool isreset = true)
        {
            var eventArg = await card.setDefine(game, define);
            if (isreset) { await card.resetCard(game); }//卡牌重置
            return eventArg;
        }
    }
}