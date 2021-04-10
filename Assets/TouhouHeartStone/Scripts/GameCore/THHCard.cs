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
        public static bool isItem(this Card card)
        {
            return card.define is ItemCardDefine;
        }
        public static bool isSkill(this Card card)
        {
            return card.define is SkillCardDefine;
        }
        public static bool isServant(this Card card)
        {
            return card.define is ServantCardDefine;
        }
        public static bool isMaster(this Card card)
        {
            return card.define is MasterCardDefine;
        }
        public static bool isSpell(this Card card)
        {
            return card.define is SpellCardDefine;
        }
        public static THHPlayer getOwner(this Card card)
        {
            return card.owner as THHPlayer;
        }
        public static int getCost(this Card card, IGame game)
        {
            int result = card.getProp<int>(game, nameof(ServantCardDefine.cost));
            if (result < 0)
                return 0;
            return result;
        }
        public static Task<IPropChangeEventArg> setCost(this Card card, IGame game, int value)
        {
            return card.setProp(game, nameof(ServantCardDefine.cost), value);
        }
        public static int getCost(this CardDefine card)
        {
            return card.getProp<int>(nameof(ServantCardDefine.cost));
        }
        public static int getAttack(this Card card, IGame game)
        {
            THHGame tgame = (THHGame)game;
            int result = card.getProp<int>(game, PROPERTY.ATTACK);
            if (card.getOwner() == tgame.currentPlayer)
            {
                if (card.define is MasterCardDefine)
                {
                    if (card.getOwner().getItem() != null)
                        result += card.getOwner().getItem().getAttack(game);
                }
            }
            if (result < 0)
                return 0;
            return result;
        }
        public static int getAttack(this CardDefine card)
        {
            int result = card.getProp<int>(PROPERTY.ATTACK);
            if (result < 0)
                return 0;
            return result;

        }
        public static Task<IPropChangeEventArg> setAttack(this Card card, IGame game, int value)
        {
            return card.setProp(game, PROPERTY.ATTACK, value);
        }
        public static int getLife(this Card card, IGame game)
        {
            return card.getProp<int>(game, nameof(ServantCardDefine.life));
        }
        public static int getLife(this CardDefine card)
        {
            return card.getProp<int>(nameof(ServantCardDefine.life));
        }
        public static Task<IPropChangeEventArg> setLife(this Card card, IGame game, int value)
        {
            return card.setProp(game, nameof(ServantCardDefine.life), value);
        }
        /// <summary>
        /// 一个随从/物品/英雄是否已经死了？除了生命值小于等于0以外，还有一个isDead的属性来控制卡片是否在死亡结算当中。
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public static bool isDead(this Card card, IGame game)
        {
            if (card.getCurrentLife(game) <= 0)
                return true;
            if (card.getLife(game) <= 0)
                return true;
            if (card.getProp<bool>(game, nameof(THHCard.isDead)))
                return true;
            return false;
        }
        /// <summary>
        /// 设置为true则随从会在下一次死亡结算中视作阵亡。
        /// </summary>
        /// <param name="card"></param>
        /// <param name="value"></param>
        public static Task<IPropChangeEventArg> setDead(this Card card, IGame game, bool value)
        {
            return card.setProp(game, nameof(THHCard.isDead), value);
        }
        public static async Task<IPropChangeEventArg[]> setDead(this IEnumerable<Card> cards, IGame game, bool value)
        {
            List<IPropChangeEventArg> argList = new List<IPropChangeEventArg>();
            foreach (var card in cards)
            {
                argList.Add(await card.setProp(game, nameof(THHCard.isDead), value));
            }
            return argList.ToArray();
        }
        public static int getArmor(this Card card, IGame game)
        {
            return card.getProp<int>(game, PROPERTY.ARMOR);
        }
        public static Task<SetArmorEventArg> gainArmor(this Card card, THHGame game, int value)
        {
            return setArmor(card, game, card.getArmor(game) + value);
        }
        public static Task<SetArmorEventArg> loseArmor(this Card card, THHGame game, int value)
        {
            return card.setArmor(game, card.getArmor(game) - value);
        }
        public static Task<SetArmorEventArg> setArmor(this Card card, THHGame game, int value)
        {
            return game.triggers.doEvent(new SetArmorEventArg() { card = card, value = value }, arg =>
            {
                card = arg.card;
                value = arg.value;
                if (value < 0)
                    value = 0;
                card.setProp(game, PROPERTY.ARMOR, value);
                return Task.CompletedTask;
            });
        }
        [Obsolete]
        public class SetArmorEventArg : EventArg, IDescribableEventArg
        {
            public Card card;
            public int value;
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
        }
        public static int getCurrentLife(this Card card, IGame game)
        {
            return card.getProp<int>(game, PROPERTY.CURRENT_LIFE);
        }
        public static Task<IPropChangeEventArg> setCurrentLife(this Card card, IGame game, int value)
        {
            return card.setProp(game, PROPERTY.CURRENT_LIFE, value);
        }
        public static bool isReady(this Card card, IGame game)
        {
            return card.getProp<bool>(game, "isReady");
        }
        public static Task<IPropChangeEventArg> setReady(this Card card, IGame game, bool value)
        {
            return card.setProp(game, "isReady", value);
        }
        public static int getAttackTimes(this Card card, IGame game)
        {
            return card.getProp<int>(game, "attackTimes");
        }
        public static Task<IPropChangeEventArg> setAttackTimes(this Card card, IGame game, int value)
        {
            return card.setProp(game, "attackTimes", value);
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
        public static bool canAttack(this Card card, THHGame game)
        {
            if (card.hasKeyword(game, KEYWORD.CANTATTACK))//不能攻击
                return false;
            if (card.getAttack(game) <= 0)//没有攻击力
                return false;
            if (card.pile.name == PILE.FIELD)
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
        public static bool canForceAttack(this Card card, THHGame game)
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
        public static bool isAttackable(this Card card, THHGame game, THHPlayer player, Card target, out string tip)
        {
            if (target == player.getMaster() || player.getField().Contains(target))
            {
                tip = INFO_CANNOT_ATTACK_SELF;
                return false;
            }
            if (game.getOpponent(player).getField().Any(c => c.isTaunt(game)) && !target.isTaunt(game))
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
        public static bool isForceAttackable(this Card card, THHGame game, THHPlayer player, Card target)
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
        public static bool isUsed(this Card card, IGame game)
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
        public static bool isTaunt(this Card card, IGame game)
        {
            return card.hasKeyword(game, KEYWORD.TAUNT);
        }
        public static bool isCharge(this Card card, IGame game)
        {
            return card.hasKeyword(game, KEYWORD.CHARGE);
        }
        public static bool isRush(this Card card, IGame game)
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
        public static bool isShield(this Card card, IGame game)
        {
            return card.hasKeyword(game, KEYWORD.SHIELD);
        }
        public static void setShield(this Card card, IGame game, bool value)
        {
            if (value)
                card.addKeywords(game, KEYWORD.SHIELD);
            else
                card.removeKeywords(game, KEYWORD.SHIELD);
        }
        public static bool isStealth(this Card card, IGame game)
        {
            return card.hasKeyword(game, KEYWORD.STEALTH) || card.hasKeyword(game, KEYWORD.PERMANENTSTEALTH);
        }
        public static void setStealth(this Card card, IGame game, bool value)
        {
            if (value)
                card.addKeywords(game, KEYWORD.STEALTH);
            else
                card.removeKeywords(game, KEYWORD.STEALTH);
        }
        public static bool isDrain(this Card card, IGame game)
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
        public static bool isPoisonous(this Card card, IGame game)
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
        public static bool isElusive(this Card card, IGame game)
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
        public static bool isFreeze(this Card card, IGame game)
        {
            return card.hasKeyword(game, KEYWORD.FREEZE);
        }
        public static Task<IPropChangeEventArg> setFreeze(this Card card, IGame game, bool value)
        {
            if (value)
                return card.addKeywords(game, KEYWORD.FREEZE);
            else
                return card.removeKeywords(game, KEYWORD.FREEZE);
        }
        public static void setFreeze(this IEnumerable<Card> cards, IGame game, bool value)
        {
            foreach (var card in cards)
            {
                card.setFreeze(game, value);
            }
        }
        public static bool isWindfury(this Card card, IGame game)
        {
            return card.hasKeyword(game, KEYWORD.WINDFURY);
        }
        public static void setWindfury(this Card card, IGame game, bool value)
        {
            if (value)
                card.addKeywords(game, KEYWORD.WINDFURY);
            else
                card.removeKeywords(game, KEYWORD.WINDFURY);
        }
        public static bool isImmunity(this Card card, IGame game)
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
        public static bool isUnique(this Card card, IGame game)
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
            var result = await card.setProp(game, PROPERTY.KEYWORDS, keywords);
            foreach (var keyword in card.getKeywords(game))
            {
                await card.setProp(game, keyword, true);
            }
            return result;
        }
        public static string[] getKeywords(this Card card, IGame game)
        {
            return card.getProp<string[]>(game, PROPERTY.KEYWORDS);
        }
        public static bool hasKeyword(this Card card, IGame game, string keyword)
        {
            string[] keywords = card.getKeywords(game);
            if (keywords == null)
            {
                return false;
            }
            return keywords.Contains(keyword);
        }
        public static bool hasKeyword(this CardDefine define, string keyword)
        {
            string[] keywords = define.getProp<string[]>(PROPERTY.KEYWORDS);
            if (keywords == null)
                return false;
            return keywords.Contains(keyword);
        }
        public static Task<IPropChangeEventArg> addKeywords(this Card card, IGame game, params string[] keywords)
        {
            string[] oldKeywords = card.getKeywords(game);
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
            List<string> keywordList = new List<string>(card.getKeywords(game));
            foreach (var keyword in keywords)
            {
                if (keywordList.Contains(keyword))
                {
                    keywordList.Remove(keyword);
                }
            }
            return card.setKeywords(game, keywordList.ToArray());
        }
        public static int getSpellDamage(this Card card, IGame game)
        {
            return card.getProp<int>(game, nameof(ServantCardDefine.spellDamage));
        }
        public static Task<IPropChangeEventArg> setSpellDamage(this Card card, IGame game, int value)
        {
            return card.setProp(game, nameof(ServantCardDefine.spellDamage), value);
        }
        public static int getDamageReduce(this Card card, IGame game)
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
        public static Task<SilenceEventArg> silence(this IEnumerable<Card> cards, THHGame game)
        {
            SilenceEventArg eventArg = new SilenceEventArg() { cards = cards.ToArray() };
            return game.triggers.doEvent(eventArg, async arg =>
            {
                cards = arg.cards;
                foreach (var card in cards)
                {
                    await card.setProp(game, PROPERTY.SILENCE, true);
                    await card.removeBuff(game, card.getBuffs());
                    foreach (var effect in card.define.effects.OfType<IPassiveEffect>())
                    {
                        await effect.onDisable(game, card, null);
                    }
                    await card.setKeywords(game, new string[0]);
                    
                }
            });
        }
        public class SilenceEventArg : EventArg, IDescribableEventArg
        {
            public Card[] cards;

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
            const string TEMPLATE = "{0}被封印";
            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                return TEMPLATE;
            }
            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                return new object[] { string.Join("，", cards.Select(c => c.getFormatString())) };
            }
        }
        public static Task<SilenceEventArg> silence(this Card card, THHGame game)
        {
            return silence(new Card[] { card }, game);
        }
        public static bool isSilenced(this Card card, IGame game)
        {
            return card.getProp<bool>(game, PROPERTY.SILENCE);
        }

        public static bool hasDeathRattle(this Card card, IGame game)
        {
            if (card.define.effects.OfType<DeathRattle>().Count() > 0 && !card.getProp<bool>(game, PROPERTY.SILENCE))
            {
                return true;
            }
            else
            {
                foreach (var buff in card.getBuffs())
                {
                    if (buff.effects.OfType<DeathRattle>().Count() > 0)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        public static bool hasTag(this Card card, IGame game, string tag)
        {
            string[] tags = card.getProp<string[]>(game, nameof(ServantCardDefine.tags));
            if (tags == null)
                return false;
            return tags.Contains(tag);
        }
        public static bool hasTag(this CardDefine define, string tag)
        {
            string[] tags = define.getProp<string[]>(nameof(ServantCardDefine.tags));
            if (tags == null)
                return false;
            return tags.Contains(tag);
        }
        public static string getTag(this Card card, IGame game, int index)
        {
            return card.getTags(game)[index];
        }
        public static string[] getTags(this Card card, IGame game)
        {
            return card.getProp<string[]>(game, nameof(ServantCardDefine.tags));
        }
        public static Task<IPropChangeEventArg> setTags(this Card card, IGame game, string[] tags)
        {
            return card.setProp(game, nameof(ServantCardDefine.tags), tags);
        }

        [TranslateMsg]
        const string NOT_YOUR_TURN = "这不是你的回合";

        [TranslateMsg]
        const string NOT_YOUR_CARD = "你不能使用不属于你的卡牌";
        [TranslateMsg]
        const string ONLY_SKILL_OR_HAND = "你只能使用技能或手牌";
        [TranslateMsg]
        const string NOT_ENOUGH_SPIRIT = "你没有足够的灵力值";
        [TranslateMsg]
        const string SERVANT_FIELD_FULL = "你无法将更多的仆从置入战场";
        [TranslateMsg]
        const string NO_SPACE_FOR_TRAP = "你没有多余的位置挂载陷阱";
        [TranslateMsg]
        const string DUPLICATED_TRAP = "你已挂载了相同的陷阱";
        [TranslateMsg]
        const string SKILL_USED = "你已经使用过技能了";
        [TranslateMsg]
        const string SKILL_UNUSABLE = "技能不可用";
        [TranslateMsg]
        const string SKILL_INVALID = "技能无效";
        [TranslateMsg]
        const string UNKNOWN_CARD = "这是一张未知的卡牌";

        public static bool isUsable(this Card card, THHGame game, THHPlayer player, out string info)
        {
            if (game.currentPlayer != player)//不是你的回合
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
            if (card.define is ServantCardDefine servant)
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
            else if (card.define is ItemCardDefine item)
            {
                if (player.getGem() < card.getCost(game))//费用不够
                {
                    info = NOT_ENOUGH_SPIRIT;
                    return false;
                }
                info = null;
                return true;
            }
            else if (card.define is SpellCardDefine spell)
            {
                if (player.getGem() < card.getCost(game))
                {
                    info = NOT_ENOUGH_SPIRIT;
                    return false;
                }
                if (spell.hasKeyword(KEYWORD.SECRET))
                {
                    if (card.getOwner().getHead().count >= card.getOwner().getHead().maxCount)
                    {
                        game.logger.log(NO_SPACE_FOR_TRAP);
                        info = NO_SPACE_FOR_TRAP;
                        return false;
                    }
                    if (card.getOwner().getHead().Any(c => c.define == spell))
                    {
                        game.logger.log(DUPLICATED_TRAP);
                        info = DUPLICATED_TRAP;
                        return false;
                    }
                }
                info = null;
                return true;
            }
            else if (card.define is SkillCardDefine skill)
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
                if (skill.getEffectOn<THHPlayer.ActiveEventArg>(game.triggers) is ITriggerEffect effect)
                {
                    if (!effect.checkCondition(game, card, new object[]
                    {
                        new THHPlayer.ActiveEventArg(player,card,new object[0])
                    }))
                    {
                        info = SKILL_UNUSABLE;
                        return false;
                    }
                    info = null;
                    return true;
                }
                if (skill.getActiveEffect() is IActiveEffect activeEffect)
                {
                    if (!activeEffect.checkCondition(game, card, null))
                    {
                        info = SKILL_UNUSABLE;
                        return false;
                    }
                    info = null;
                    return true;
                }
                info = SKILL_INVALID;
                return false;
            }
            else
            {
                info = UNKNOWN_CARD;
                return false;//不知道是什么卡
            }
        }
        public static bool isNeedTarget(this Card card, THHGame game, out Card[] targets)
        {
            if (card.define.getActiveEffect() is ITargetEffect targetEffect)
            {
                List<Card> targetList = new List<Card>();
                foreach (THHPlayer player in game.players)
                {
                    if (card.isValidTarget(game, player.getMaster(), targetEffect, null))
                        targetList.Add(player.getMaster());
                    foreach (Card servant in player.getField())
                    {
                        if (card.isValidTarget(game, servant, targetEffect, null))
                            targetList.Add(servant);
                    }
                }
                targets = targetList.ToArray();
                return true;
            }
            if (card.define.getEffectOn<THHPlayer.ActiveEventArg>(game.triggers) is ITriggerEffect triggerEffect)
            {
                List<Card> targetList = new List<Card>();
                foreach (THHPlayer player in game.players)
                {
                    if (card.isValidTarget(game, player.getMaster(), null, triggerEffect))
                        targetList.Add(player.getMaster());
                    foreach (Card servant in player.getField())
                    {
                        if (card.isValidTarget(game, servant, null, triggerEffect))
                            targetList.Add(servant);
                    }
                }
                targets = targetList.ToArray();
                return true;
            }
            targets = null;
            return false;
        }
        /// <summary>
        /// 这张卡的主动效果的可选目标
        /// </summary>
        /// <param name="card"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        public static Card[] getAvaliableTargets(this Card card, THHGame game)
        {
            isNeedTarget(card, game, out var targets);
            return targets;
        }
        public static bool isValidTarget(this Card card, THHGame game, Card target)
        {
            ITargetEffect targetEffect = card.define.getActiveEffect() as ITargetEffect;
            ITriggerEffect triggerEffect = card.define.getEffectOn<THHPlayer.ActiveEventArg>(game.triggers);
            return isValidTarget(card, game, target, targetEffect, triggerEffect);
        }
        private static bool isValidTarget(this Card card, THHGame game, Card target, ITargetEffect targetEffect, ITriggerEffect triggerEffect)
        {
            if (target.isStealth(game) && target.getOwner() != card.getOwner())
                return false;
            if (targetEffect == null && triggerEffect == null)
                return false;
            if (targetEffect != null && !targetEffect.checkTargets(game, card, null, new object[] { target }))
                return false;
            if (triggerEffect != null && triggerEffect.checkTargets(game, card, null, new object[] { target }))
                return false;
            return true;
        }

        /// <summary>
        /// 重置卡牌
        /// </summary>
        /// <param name="card"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        public static async Task resetCard(this Card card, THHGame game)
        {
            await card.removeBuff(game, card.getBuffs());

            if (card.define is ServantCardDefine servant)
            {
                await card.setDead(game, false);
                await card.setCurrentLife(game, servant.life);
                await card.setCost(game, servant.cost);
                await card.setSpellDamage(game, servant.spellDamage);
                await card.setKeywords(game, servant.keywords);
                await card.setTags(game, servant.tags);
                await card.setReady(game, false);
                await card.setAttackTimes(game, 0);
                await card.setProp(game, PROPERTY.SILENCE, false);
            }
            else if (card.define is SpellCardDefine spell)
            {
                await card.setCost(game, spell.cost);
                foreach (var keyword in spell.keywords)
                {
                    if (string.IsNullOrEmpty(keyword))
                        continue;
                    await card.setProp(game, keyword, true);
                }
                await card.setProp(game, PROPERTY.KEYWORDS, spell.keywords);
                await card.setProp(game, nameof(SpellCardDefine.tags), spell.tags);
            }
            else if (card.define is SkillCardDefine skill)
            {
                await card.setCost(game, skill.cost);
            }
            else if (card.define is ItemCardDefine item)
            {
                await card.setCost(game, item.cost);
                await card.setAttack(game, item.attack);
                await card.setLife(game, item.life);
            }

        }
        public static async Task resetCard(this IEnumerable<Card> cards, THHGame game)
        {
            foreach (var card in cards)
            {
                await resetCard(card, game);
            }
        }

        public static Task activeEffect(this Card card, THHGame game, THHPlayer player, Card[] targets)
        {
            ITriggerEffect triggerEffect = card.define.getEffectOn<THHPlayer.ActiveEventArg>(game.triggers);
            if (triggerEffect != null)
            {
                return triggerEffect.execute(game, card, new object[] { new THHPlayer.ActiveEventArg(player, card, targets) }, targets);
            }
            IActiveEffect activeEffect = card.define.getActiveEffect();
            if (activeEffect != null)
            {
                return activeEffect.execute(game, card, new object[] { new THHPlayer.ActiveEventArg(player, card, targets) }, targets);
            }
            return Task.CompletedTask;
        }
        public static async Task triggerDeathRattles(this Card card, THHGame game)
        {
            //首先触发本体亡语
            foreach (var deathRattle in card.define.effects.OfType<DeathRattle>())
            {
                await deathRattle.execute(game, card, null, new DeathEventArg()
                {
                    infoDic = new Dictionary<Card, DeathEventArg.Info>()
                    {
                        { card, new DeathEventArg.Info(){ player = card.getOwner(), position = card.getOwner().getField().indexOf(card) } }
                    }
                });
            }
            //然后触发buff亡语
            foreach (var buff in card.getBuffs())
            {
                foreach (var deathRattle in buff.effects.OfType<DeathRattle>())
                {
                    await deathRattle.execute(game, card, buff, new DeathEventArg()
                    {
                        infoDic = new Dictionary<Card, DeathEventArg.Info>()
                        {
                            { card, new DeathEventArg.Info(){ player = card.getOwner(), position = card.getOwner().getField().indexOf(card) } }
                        }
                    });
                }
            }
        }
        public static async Task<AttackEventArg> tryAttack(this Card card, THHGame game, THHPlayer player, Card target)
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
        public static Task<AttackEventArg> forceAttack(this Card card, THHGame game, THHPlayer player, Card target)
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
        public static bool canAttackTest(this Card card, THHGame game)
        {
            if (!(card.pile.name == PILE.FIELD || card.pile.name == PILE.MASTER))//不在战场上
                return false;
            if (card.isDead(game))
                return false;
            return true;
        }
        static async Task<AttackEventArg> attack(this Card card, THHGame game, Card target)
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
                    await damage(game, new Dictionary<Card, DamageEventArg.Info>()
                    {
                        { card, new DamageEventArg.Info(target.getAttack(game),target) },
                        { target, new DamageEventArg.Info(card.getAttack(game),card) }
                    });
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
            await game.updateDeath();
            return eventArg;
        }
        public class AttackEventArg : EventArg, IDescribableEventArg
        {
            public Card card;
            public Card target;
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

            [MukioI18n.TranslateMsg("{0} {1}是卡")]
            const string TEMPLATE = "{0}攻击{1}";
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
        }
        public static Task<DamageEventArg> damage(this Card card, THHGame game, Card source, int value)
        {
            try
            {
                return damage(game, new Dictionary<Card, DamageEventArg.Info>()
                {
                    { card, new DamageEventArg.Info(value,source) }
                });
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static Task<DamageEventArg> damage(this IEnumerable<Card> cards, THHGame game, Card source, int value)
        {
            return damage(game, cards.ToDictionary(c => c, c => new DamageEventArg.Info(value, source)));
        }
        public static Task<DamageEventArg> damage(THHGame game, Dictionary<Card, DamageEventArg.Info> damageInfoDic)
        {
            return game.triggers.doEvent(new DamageEventArg() { infoDic = damageInfoDic }, async arg =>
            {
                damageInfoDic = arg.infoDic;
                List<Card> removeKeys = new List<Card>();
                foreach (Card card in damageInfoDic.Keys)
                {
                    if (card == null)
                        continue;
                    DamageEventArg.Info info = damageInfoDic[card];
                    Card source = info.source;
                    int value = info.damageValue;
                    if (source != null && source.isStealth(game))
                        source.setStealth(game, false);
                    if (value < 1)
                    {
                        removeKeys.Add(card);
                    }
                    else if (card.isImmunity(game))
                    {
                        game.logger.log(card + "免疫，不受伤害");
                        removeKeys.Add(card);
                    }
                    else if (card.isShield(game))
                    {
                        game.logger.log(card + "受到伤害，失去圣盾");
                        card.setShield(game, false);
                        removeKeys.Add(card);
                    }
                    else
                    {
                        int damageReduce = card.getDamageReduce(game);
                        int damage;
                        if (damageReduce > 0)
                        {
                            damage = value - damageReduce;
                            game.logger.log(card + "的伤害抵消" + damageReduce + "使伤害" + value + "=>" + damage);
                        }
                        else
                            damage = value;
                        if (damage > 0)
                        {
                            int armor = card.getArmor(game);
                            if (armor > 0)
                            {
                                if (armor >= damage)
                                {
                                    await card.loseArmor(game, damage);
                                    game.logger.log(card + "受到" + damage + "点伤害，护甲=>" + card.getArmor(game));
                                    await onDealKeywords(game, source, card, damage);
                                    info.damagedValue = damage;
                                    info.currentArmor = card.getArmor(game);
                                    info.currentLife = card.getCurrentLife(game);
                                }
                                else
                                {
                                    int leakDamage = damage - armor;
                                    await card.loseArmor(game, armor);
                                    await card.setCurrentLife(game, card.getCurrentLife(game) - leakDamage);
                                    game.logger.log(card + "受到" + damage + "点伤害，护甲=>" + card.getArmor(game) + "，生命值=>" + card.getCurrentLife(game));
                                    await onDealKeywords(game, source, card, damage);
                                    info.damagedValue = damage;
                                    info.currentArmor = card.getArmor(game);
                                    info.currentLife = card.getLife(game);
                                }
                            }
                            else
                            {
                                await card.setCurrentLife(game, card.getCurrentLife(game) - damage);
                                game.logger.log(card + "受到" + damage + "点伤害，生命值=>" + card.getCurrentLife(game));
                                await onDealKeywords(game, source, card, damage);
                                info.damagedValue = damage;
                                info.currentArmor = card.getArmor(game);
                                info.currentLife = card.getCurrentLife(game);
                            }
                        }
                        else
                        {
                            game.logger.log(card + "的伤害" + value + "被减伤" + damageReduce + "抵消");
                            removeKeys.Add(card);
                        }
                    }
                }
                foreach (var key in removeKeys)
                {
                    damageInfoDic.Remove(key);
                }
            });
            async Task onDealKeywords(THHGame g, Card s, Card c, int d)
            {
                if (s != null)
                {
                    if (s.isPoisonous(g) && !g.players.Any(p => p.getMaster() == c))
                        await c.setDead(game, true);
                    if (s.isDrain(g))
                        await s.getOwner().getMaster().heal(g, c, d);
                }
            }
        }
        public static async Task<DamageEventArg[]> damageByRandom(this IEnumerable<Card> cards, THHGame game, Card source, int value)
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
        public static Task<DamageEventArg> damageByRandomCount(this IEnumerable<Card> cards, THHGame game, Card source, int value, int count)
        {
            if (cards.Count() > count)
                cards = cards.randomTake(game, count);
            return cards.damage(game, source, value);

        }
        public class DamageEventArg : EventArg, IDescribableEventArg
        {
            public Dictionary<Card, Info> infoDic = new Dictionary<Card, Info>();
            ICard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return infoDic.Count > 0 ? infoDic.First().Value.source : null;
            }
            ICard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return infoDic.Keys.ToArray();
            }
            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                return string.Join("，", infoDic.Select(p => p.Value.source != null ?
                    p.Value.source.getFormatString() + "对" + p.Key.getFormatString() + "造成" + p.Value.damagedValue + "点伤害" :
                    p.Key.getFormatString() + "受到" + p.Value.damagedValue + "点伤害"));
            }

            [MukioI18n.TranslateMsg("{0}是卡，{1}是伤害值")]
            const string TEMPLATE_RECV = "{0}受到{1}点伤害";
            [MukioI18n.TranslateMsg("{0}{1}是卡，{2}是伤害值")]
            const string TEMPLATE_ATK = "{0}对{1}造成{2}点伤害";
            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                if (infoDic.Count == 1)
                {
                    return infoDic.First().Value.source != null ? TEMPLATE_ATK : TEMPLATE_RECV;
                }
                // todo: 暂时不支持列表
                IDescribableEventArg arg = this;
                return arg.toString(game, viewer);
            }
            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                if (infoDic.Count == 1)
                {
                    var kv = infoDic.First();
                    if (kv.Value.source != null)
                    {
                        return new object[] {
                            kv.Value.source.getFormatString(),
                            kv.Key.getFormatString(),
                            kv.Value.damagedValue
                        };
                    }
                    else
                    {
                        return new object[] {
                            kv.Key.getFormatString(),
                            kv.Value.damagedValue
                        };
                    }
                }
                return new object[0];
            }

            public class Info
            {
                public Card source;
                public int damageValue;
                public int damagedValue = 0;
                public int currentArmor = 0;
                public int currentLife = 0;
                public Info(int damage, Card source)
                {
                    damageValue = damage;
                    this.source = source;
                }
            }
        }

        public static bool isDamage(this Card card, IGame game)
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
        public static Task<HealEventArg> heal(this IEnumerable<Card> cards, THHGame game, Card sourse, int value)
        {
            cards = cards.Where(c => c.getCurrentLife(game) < c.getLife(game));
            if (cards.Count() < 1)
                return Task.FromResult<HealEventArg>(default);
            return game.triggers.doEvent(new HealEventArg() { cards = cards.ToArray(), source = sourse, value = value }, async arg =>
             {
                 cards = arg.cards;
                 sourse = arg.source;
                 value = arg.value;
                 foreach (Card card in cards)
                 {
                     if (card.getCurrentLife(game) + value < card.getLife(game))
                     {
                         await card.setCurrentLife(game, card.getCurrentLife(game) + value);
                         arg.infoDic.Add(card, new HealEventArg.Info()
                         {
                             healedValue = value
                         });
                         game.logger.log(card + "恢复" + value + "点生命值，生命值=>" + card.getCurrentLife(game));
                     }
                     else
                     {
                         int healedValue = card.getLife(game) - card.getCurrentLife(game);
                         await card.setCurrentLife(game, card.getLife(game));
                         arg.infoDic.Add(card, new HealEventArg.Info()
                         {
                             healedValue = healedValue
                         });
                         game.logger.log(card + "恢复" + healedValue + "点生命值，生命值=>" + card.getCurrentLife(game));
                     }
                 }
             });
        }
        public static Task<HealEventArg> heal(this Card card, THHGame game, Card sourse, int value)
        {
            return heal(new Card[] { card }, game, sourse, value);

        }
        public class HealEventArg : EventArg, IDescribableEventArg
        {
            public Card[] cards;
            public Card source;
            public int value;
            public Dictionary<Card, Info> infoDic = new Dictionary<Card, Info>();

            ICard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return infoDic.Count > 1 ? null : infoDic.First().Key;
            }

            ICard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return infoDic.Count > 1 ? infoDic.Keys.ToArray() : null;
            }

            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                return string.Join("，", infoDic.Select(p => "治疗" + p.Key.getFormatString() + p.Value.healedValue + "点生命值"));
            }
            [MukioI18n.TranslateMsg("{0}是卡，{1}是治疗量")]
            const string TEMPLATE = "治疗{0}{1}点生命值";
            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                if (infoDic.Count() == 1)
                {
                    return TEMPLATE;
                }
                // todo: 列表的实现
                return (this as IDescribableEventArg).toString(game, viewer);
            }
            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                if (infoDic.Count() == 1)
                {
                    var kv = infoDic.First();
                    return new object[] { kv.Key.getFormatString(), kv.Value.healedValue };
                }
                else
                    return new object[] { };
            }

            public class Info
            {
                public int healedValue;
            }
        }
        public static Task die(this Card card, THHGame game)
        {
            return die(new Card[] { card }, game);
        }
        public static async Task<DeathEventArg> die(this IEnumerable<Card> cards, THHGame game)
        {
            List<THHPlayer> remainPlayerList = new List<THHPlayer>(game.players);
            var eventArg = await game.triggers.doEvent(new DeathEventArg() { infoDic = cards.ToDictionary(c => c, c => default(DeathEventArg.Info)) }, onDie);
            async Task onDie(DeathEventArg arg)
            {
                Dictionary<Card, DeathEventArg.Info> infoDic = new Dictionary<Card, DeathEventArg.Info>();
                foreach (var card in arg.infoDic.Keys)
                {
                    if (card.pile.name == PILE.MASTER)//英雄阵亡
                    {
                        infoDic.Add(card, new DeathEventArg.Info()
                        {
                            player = card.getOwner(),
                            position = card.pile.indexOf(card)
                        });
                        remainPlayerList.Remove(card.getOwner());
                        game.logger.log(card.getOwner() + "失败");
                    }
                    else if (card.pile.name == PILE.FIELD)//随从阵亡
                    {
                        infoDic.Add(card, new DeathEventArg.Info()
                        {
                            player = card.getOwner(),
                            position = card.pile.indexOf(card)
                        });
                        await card.pile.moveTo(game, card, card.getOwner().getGrave());
                        game.logger.log(card + "阵亡");
                    }
                    else if (card.pile.name == PILE.ITEM)//装备摧毁
                    {
                        infoDic.Add(card, new DeathEventArg.Info()
                        {
                            player = card.getOwner(),
                            position = card.pile.indexOf(card)
                        });
                        await card.pile.moveTo(game, card, card.getOwner().getGrave());
                        game.logger.log(card + "摧毁");
                    }
                }
                arg.infoDic = infoDic;
            }
            foreach (var card in eventArg.infoDic.Keys)
            {
                await card.resetCard(game);
            }
            if (remainPlayerList.Count != game.players.Length)
            {
                if (remainPlayerList.Count > 0)
                    await game.gameEnd(remainPlayerList.ToArray());
                else
                    await game.gameEnd(new THHPlayer[0]);
            }
            return eventArg;
        }
        public class DeathEventArg : EventArg, IDescribableEventArg
        {
            public Dictionary<Card, Info> infoDic;

            ICard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return infoDic.Count > 1 ? null : infoDic.First().Key;
            }

            ICard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return infoDic.Count > 1 ? infoDic.Keys.ToArray() : null;
            }

            public class Info
            {
                public THHPlayer player;
                public int position;
            }
            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                IDescribableEventArg arg = this;
                return String.Format(arg.localizeTemplateString(game, viewer), arg.localizeStringArgs(game, viewer));
            }

            [MukioI18n.TranslateMsg("{0}是卡牌列表")]
            const string TEMPLATE = "{0}死亡";
            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                return TEMPLATE;
            }
            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                return new object[] {
                    string.Join("，", infoDic.Keys.Select(c => c.getFormatString()))
                };
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
        public static List<Card> getAttackableTargets(this Card card, THHGame game)
        {
            var targets = new List<Card>();
            if (card.pile != card.getOwner().getField() && !(card.define is MasterCardDefine))
            {
                return targets;
            }

            var oppo = game.getOpponent(card.getOwner());
            var allEnem = oppo.getField().ToList().Append(oppo.getMaster()).Where(c=>!c.isStealth(game)).ToList();
            if (allEnem.Any(c=>c.hasKeyword(game,KEYWORD.TAUNT)))
            {
                targets.AddRange(allEnem.Where(c => c.hasKeyword(game, KEYWORD.TAUNT)));
            }
            else
            {
                targets.AddRange(allEnem);
            }
            
            return targets;
        }

        public static Task fieldToHand(this Card card, THHGame game)
        {
            return fieldToHand(new Card[] { card }, game);
        }
        public static async Task fieldToHand(this IEnumerable<Card> cards, THHGame game)
        {
            foreach (IGrouping<THHPlayer, Card> g in cards.GroupBy(c => c.getOwner()))
            {
                int restCount = g.Key.getHand().maxCount - g.Key.getHand().count;
                if (restCount > 0)
                {
                    var toHand = g.Take(restCount);
                    game.logger.log("将" + string.Join("，", toHand) + "置入" + g.Key + "的手牌");
                    await cards.resetCard(game);
                    await g.Key.getField().moveTo(game, toHand, g.Key.getHand());
                }
                if (restCount < g.Count())
                {
                    var toGrave = g.Skip(restCount);
                    game.logger.log(g.Key + "的手牌已满，无法将" + string.Join(",", toGrave) + "置入手牌");
                    await toGrave.setDead(game, true);
                }
            }
            await game.updateDeath();
        }

        /// <summary>
        /// 场上回牌库
        /// </summary>
        /// <param name="card"></param>
        /// <param name="game"></param>
        /// <param name="isYourDeck"></param>
        /// <param name="random">若为 false, 将移动到牌堆顶</param>
        /// <returns></returns>
        public static Task fieldToDeck(this Card card,THHGame game, bool random)
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
        public static async Task fieldToDeck(this IEnumerable<Card> cards,THHGame game, bool random)
        {
            foreach (var g in cards.GroupBy(c=>c.getOwner()))
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
        public static Task graveToHand(this Card card, THHGame game)
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
        public static async Task graveToHand(this IEnumerable<Card> cards, THHGame game)
        {
            foreach (IGrouping<THHPlayer, Card> g in cards.GroupBy(c => c.getOwner()))
            {
                int restCount = g.Key.getHand().maxCount - g.Key.getHand().count;
                if (restCount > 0)
                {
                    var toHand = g.Take(restCount);
                    game.logger.log("将" + string.Join("，", toHand) + "置入" + g.Key + "的手牌");
                    await g.Key.getGrave().moveTo(game, toHand, g.Key.getHand());
                }
                if (restCount < g.Count())
                {
                    var toGrave = g.Skip(restCount);
                    game.logger.log(g.Key + "的手牌已满，无法将" + string.Join(",", toGrave) + "置入手牌");
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
        public static Task graveToDeck(this Card card, THHGame game, bool random)
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
        public static async Task graveToDeck(this IEnumerable<Card> cards, THHGame game, bool random)
        {
            foreach (IGrouping<THHPlayer, Card> g in cards.GroupBy(c => c.getOwner()))
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
                THHPlayer player = arg.card.getOwner();
                await player.getHead().moveTo(game, arg.card, player.getGrave());
            });
        }
        public class SecretTriggerEventArg : EventArg, IDescribableEventArg
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
        }

        /// <summary>
        /// 任务完成
        /// </summary>
        /// <param name="card"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        public static Task<MissionCompleteArg> MissionComplete(this Card card,IGame game)
        {
            return game.triggers.doEvent(new MissionCompleteArg() { card = card }, async arg =>
            {
                game.logger.log("任务完成 : " + card);
                THHPlayer player = arg.card.getOwner();
                await player.getHead().moveTo(game, arg.card, player.getGrave());
            });
        }
        public class MissionCompleteArg : EventArg, IDescribableEventArg
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
        }

        /// <summary>
        /// 变形
        /// </summary>
        /// <param name="card"></param>
        /// <param name="game"></param>
        /// <param name="define"></param>
        public static async Task<Card.SetDefineEventArg> setDefine(this Card card, IGame game, CardDefine define)
        {
            var eventArg = await card.setDefine(game, define);
            await card.resetCard(game as THHGame);//卡牌重置
            return eventArg;
        }
        /// <summary>
        /// 登场
        /// </summary>
        /// <param name="card">触发登场的牌</param>
        /// <param name="game"></param>
        /// <param name="targets">登场效果目标，可以为null</param>
        /// <returns></returns>
        public static async Task<ComeOnStageArg> ComeOnStage(this Card card, THHGame game, THHPlayer player, Card[] targets)
        {

            IActiveEffect activeEffect = card.define.getActiveEffect();
            if (activeEffect != null)
            {
                var eventArg = await game.triggers.doEvent(new ComeOnStageArg() { card = card, player = player, targets = targets }, async arg =>
                {
                     await card.activeEffect(game, player, targets);//执行随从效果
                     if (targets != null &&
                        targets.Length > 0)
                     {
                         game.logger.log("触发战吼" + card + ",目标是" + targets[0]);
                     }
                     else
                     {
                         game.logger.log("触发战吼" + card);
                     }
                });
                return eventArg;
            }
            return null;
        }
        public class ComeOnStageArg : EventArg, IDescribableEventArg
        {
            public Card card;
            public THHPlayer player;
            public Card[] targets;

            ICard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return card;
            }
            ICard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return targets;
            }
            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                IDescribableEventArg arg = this;
                return String.Format(arg.localizeTemplateString(game, viewer), arg.localizeStringArgs(game, viewer));
            }

            [MukioI18n.TranslateMsg("{0}是卡")]
            const string TEMPLATE_NO_TARGET = "{0}的宣言。";
            [MukioI18n.TranslateMsg("{0}是卡，{1}是目标")]
            const string TEMPLATE_HAS_TARGET = "{0}的宣言。目标：{1}";
            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                if (targets == null || targets.Length < 1)
                    return TEMPLATE_NO_TARGET;
                else
                    return TEMPLATE_HAS_TARGET;
            }
            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                return new object[] {
                    card.getFormatString(),
                    string.Join("，", targets?.Select(c => c.getFormatString()))
                };
            }
        }

    }
}