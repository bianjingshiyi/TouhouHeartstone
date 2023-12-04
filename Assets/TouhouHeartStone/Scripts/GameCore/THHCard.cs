using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using System;
using MukioI18n;
using PropChangeEventArg = TouhouCardEngine.Card.PropChangeEventArg;

namespace TouhouHeartstone
{
    public static partial class THHCard
    {
        #region 公有方法

        #region 获取类型
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
        #endregion

        #region 属性值
        public static int getCost(this ICardData card, IGame game)
        {
            int result = card.getProp<int>(game, DEFINE.COST);
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
        public static int getLife(this ICardData card, IGame game)
        {
            return card.getProp<int>(game, DEFINE.LIFE);
        }
        public static int getCurrentLife(this ICardData card, IGame game)
        {
            var prop = card.getProp(game, CARD.CURRENT_LIFE);
            if (prop is int life)
            {
                return life;
            }
            return card.getLife(game);
        }
        [Obsolete("Use buff or modifier instead")]
        public static Task<PropChangeEventArg> setAttack(this Card card, IGame game, int value)
        {
            return card.setProp(game, DEFINE.ATTACK, value);
        }

        [Obsolete("Use buff or modifier instead")]
        public static Task<PropChangeEventArg> setLife(this Card card, IGame game, int value)
        {
            return card.setProp(game, DEFINE.LIFE, value);
        }
        public static Task<PropChangeEventArg> setCurrentLife(this Card card, IGame game, int value)
        {
            var maxLife = card.getLife(game);
            return card.setProp(game, CARD.CURRENT_LIFE, Math.Min(maxLife, value));
        }
        #endregion

        #region 卡牌标签
        public static bool hasTag(this ICardData card, IGame game, string tag)
        {
            string[] tags = card.getTags(game);
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
            string[] tags = card.getProp<string[]>(game, DEFINE.TAGS);
            return tags ?? card.define.getTags();
        }
        public static Task<PropChangeEventArg> setTags(this Card card, IGame game, string[] tags)
        {
            return card.setProp(game, DEFINE.TAGS, tags);
        }
        #endregion

        #region 技能
        /// <summary>
        /// 技能是否已经使用过？
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public static bool isUsed(this ICardData card, IGame game)
        {
            return card.getProp<bool>(game, CARD.IS_USED);
        }
        /// <summary>
        /// 设置技能是否使用过。
        /// </summary>
        /// <param name="card"></param>
        /// <param name="value"></param>
        public static Task<PropChangeEventArg> setUsed(this Card card, IGame game, bool value)
        {
            return card.setProp(game, CARD.IS_USED, value);
        }
        public static void setSkillRef(this CardDefine cardDefine, DefineReference defRef)
        {
            cardDefine.setProp(DEFINE.CARD_PROP_SKILL_REF, defRef);
        }
        public static DefineReference getSkillRef(this CardDefine cardDefine)
        {
            if (cardDefine.hasProp(DEFINE.CARD_PROP_SKILL_REF))
            {
                return cardDefine.getProp<DefineReference>(DEFINE.CARD_PROP_SKILL_REF);
            }
            return new DefineReference(cardDefine.cardPoolId, cardDefine.getProp<int>(DEFINE.CARD_PROP_SKILL_ID));
        }
        #endregion

        #region 使用/选择目标
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
                if (card.isTrap(game))
                {
                    if (card.getOwner().getHead().count >= card.getOwner().getHead().maxCount)
                    {
                        info = NO_SPACE_FOR_TRAP;
                        return false;
                    }
                    if (card.getOwner().getHead().Any(c => c.define == card.define))
                    {
                        info = DUPLICATED_TRAP;
                        return false;
                    }
                }
                if (card.hasKeyword(game, KEYWORD.MISSION))
                {
                    if (card.getOwner().getHead().count >= card.getOwner().getHead().maxCount)
                    {
                        info = NO_SPACE_FOR_TRAP;
                        return false;
                    }
                    if (card.getOwner().getHead().Any(c => c.define.hasKeyword(KEYWORD.MISSION)))
                    {
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
            targetList.AddRange(card.define.getAvaliableTargets(game, card));
            return targetList.ToArray();
        }
        public static bool isValidTarget(this Card card, CardEngine game, Card target)
        {
            return isValidTarget(card, game, target, out _);
        }
        public static bool isValidTarget(this Card card, CardEngine game, Card target, out string invalidMsg)
        {
            invalidMsg = null;
            if (card.isSilenced(game))
                return false;
            GeneratedActiveEffect generatedEffect = card.define.getEffects().OfType<GeneratedActiveEffect>().FirstOrDefault();
            return isValidTarget(card, game, target, generatedEffect, out invalidMsg);
        }
        public static bool isValidTarget(this Card card, CardEngine game, Card target, GeneratedActiveEffect generatedEffect)
        {
            return isValidTarget(card, game, target, generatedEffect, out _);
        }
        public static bool isValidTarget(this Card card, CardEngine game, Card target, GeneratedActiveEffect generatedEffect, out string invalidMsg)
        {
            invalidMsg = null;
            //不能是自己
            if (card == target)
                return false;
            //目标不在战场或者英雄位
            if (target.pile?.name != PILE.FIELD && target.pile?.name != PILE.MASTER)
                return false;
            //目标不可作为效果目标
            if (target.isStealth(game) && target.getOwner() != card.getOwner())
                return false;
            //目标是魔免
            if (target.isElusive(game) && (card.isSpell() || card.isSkill()))
            {
                invalidMsg = INFO_TARGET_IS_ELUSIVE;
                return false;
            }
            var eventArg = new HearthStoneRule.ActiveEventArg(card.getOwner(), card, new Card[] { target });
            if (generatedEffect != null && !generatedEffect.checkTarget(game, target, card, null, eventArg, out invalidMsg))
                return false;
            return true;
        }
        #endregion

        #region 攻击
        public static bool isReady(this Card card, IGame game)
        {
            return card.getProp<bool>(game, CARD.ISREADY);
        }
        public static Task<PropChangeEventArg> setReady(this Card card, IGame game, bool value)
        {
            return card.setProp(game, CARD.ISREADY, value);
        }
        public static int getAttackTimes(this Card card, IGame game)
        {
            return card.getProp<int>(game, CARD.ATTACK_TIMES);
        }
        public static Task<PropChangeEventArg> setAttackTimes(this Card card, IGame game, int value)
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
            if (card == target)//是自己
                return false;
            if (!(target.pile.name == PILE.FIELD || target.pile.name == PILE.MASTER))//不在战场上
                return false;
            if (target.isDead(game))//已经死了
                return false;
            return true;
        }

        public static async Task<AttackEventArg> tryAttack(this Card card, CardEngine game, Player player, Card target)
        {
            if (!card.canAttack(game))
            {
                game.logger.logTrace($"{card}无法进行攻击");
                return null;
            }
            if (!card.isAttackable(game, player, target, out var reason))
            {
                game.logger.logTrace($"{card}无法攻击{target}，因为{reason}");
                return null;
            }
            await card.setAttackTimes(game, card.getAttackTimes(game) + 1);
            return await card.attack(game, target);
        }
        public static Task<AttackEventArg> forceAttack(this Card card, CardEngine game, Player player, Card target)
        {
            if (!card.canForceAttack(game))
            {
                game.logger.logTrace($"{card}无法进行强制攻击");
                return Task.FromResult<AttackEventArg>(null);
            }
            if (!card.isForceAttackable(game, player, target))
            {
                game.logger.logTrace($"{card}无法对{target}进行强制攻击");
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
                card = arg.card;
                target = arg.target;
                if (!card.canAttackTest(game))
                {
                    game.logger.logTrace($"{card}无法进行攻击");
                    arg.isCanceled = true;
                }
                else if (card == target)
                {
                    game.logger.log(card + "尝试攻击自己");
                    arg.isCanceled = true;
                }
                else
                {
                    game.logger.logTrace($"{card}攻击{target}");
                    int targetAttack = target.getAttack(game);
                    int cardAttack = card.getAttack(game);
                    await card.damage(game, target, targetAttack);
                    await target.damage(game, card, cardAttack);

                    // 取消隐匿
                    if (card.isStealth(game))
                    {
                        await card.setStealth(game, false);
                    }
                    // 消耗耐久
                    var owner = card.getOwner();
                    if (owner != null && owner.getMaster() == card)
                    {
                        var item = owner.getItem();
                        if (item != null)
                        {
                            await item.damageItem(game, 1);
                        }
                    }
                }
            });
            return eventArg;
        }
        #endregion

        #region 伤害
        [Obsolete]
        public static async Task<DamageEventArg[]> damageObsolete(this IEnumerable<Card> cards, CardEngine game, Card source, int value)
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
        [Obsolete]
        public static async Task<DamageEventArg[]> damageByRandomObsolete(this IEnumerable<Card> cards, CardEngine game, Card source, int value)
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
        [Obsolete]
        public static Task<DamageEventArg[]> damageByRandomCountObsolete(this IEnumerable<Card> cards, CardEngine game, Card source, int value, int count)
        {
            if (cards.Count() > count)
                cards = cards.randomTake(game, count);
            return cards.damageObsolete(game, source, value);
        }
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
                if (dmgValue < 1)
                {
                    arg.isCanceled = true;
                }
                else if (target.isImmunity(game))
                {
                    arg.isCanceled = true;
                    game.logger.logTrace($"{target}免疫，不受伤害");
                }
                else if (target.isShield(game))
                {
                    arg.isCanceled = true;
                    game.logger.logTrace($"{target}受到伤害，失去圣盾");
                    await target.setShield(game, false);
                }
                else
                {
                    int damageReduce = target.getDamageReduce(game);
                    int damage;
                    if (damageReduce > 0)
                    {
                        damage = dmgValue - damageReduce;
                        game.logger.logTrace($"{target}的伤害抵消{damageReduce}使伤害{dmgValue}=>{damage}");
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
                                game.logger.logTrace($"{target}受到{damage}点伤害，护甲=>{target.getArmor(game)}");
                                await onDealKeywords(game, src, target, damage);
                                arg.damagedValue = damage;
                            }
                            else
                            {
                                int leakDamage = damage - armor;
                                await target.loseArmor(game, armor);
                                await target.setCurrentLife(game, target.getCurrentLife(game) - leakDamage);
                                game.logger.logTrace($"{target}受到{damage}点伤害，护甲=>{target.getArmor(game)}，生命值=>{target.getCurrentLife(game)}");
                                await onDealKeywords(game, src, target, damage);
                                arg.damagedValue = damage;
                            }
                        }
                        else
                        {
                            await target.setCurrentLife(game, target.getCurrentLife(game) - damage);
                            game.logger.logTrace($"{target}受到{damage}点伤害，生命值=>{target.getCurrentLife(game)}");
                            await onDealKeywords(game, src, target, damage);
                            arg.damagedValue = damage;
                        }
                    }
                    else
                    {
                        arg.isCanceled = true;
                        game.logger.logTrace($"{target}的伤害{dmgValue}被减伤{damageReduce}抵消");
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
        public static Task<MassDamageEventArg> damage(this IEnumerable<Card> cards, CardEngine game, Card source, int value)
        {
            if (cards.Count() <= 0)
                return default;
            return game.triggers.doEvent(new MassDamageEventArg() { cards = cards.ToArray(), source = source, value = value }, async arg =>
            {
                foreach (var target in arg.cards)
                {
                    if (target == null)
                        continue;
                    await target.damage(game, arg.source, arg.value);
                }
            });
        }
        public static Task<DamageEventArg> damageItem(this Card item, CardEngine game, int value)
        {
            return item.damage(game, null, value);
        }
        public static Task<SplitDamageEventArg> damageByRandom(this IEnumerable<Card> cards, CardEngine game, Card source, int value)
        {
            if (cards.Count() <= 0)
                return default;
            return game.triggers.doEvent(new SplitDamageEventArg() { targets = cards.ToArray(), source = source, value = value }, async arg =>
            {
                var argCards = arg.targets;
                var argSource = arg.source;
                var argValue = arg.value;
                List<Card> hit = new List<Card>();
                for (int i = 0; i < argValue; i++)
                {
                    Card target = argCards.Where(c => !c.isDead(game)).random(game);
                    if (target == null)
                        continue;
                    var damageArg = await target.damage(game, argSource, 1);
                    hit.Add(damageArg.target);
                }
                arg.damagedCards = hit.ToArray();
            });
        }
        public static Task<MassDamageEventArg> damageByRandomCount(this IEnumerable<Card> cards, CardEngine game, Card source, int value, int count)
        {
            if (cards.Count() > count)
                cards = cards.randomTake(game, count);
            return cards.damage(game, source, value);

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
        #endregion

        #region 治疗
        [Obsolete]
        public static async Task<HealEventArg[]> healObsolete(this IEnumerable<Card> cards, CardEngine game, Card sourse, int value)
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
        public static Task<MassHealEventArg> heal(this IEnumerable<Card> cards, CardEngine game, Card source, int value)
        {
            return game.triggers.doEvent(new MassHealEventArg() { cards = cards.ToArray(), source = source, value = value }, async arg =>
            {
                var argCards = arg.cards;
                var argSource = arg.source;
                var argValue = arg.value;
                foreach (var card in argCards)
                {
                    await heal(card, game, argSource, argValue);
                }
            });
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
                    game.logger.logTrace($"{card}恢复{healedValue}点生命值，生命值=>{card.getCurrentLife(game)}");
                }
                else
                {
                    arg.healedValue = 0;
                    game.logger.logTrace($"{card}没有恢复生命值，生命值=>{card.getCurrentLife(game)}");
                }
            });

        }
        #endregion

        #region 死亡/消灭
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
        public static Task<PropChangeEventArg> kill(this Card card, IGame game)
        {
            return setDead(card, game, true);
        }
        /// <summary>
        /// 杀死多张卡牌。
        /// 实际作用为将其设置为已死亡。
        /// </summary>
        /// <param name="cards">卡牌列表。</param>
        /// <param name="game">游戏对象。</param>
        public static async Task<PropChangeEventArg[]> kill(this IEnumerable<Card> cards, IGame game)
        {
            List<Card.PropChangeEventArg> eventArgs = new List<Card.PropChangeEventArg>();
            foreach (var card in cards)
            {
                if (card == null)
                    continue;
                eventArgs.Add(await card.kill(game));
            }
            return eventArgs.ToArray();
        }
        /// <summary>
        /// 设置为true则随从会在下一次死亡结算中视作阵亡。
        /// </summary>
        /// <param name="card"></param>
        /// <param name="value"></param>
        public static Task<PropChangeEventArg> setDead(this Card card, IGame game, bool value)
        {
            return card.setProp(game, CARD.ISDEAD, value);
        }
        public static async Task<PropChangeEventArg[]> setDead(this IEnumerable<Card> cards, CardEngine game, bool value)
        {
            List<PropChangeEventArg> argList = new List<PropChangeEventArg>();
            foreach (var card in cards)
            {
                argList.Add(await card.setProp(game, CARD.ISDEAD, value));
            }
            return argList.ToArray();
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
                    foreach (var effect in buff.getEffects().OfType<IPileRangedEffect>().Where(e => !e.piles.Contains(PILE.GRAVE) && !e.isDeathRattle()))
                    {
                        await effect.onDisable(game, card, buff);
                    }
                }
            }
            // 触发死亡事件。
            List<DeathEventArg> eventArgs = new List<DeathEventArg>();
            Dictionary<Card, (IEffect deathRattle, Buff buff)[]> deathrattles = new Dictionary<Card, (IEffect deathRattle, Buff buff)[]>();
            foreach (Card card in cards)
            {
                var eventArg = await game.triggers.doEvent(new DeathEventArg(card), onDie);
                async Task onDie(DeathEventArg arg)
                {
                    Card death = arg.card;
                    deathrattles.Add(death, death.getDeathRattles(game));
                    if (death.pile.name == PILE.MASTER)//英雄阵亡
                    {
                        remainPlayerList.Remove(death.getOwner());
                        game.logger.logTrace($"{death.getOwner()}失败");
                    }
                    else if (death.pile.name == PILE.FIELD)//随从阵亡
                    {
                        await death.pile.moveTo(game, death, death.getOwner().getGrave());
                        game.logger.logTrace($"{death}阵亡");
                    }
                    else if (death.pile.name == PILE.ITEM)//装备摧毁
                    {
                        await death.pile.moveTo(game, death, death.getOwner().getGrave());
                        game.logger.logTrace($"{death}摧毁");
                    }
                }
                eventArgs.Add(eventArg);
            }

            // 触发遗言。
            try
            {
                foreach (var pair in deathrattles)
                {
                    var card = pair.Key;
                    await triggerDeathRattles(card, game, pair.Value);
                }
            }
            catch (Exception e)
            {
                game.logger?.logError("Effect", $"触发遗言时引发异常：{e}");
            }
            if (remainPlayerList.Count != game.players.Length)
            {
                if (remainPlayerList.Count > 0)
                    await game.gameEnd(remainPlayerList.ToArray()); //TODO: 把这个方法移动到HearthStoneRule之后记得把getRule删了
                else
                    await game.gameEnd(new Player[0]);//TODO: 把这个方法移动到HearthStoneRule之后记得把getRule删了
            }
            return eventArgs.ToArray();
        }
        #endregion

        #region 重置卡牌

        /// <summary>
        /// 重置“战场专用属性”。
        /// </summary>
        /// <param name="card"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        public static async Task resetBattlefieldProps(this Card card, CardEngine game)
        {
            await resetTemporaryFieldProps(card, game);
            await resetPersistentFieldProps(card, game);
        }
        /// <summary>
        /// 重置临时的“战场专用属性”。
        /// </summary>
        /// <param name="card"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        public static async Task resetTemporaryFieldProps(this Card card, CardEngine game)
        {
            await card.setDead(game, false);
            await card.setAttackTimes(game, 0);
            await card.setReady(game, false);
            await card.setUsed(game, false);
        }
        /// <summary>
        /// 重置持续存在的的“战场专用属性”。
        /// </summary>
        /// <param name="card"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        public static async Task resetPersistentFieldProps(this Card card, CardEngine game)
        {
            await card.setCurrentLife(game, card.getLife(game));
            await card.setProp(game, CARD.ARMOR, 0);
            await card.setProp(game, CARD.SILENCE, false);
            await card.clearConsumableKeywords(game);
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
            await card.removeAllBuffs(game);
            await card.setSpellDamage(game, define.getSpellDamage());
            await card.setDamageReduce(game, define.getDamageReduce());
            await card.setKeywords(game, define.getKeywords());
            await card.setTags(game, define.getTags());
            await card.setChargeLevel(game, 0);
            await card.resetBattlefieldProps(game);
            if (card.pile.isGeneralizedBattlefield())
            {
                await card.InitConsumableKeywords(game);
            }
        }
        public static async Task resetCard(this IEnumerable<Card> cards, CardEngine game)
        {
            foreach (var card in cards)
            {
                await resetCard(card, game);
            }
        }
        #endregion

        #region 护甲值
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
            if (value < 0)
                value = 0;
            if (beforeValue == value)
                return Task.FromResult<SetArmorEventArg>(null);
            return game.triggers.doEvent(new SetArmorEventArg() { card = card, beforeValue = beforeValue, value = value }, async arg =>
            {
                card = arg.card;
                value = arg.value;
                await card.setProp(game, CARD.ARMOR, value);
            });
        }
        #endregion

        #region 效果
        public static (IEffect effect, Buff buff)[] getSpellActiveEffects(this Card card, IGame game)
        {
            List<(IEffect effect, Buff buff)> results = new List<(IEffect effect, Buff buff)>();
            if (card.isSpell() || card.isSkill())
            {
                if (!card.isSilenced(game))
                {
                    results.AddRange(card.define.getSpellActiveEffects().Select<IEffect, (IEffect effect, Buff buff)>(e => (e, null)));
                }
                foreach (var buff in card.getBuffs())
                {
                    var effects = buff.getEffects();
                    results.AddRange(effects.Where(e => e.isSpellActiveEffect()).Select<IEffect, (IEffect effect, Buff buff)>(e => (e, buff)));
                }
            }
            return results.ToArray();
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
                var effects = buff.getEffects();
                results.AddRange(effects.Where(e =>
                {
                    if (e.isBattlecry())
                        return true;
                    return false;
                })
                    .Select<IEffect, (IEffect effect, Buff buff)>(e => (e, buff)));
            }
            return results.ToArray();
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
                var effects = buff.getEffects();
                results.AddRange(effects.Where(e => e.isDeathRattle())
                    .Select<IEffect, (IEffect effect, Buff buff)>(e => (e, buff)));
            }
            return results.ToArray();
        }
        public static (IEffect effect, Buff buff)[] getAuras(this Card card, IGame game)
        {
            List<(IEffect effect, Buff buff)> results = new List<(IEffect effect, Buff buff)>();
            results.AddRange(card.define.getAuras()
                .Select<IEffect, (IEffect effect, Buff buff)>(e => (e, null)));
            foreach (var buff in card.getBuffs())
            {
                var effects = buff.getEffects();
                results.AddRange(effects.Where(e => e.isAura())
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
                var effects = buff.getEffects();
                if (effects.Any(e => e.isBattlecry()))
                    return true;
            }
            return false;
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
                var effects = buff.getEffects();
                if (effects.Any(e => e.isDeathRattle()))
                {
                    return true;
                }
            }
            return false;
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

        #region 触发效果
        /// <summary>
        /// 发动一张卡的主动效果
        /// </summary>
        /// <param name="card"></param>
        /// <param name="game"></param>
        /// <param name="player"></param>
        /// <param name="targets"></param>
        /// <returns></returns>
        public static Task<HearthStoneRule.ActiveEventArg> activeEffect(this Card card, CardEngine game, Player player, Card[] targets)
        {
            if (card.isServant() || card.isItem() || card.isMaster())
            {
                return triggerBattlecries(card, game, player, targets);
            }
            else if (card.isSpell() || card.isSkill())
            {
                return triggerSpellActiveEffect(card, game, player, targets);
            }
            return Task.FromResult<HearthStoneRule.ActiveEventArg>(null);
        }
        /// <summary>
        /// 发动特定法术或技能卡牌定义的主动效果
        /// </summary>
        public static Task<HearthStoneRule.ActiveEventArg> triggerSpellCardDefineActiveEffect(this Card card, CardEngine game, Player player, CardDefine define, Card[] targets)
        {
            var eventArg = new HearthStoneRule.ActiveEventArg(player, card, targets);
            return game.triggers.doEvent(eventArg, async arg =>
            {
                var argCard = arg.card;
                var argTargets = arg.targets;
                var activeEffects = define.getSpellActiveEffects();
                foreach (var effect in activeEffects)
                {
                    if (effect is IActiveEffect active)
                    {
                        if (active.checkCondition(game, card, null, arg))
                            await active.execute(game, card, null, arg);
                    }
                }
            });
        }
        public static Task<HearthStoneRule.ActiveEventArg> triggerSpellActiveEffect(this Card card, CardEngine game, Player player, Card[] targets)
        {
            var eventArg = new HearthStoneRule.ActiveEventArg(player, card, targets);
            return game.triggers.doEvent(eventArg, async arg =>
            {
                var argCard = arg.card;
                var argTargets = arg.targets;
                var activeEffects = argCard.getSpellActiveEffects(game);
                foreach (var (effect, buff) in activeEffects)
                {
                    if (effect is IActiveEffect active)
                    {
                        if (active.checkCondition(game, argCard, null, arg))
                            await active.execute(game, argCard, buff, arg);
                    }
                }
            });
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
        public static Task<HearthStoneRule.ActiveEventArg> triggerCardDefineBattlecries(this Card card, CardEngine game, Player player, CardDefine define, Card[] targets)
        {
            var eventArg = new HearthStoneRule.ActiveEventArg(player, card, targets);
            return game.triggers.doEvent(eventArg, async arg =>
            {
                var argCard = arg.card;
                var argTargets = arg.targets;
                var battlecries = define.getBattlecries();
                foreach (var battlecry in battlecries)
                {
                    if (battlecry is IActiveEffect active)
                    {
                        if (active.checkCondition(game, argCard, null, arg))
                            await active.execute(game, argCard, null, arg);
                    }
                }
            });
        }
        public static Task<HearthStoneRule.ActiveEventArg> triggerBattlecries(this Card card, CardEngine game, Player player, Card[] targets)
        {
            var eventArg = new HearthStoneRule.ActiveEventArg(player, card, targets);
            return game.triggers.doEvent(eventArg, async arg =>
            {
                var argCard = arg.card;
                var argTargets = arg.targets;
                var battlecries = getBattlecries(argCard, game);
                foreach (var (effect, buff) in battlecries)
                {
                    if (effect is IActiveEffect active)
                    {
                        if (active.checkCondition(game, argCard, buff, arg))
                            await active.execute(game, argCard, buff, arg);
                    }
                }
            });
        }
        /// <summary>
        /// 触发一个特定卡牌定义自带的遗言。
        /// </summary>
        /// <param name="card">触发遗言的仆从。</param>
        /// <param name="game">游戏对象。</param>
        /// <param name="define">要触发遗言的卡牌定义。</param>
        /// <returns></returns>
        public static Task<HearthStoneRule.LastwordEventArg> triggerCardDefineDeathRattles(this Card card, CardEngine game, CardDefine define)
        {
            var eventArg = new HearthStoneRule.LastwordEventArg(card.getOwner(), card);
            return game.triggers.doEvent(eventArg, async arg =>
            {
                var argCard = arg.card;
                var deathRattles = define.getDeathRattles();
                foreach (var deathRattle in deathRattles)
                {
                    if (deathRattle is GeneratedPassiveEffect genPassive)
                    {
                        if (genPassive.checkCondition(game, argCard, null, arg))
                            await genPassive.execute(game, argCard, null, arg);
                    }
                }
            });
        }
        public static Task<HearthStoneRule.LastwordEventArg> triggerDeathRattles(this Card card, CardEngine game)
        {
            var eventArg = new HearthStoneRule.LastwordEventArg(card.getOwner(), card);
            return game.triggers.doEvent(eventArg, arg =>
            {
                var argCard = arg.card;
                var deathrattles = getDeathRattles(argCard, game);
                return argCard.executeDeathRattles(game, arg, deathrattles);
            });
        }
        public static Task<HearthStoneRule.LastwordEventArg> triggerDeathRattles(this Card card, CardEngine game, (IEffect effect, Buff buff)[] deathrattles)
        {
            var eventArg = new HearthStoneRule.LastwordEventArg(card.getOwner(), card);
            return game.triggers.doEvent(eventArg, arg =>
            {
                var argCard = arg.card;
                return argCard.executeDeathRattles(game, arg, deathrattles);
            });
        }
        #endregion

        #endregion

        #region 移动卡牌

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
        [Obsolete]
        public async static Task<DiscardEventArg[]> discardObsoleteMass(this IEnumerable<Card> cards, CardEngine game)
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
        public static Task<MassDiscardEventArg> discard(this IEnumerable<Card> cards, CardEngine game)
        {
            return game.triggers.doEvent(new MassDiscardEventArg() { cards = cards.ToArray() }, async arg =>
            {
                foreach (Card card in arg.cards)
                {
                    if (card == null)
                        continue;
                    await card.discard(game);
                }
            });
        }
        #endregion

        #region 群体移动
        public static Task<MassEnterFieldEventArg> moveToField(this IEnumerable<Card> cards, CardEngine game, Player player)
        {
            return game.triggers.doEvent(new MassEnterFieldEventArg() { cards = cards.ToArray(), player = player }, async arg =>
            {
                var argCards = arg.cards;
                var argPlayer = arg.player;
                List<Card> movedCards = new List<Card>();
                foreach (var card in argCards)
                {
                    var moveArg = await card.moveToField(game, argPlayer);
                    if (moveArg != null && moveArg.success)
                        movedCards.Add(moveArg.card);
                }
                arg.movedCards = movedCards.ToArray();
            });
        }
        public static Task<MassShuffleToDeckEventArg> moveToDeck(this IEnumerable<Card> cards, CardEngine game, Player player, bool top = false)
        {
            return game.triggers.doEvent(new MassShuffleToDeckEventArg() { cards = cards.ToArray(), player = player, top = top }, async arg =>
            {
                var argCards = arg.cards;
                var argPlayer = arg.player;
                var argTop = arg.top;
                List<Card> movedCards = new List<Card>();
                foreach (var card in argCards)
                {
                    if (card == null)
                        continue;
                    int position = argTop ? argPlayer.getDeck().count : game.randomInt(0, argPlayer.getDeck().count);
                    game.logger.logTrace($"将{string.Join("，", card)}洗入{argPlayer}的牌库");
                    var moveArg = await card.pile.moveTo(game, card, player.getDeck(), position);
                    if (moveArg != null && moveArg.success)
                        movedCards.Add(card);
                }
                arg.movedCards = movedCards.ToArray();
            });
        }
        public static Task<MassMoveCardEventArg> moveToGrave(this IEnumerable<Card> cards, CardEngine game, Player player)
        {
            var to = player.getGrave();
            return game.triggers.doEvent(new MassMoveCardEventArg() { cards = cards.ToArray(), to = to }, async arg =>
            {
                var argCards = arg.cards;
                var argTo = arg.to;
                List<Card> movedCards = new List<Card>();
                foreach (var card in argCards)
                {
                    if (card == null)
                        continue;
                    game.logger.logTrace($"将{string.Join("，", card)}置入墓地{argTo}");
                    await card.pile.moveTo(game, card, argTo);
                    movedCards.Add(card);
                }
                arg.movedCards = movedCards.ToArray();
            });
        }
        public static Task<MassMoveToHeadEventArg> moveToHead(this IEnumerable<Card> cards, CardEngine game, Player player)
        {
            return game.triggers.doEvent(new MassMoveToHeadEventArg() { cards = cards.ToArray(), player = player }, async arg =>
            {
                var argCards = arg.cards;
                var argPlayer = arg.player;
                List<Card> movedCards = new List<Card>();
                foreach (var card in argCards)
                {
                    var moveArg = await card.moveToHead(game, argPlayer);
                    if (moveArg != null && moveArg.success)
                        movedCards.Add(moveArg.card);
                }
                arg.movedCards = movedCards.ToArray();
            });
        }
        public static Task<MassMoveCardEventArg> moveToWarp(this IEnumerable<Card> cards, CardEngine game, Player player)
        {
            var to = player.getWarp();
            return game.triggers.doEvent(new MassMoveCardEventArg() { cards = cards.ToArray(), to = to }, async arg =>
            {
                var argCards = arg.cards;
                var argTo = arg.to;
                List<Card> movedCards = new List<Card>();
                foreach (var card in argCards)
                {
                    if (card == null)
                        continue;
                    game.logger.logTrace($"将{string.Join("，", card)}置入暂存区{argTo}");
                    await card.pile.moveTo(game, card, argTo);
                    movedCards.Add(card);
                }
                arg.movedCards = movedCards.ToArray();
            });
        }
        /// <summary>
        /// 将多张卡牌返回到指定玩家的手牌。
        /// </summary>
        /// <param name="cards">卡牌。</param>
        /// <param name="game"></param>
        /// <param name="player">移动至手牌的玩家。</param>
        /// <returns></returns>
        public static Task<MassReturnHandEventArg> returnToHand(this IEnumerable<Card> cards, CardEngine game, Player player)
        {
            if (cards == null || cards.Count() <= 0 || player == null)
                return null;

            return game.triggers.doEvent(new MassReturnHandEventArg() { cards = cards.ToArray(), player = player }, async arg =>
            {
                var argCards = arg.cards;
                var argPlayer = arg.player;
                var target = argPlayer.getHand();
                var movedCards = new List<Card>();
                foreach (var card in argCards)
                {
                    var moveArg = await card.returnToHand(game, argPlayer);
                    if (moveArg != null && moveArg.success && moveArg.to == target)
                        movedCards.Add(moveArg.card);
                }
                arg.movedCards = movedCards.ToArray();
            });
        }
        #endregion

        #region 单体移动
        public static Task<THHPile.MoveCardEventArg> moveToHead(this Card card, CardEngine game, Player player)
        {
            return moveToHead(card, game, card.pile, player?.getHead());
        }
        /// <summary>
        /// 将一张卡牌返回到指定玩家的手牌。
        /// </summary>
        /// <param name="card">卡牌。</param>
        /// <param name="game"></param>
        /// <param name="player">移动至手牌的玩家。</param>
        /// <returns></returns>
        public static Task<THHPile.MoveCardEventArg> returnToHand(this Card card, CardEngine game, Player player)
        {
            return moveToHand(card, game, card.pile, player?.getHand());
        }
        public static Task<THHPile.MoveCardEventArg> moveToField(this Card card, CardEngine game, Player player)
        {
            return moveToField(card, game, card.pile, player?.getField());
        }
        public static Task<THHPile.MoveCardEventArg> moveToItem(this Card card, CardEngine game, Player player)
        {
            return moveToItem(card, game, card.pile, player[PILE.ITEM]);
        }
        public static async Task<THHPile.MoveCardEventArg> moveToHead(this Card card, CardEngine game, Pile from, Pile to)
        {
            if (isInvalidMove(card, from, to))
                return null;
            if (!card.isTrap(game) && !card.hasKeyword(game, KEYWORD.MISSION))
            {
                game.logger.logTrace($"卡牌类型非法，无法将{string.Join(",", card)}置入延迟区");
                return null;
            }
            var player = to.owner;
            if (to.isFull)
            {
                game.logger.logTrace($"{player}的延迟区已满，无法将{string.Join(",", card)}置入延迟区");
                return null;
            }
            if (to.Any(s => s.define == card.define))
            {
                game.logger.logTrace($"{player}的延迟区已包含{card.define}，无法将{string.Join(",", card)}置入延迟区");
                return null;
            }

            game.logger.logTrace($"将{string.Join("，", card)}置入{player}的延迟区");
            return await from.moveTo(game, card, to);
        }
        /// <summary>
        /// 将一张卡牌返回到指定玩家的手牌。
        /// </summary>
        /// <param name="card">卡牌。</param>
        /// <param name="game"></param>
        /// <param name="player">移动至手牌的玩家。</param>
        /// <returns></returns>
        public static async Task<THHPile.MoveCardEventArg> moveToHand(this Card card, CardEngine game, Pile from, Pile to)
        {
            if (isInvalidMove(card, from, to))
                return null;
            var player = to?.owner;
            if (to.isFull) // 手牌已满
            {
                game.logger.logTrace($"{player}的手牌已满，无法将{card}置入手牌");
                if (from?.name == PILE.FIELD) // 仆从直接将其消灭
                {
                    await kill(card, game);
                }
                else //否则移入墓地。
                {
                    var grave = player.getGrave();
                    if (from != grave)
                    {
                        return await from.moveTo(game, card, grave, grave.count);
                    }
                }
                return null;
            }
            // 手牌未满
            game.logger.logTrace($"将{card}置入{player}的手牌");
            return await from.moveTo(game, card, to, to.count);
        }
        public static async Task<THHPile.MoveCardEventArg> moveToField(this Card card, CardEngine game, Pile from, Pile to)
        {
            if (isInvalidMove(card, from, to))
                return null;
            var enterFieldArg = await to.owner.tryEnterField(game, from, card, to.count);
            if (enterFieldArg == null)
                return null;
            return enterFieldArg.getChildEvents(EventState.Logic).OfType<THHPile.MoveCardEventArg>().FirstOrDefault();
        }
        public static async Task<THHPile.MoveCardEventArg> moveToItem(this Card card, CardEngine game, Pile from, Pile to)
        {
            if (isInvalidMove(card, from, to))
                return null;
            var equipArg = await to.owner.equip(game, card.pile, card);
            if (equipArg == null)
                return null;
            return equipArg.getChildEvents(EventState.Logic).OfType<THHPile.MoveCardEventArg>().FirstOrDefault();
        }
        public static async Task<THHPile.MoveCardEventArg> moveCardFromPileToPile(this Card card, CardEngine game, Pile fromPile, Pile toPile)
        {
            var toName = toPile?.name;
            switch (toName)
            {
                case PILE.HAND:
                    return await card.moveToHand(game, fromPile, toPile);
                case PILE.HEAD:
                    return await card.moveToHead(game, fromPile, toPile);
                case PILE.ITEM:
                    return await card.moveToItem(game, fromPile, toPile);
                case PILE.FIELD:
                    return await card.moveToField(game, fromPile, toPile);
            }
            return await fromPile.moveTo(game, card, toPile);
        }
        #endregion
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
        public static async Task<THHPile.MoveCardEventArg> takeControl(this Card card, CardEngine game, Player player, int position = -1)
        {
            if (card.pile?.name != PILE.FIELD) // 不在场上
                return null;
            if (card.pile == player.getField())// 目标和源相同
                return null;
            if (player.getField().isFull)//没位置了
            {
                await card.kill(game);
                return null;
            }

            if (position < 0)
                position = player.getField().count;
            var eventArg = await card.pile.moveTo(game, card, player.getField(), position);
            await card.setReady(game, false);
            return eventArg;
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
                await g.resetCard(game);
                foreach (var card in g)
                {
                    int position = random ? game.randomInt(0, g.Key.getDeck().count) : g.Key.getDeck().count;
                    await g.Key.getField().moveTo(game, card, g.Key.getDeck(), position);
                }
            }

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
                    await g.Key.getGrave().moveTo(game, g.Take(g.Count()), g.Key.getDeck(), position);
                }
            }
        }
        #endregion

        #region 增益
        public static Task<MassAddBuffEventArg> addBuffs(this Card[] cards, CardEngine game, BuffDefine buffDefine)
        {
            return game.triggers.doEvent(new MassAddBuffEventArg() { cards = cards, buffDefine = buffDefine }, async arg =>
            {
                var argCards = arg.cards;
                var argBuffDefine = arg.buffDefine;
                List<Buff> buffList = new List<Buff>();
                foreach (var card in argCards)
                {
                    if (card == null)
                        continue;
                    Buff buff = new GeneratedBuff(argBuffDefine);
                    buffList.Add(buff);
                    await card.addBuff(game, buff);
                }
                arg.buffs = buffList.ToArray();
            });
        }
        /// <summary>
        /// 移除所有增益（不包括固有增益）
        /// </summary>
        /// <param name="card"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        public static Task<int> removeAllBuffs(this Card card, IGame game)
        {
            return card.removeBuff(game, card.getBuffs().Where(b => b is not IInnateBuff));
        }
        #endregion

        #region 延迟区
        public static bool isTrap(this ICardData card, IGame game)
        {
            return card.define.getGeneratedEffects().Any(e => e.isTrap());
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
                game.logger.logTrace($"触发陷阱{card}");
                Player player = arg.card.getOwner();
                await player.getHead().moveTo(game, arg.card, player.getGrave());
            });
        }
        public static async Task updateSecret(this Card secret, CardEngine game)
        {
            if (secret.isSpell() && secret.isTrap(game)) // 是陷阱。
            {
                // 查找所有自定义事件效果。
                // 如果是陷阱效果，改变其启用或禁用。
                foreach (var effect in secret.define.getEffects().Where(e => e.isTrap()))
                {
                    if (effect is IPileRangedEffect rangedEffect && rangedEffect.piles.Contains(PILE.HEAD))
                    {
                        if (game.getCurrentPlayer() == secret.getOwner())
                            await effect.onDisable(game, secret, null);
                        else
                            await effect.onEnable(game, secret, null);
                    }
                }
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
                game.logger.logTrace($"任务完成 : {card}");
                Player player = arg.card.getOwner();
                await player.getHead().moveTo(game, arg.card, player.getGrave());
            });
        }
        #endregion

        #region 杂项
        public static Player getOwner(this ICardData card)
        {
            return card.owner;
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
        /// <summary>
        /// 将一张卡牌复制到指定牌堆。
        /// </summary>
        /// <param name="card">要复制的卡牌。</param>
        /// <param name="game">游戏对象。</param>
        /// <param name="targetPile">目标牌堆。</param>
        /// <returns></returns>
        public static async Task<Card> cloneCardTo(this Card card, CardEngine game, Pile targetPile)
        {
            // 根据卡牌的定义创建一张新的卡牌。
            var clone = game.createCard(card.define);

            // 复制所有可复制的增益。
            foreach (var buff in card.getBuffs())
            {
                if (!buff.canClone)
                    continue;
                var buffClone = buff.clone();
                await clone.addBuff(game, buffClone);
            }
            // 复制所有卡牌的属性
            foreach (var pair in card.getAllProps(game, true))
            {
                await clone.setProp(game, pair.Key, pair.Value);
            }

            // 不移动到任何牌堆，直接返回复制。
            if (targetPile == null)
                return clone;

            // 源牌堆假定为要复制的卡牌的牌堆，用于处理战场→手牌，战场→牌库这样的逆向移动。
            Pile from = card.pile;

            // 将卡牌移动到目标牌堆。
            // 从null移动到目标牌堆，不会清除卡牌的增益或者属性，但是会清除所有战场属性。
            var moveArg = await moveCardFromPileToPile(clone, game, null, targetPile);

            // 移动不成功，复制失败。
            if (moveArg == null || !moveArg.success)
                return null;

            // 移动成功。
            // 该卡牌移动需要重置卡牌属性。
            if (moveArg.resetProps && THHPile.shouldCardMoveResetCardProp(from, targetPile))
            {
                await clone.resetCard(game);
            }
            else // 该移动不会重置卡牌，比如同牌堆移动，或者向更低优先级的牌堆移动。
            {
                // 复制卡牌的永久战场属性，因为移动会清除它们。包括封印、护甲、当前生命值等状态。
                foreach (var propName in persistentBattlefieldPropNames)
                {
                    var propValue = card.getProp(game, propName, true);
                    await clone.setProp(game, propName, propValue);
                }
                // 复制卡牌所有效果的启用状态，用于复制被封印的仆从。
                foreach (var (buff, effect) in clone.getEnabledEffects())
                {
                    await effect.onDisable(game, clone, buff);
                }
                var enabledEffects = card.getEnabledEffects();
                foreach (var (buff, effect) in enabledEffects)
                {
                    if (buff != null) // 该效果依附于增益上
                    {
                        // 寻找卡牌复制中拥有同样实例ID的增益。
                        var cloneBuff = clone.getBuffs().FirstOrDefault(b => b.instanceID == buff.instanceID);
                        if (cloneBuff != null) // 如果没有找到，说明该增益没有被复制，不启用。
                            await effect.onEnable(game, clone, buff);
                    }
                    else
                    {
                        // 效果是卡牌定义上的，复制。
                        await effect.onEnable(game, clone, null);
                    }
                }
                // 清除卡牌的临时战场属性，包括已就绪，攻击次数，濒死，以及技能已使用的状态。
                await clone.resetTemporaryFieldProps(game);
            }
            return clone;
        }
        #endregion

        #endregion

        #region 私有方法
        private static async Task executeDeathRattles(this Card argCard, CardEngine game, EventArg arg, (IEffect effect, Buff buff)[] deathrattles)
        {
            foreach (var (effect, buff) in deathrattles)
            {
                if (effect is GeneratedPassiveEffect genPassive)
                {
                    if (genPassive.checkCondition(game, argCard, buff, arg))
                        await genPassive.execute(game, argCard, buff, arg);
                }
            }
        }
        private static bool isInvalidMove(Card card, Pile from, Pile to)
        {
            if (card == null || to == null)
                return true;
            if (from != null && !from.Contains(card)) // 源牌堆不包含该卡牌。
                return true;
            return false;
        }
        #endregion

        #region 属性字段
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
        [TranslateMsg("目标选择的错误信息")]
        const string INFO_TARGET_IS_ELUSIVE = "目标具有魔免";
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
        public static string[] persistentPropNames = new string[]
        {
            DEFINE.ATTACK,
            DEFINE.LIFE,
            DEFINE.TAGS,
            DEFINE.SPELL_DAMAGE,
            DEFINE.DAMAGE_REDUCE,
            CARD.CHARGED_LEVEL,
        };
        public static string[] persistentBattlefieldPropNames = new string[]
        {
            DEFINE.KEYWORDS,
            CARD.CURRENT_LIFE,
            CARD.ARMOR,
            CARD.SILENCE,
        };
        public static string[] temporaryBattlefieldPropNames = new string[]
        {
            CARD.IS_USED,
            CARD.ISREADY,
            CARD.ATTACK_TIMES,
            CARD.ISDEAD,
        };
        #endregion

        #region 内嵌类
        [EventChildren(typeof(PropChangeEventArg))]
        public class DamageEventArg : EventArg, IDescribableEventArg, ICardEventArg
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
            DescribableCard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return source;
            }
            DescribableCard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return new DescribableCard[] { target };
            }
            ICard ICardEventArg.getCard()
            {
                return target;
            }
            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                return source != null ?
                    $"{source.getFormatString()}对{target.getFormatString()}造成{damagedValue}点伤害" :
                    $"{target.getFormatString()}受到{damagedValue}点伤害";
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
        [EventChildren(typeof(PropChangeEventArg))]
        public class SetArmorEventArg : EventArg, IDescribableEventArg, ICardEventArg
        {
            DescribableCard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return card;
            }
            DescribableCard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return null;
            }
            ICard ICardEventArg.getCard() => card;

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
        [EventChildren(typeof(PropChangeEventArg))]
        public class BeChargedEventArg : EventArg
        {
            public Card card
            {
                get => getVar<Card>(EVENTVAR.CARD);
                set => setVar(EVENTVAR.CARD, value);
            }
            public int beforeValue
            {
                get => getVar<int>(VAR_BEFORE_VALUE);
                set => setVar(VAR_BEFORE_VALUE, value);
            }
            public int value
            {
                get => getVar<int>(EVENTVAR.VALUE);
                set => setVar(EVENTVAR.VALUE, value);
            }
            public override void Record(IGame game, EventRecord record)
            {
                record.setCardState(EVENTVAR.CARD, card);
                record.setVar(VAR_BEFORE_VALUE, beforeValue);
                record.setVar(EVENTVAR.VALUE, value);
            }
            public const string VAR_BEFORE_VALUE = "原数值";
        }
        [EventChildren(typeof(PropChangeEventArg))]
        public class SilenceEventArg : EventArg, IDescribableEventArg, ICardEventArg
        {
            public override void Record(IGame game, EventRecord record)
            {
                record.setCardState(EVENTVAR.CARD, card);
            }
            DescribableCard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return card;
            }

            DescribableCard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return null;
            }
            ICard ICardEventArg.getCard()
            {
                return card;
            }

            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                IDescribableEventArg arg = this;
                return string.Format(arg.localizeTemplateString(game, viewer), arg.localizeStringArgs(game, viewer));
            }

            [MukioI18n.TranslateMsg("{0}是卡牌列表")]
            const string TEMPLATE = "{0} 被封印";
            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                return TEMPLATE;
            }
            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                return new object[] { string.Join("，", card.getFormatString()) };
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
            public Card card
            {
                get { return getVar<Card>(EVENTVAR.CARD); }
                set { setVar(EVENTVAR.CARD, value); }
            }
        }
        [EventChildren(typeof(DamageEventArg))]
        public class AttackEventArg : EventArg, IDescribableEventArg, ICardEventArg
        {
            DescribableCard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return card;
            }
            DescribableCard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return new DescribableCard[] { target };
            }
            ICard ICardEventArg.getCard()
            {
                return card;
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
        [EventChildren(typeof(THHPile.MoveCardEventArg))]
        public class DiscardEventArg : EventArg, IDescribableEventArg, ICardEventArg
        {
            DescribableCard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return null;
            }

            DescribableCard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return new DescribableCard[] { card };
            }
            ICard ICardEventArg.getCard() => card;

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
        [EventChildren(typeof(PropChangeEventArg))]
        public class HealEventArg : EventArg, IDescribableEventArg, ICardEventArg
        {

            DescribableCard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return card;
            }

            DescribableCard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return null;
            }
            ICard ICardEventArg.getCard()
            {
                return card;
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
        [EventChildren(typeof(THHPile.MoveCardEventArg))]
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
            DescribableCard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return card;
            }

            DescribableCard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return new DescribableCard[] { card };
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
        [EventChildren(typeof(THHPile.MoveCardEventArg))]
        public class SecretTriggerEventArg : EventArg, IDescribableEventArg
        {
            DescribableCard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return card;
            }
            DescribableCard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
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
        [EventChildren(typeof(THHPile.MoveCardEventArg))]
        public class MissionCompleteEventArg : EventArg, IDescribableEventArg
        {
            public Card card
            {
                get => getVar<Card>(EVENTVAR.CARD);
                set => setVar(EVENTVAR.CARD, value);
            }
            DescribableCard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return card;
            }
            DescribableCard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
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
        [EventChildren(typeof(PropChangeEventArg))]
        public class ShackleEventArg : EventArg, IDescribableEventArg, ICardEventArg
        {
            public override void Record(IGame game, EventRecord record)
            {
                record.setCardState(EVENTVAR.CARD, card);
            }
            DescribableCard IDescribableEventArg.getCard(IGame game, IPlayer viewer)
            {
                return card;
            }

            DescribableCard[] IDescribableEventArg.getTargets(IGame game, IPlayer viewer)
            {
                return null;
            }
            ICard ICardEventArg.getCard()
            {
                return card;
            }

            string IDescribableEventArg.toString(IGame game, IPlayer viewer)
            {
                IDescribableEventArg arg = this;
                return string.Format(arg.localizeTemplateString(game, viewer), arg.localizeStringArgs(game, viewer));
            }

            [MukioI18n.TranslateMsg("{0}是卡牌列表")]
            const string TEMPLATE = "{0} 被禁锢";
            string IDescribableEventArg.localizeTemplateString(IGame game, IPlayer viewer)
            {
                return TEMPLATE;
            }
            object[] IDescribableEventArg.localizeStringArgs(IGame game, IPlayer viewer)
            {
                return new object[] { string.Join("，", card.getFormatString()) };
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
            public Card card
            {
                get { return getVar<Card>(EVENTVAR.CARD); }
                set { setVar(EVENTVAR.CARD, value); }
            }
        }
        #endregion
    }
}