using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using System;
using MukioI18n;
using System.IO;

namespace TouhouHeartstone
{
    public static partial class THHCard
    {
        #region 公有方法

        #region 基础扩展
        public static Task<EventArg> setProp(this Card card, IGame game, string name, object value)
        {
            return SetCardPropEventDefine.doEvent(game as CardEngine, card, name, value);
        }
        public static Task<EventArg> addBuff(this Card card, IGame game, Buff buff)
        {
            return AddBuffEventDefine.doEvent(game as CardEngine, card, buff);
        }
        public static Task<EventArg> removeBuff(this Card card, IGame game, Buff buff)
        {
            return RemoveBuffEventDefine.doEvent(game as CardEngine, card, buff);
        }
        public static async Task<int> removeBuff(this Card card, IGame game, IEnumerable<Buff> buffs)
        {
            int count = 0;
            foreach (var buff in buffs)
            {
                var eventArg = await card.removeBuff(game, buff);
                if (eventArg != null && eventArg.getVar<bool>(RemoveBuffEventDefine.VAR_REMOVED))
                    count++;
            }
            return count;
        }
        #endregion

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
        public static Task<EventArg> setAttack(this Card card, IGame game, int value)
        {
            return card.setProp(game, DEFINE.ATTACK, value);
        }

        [Obsolete("Use buff or modifier instead")]
        public static Task<EventArg> setLife(this Card card, IGame game, int value)
        {
            return card.setProp(game, DEFINE.LIFE, value);
        }
        public static Task<EventArg> setCurrentLife(this Card card, IGame game, int value)
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
        public static Task<EventArg> setTags(this Card card, IGame game, string[] tags)
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
        public static Task<EventArg> setUsed(this Card card, IGame game, bool value)
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
            if (card.isUnplayable(game))
            {
                info = INFO_UNPLAYABLE;
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
                    if (!player.canAddTrap(card, out info))
                        return false;
                }
                if (card.hasKeyword(game, KEYWORD.MISSION))
                {
                    if (!player.canAddMission(card, out info))
                        return false;
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
            var eventArg = ActiveEventDefine.newEvent(game, card, new Card[] { target });
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
        public static Task<EventArg> setReady(this Card card, IGame game, bool value)
        {
            return card.setProp(game, CARD.ISREADY, value);
        }
        public static int getAttackTimes(this Card card, IGame game)
        {
            return card.getProp<int>(game, CARD.ATTACK_TIMES);
        }
        public static Task<EventArg> setAttackTimes(this Card card, IGame game, int value)
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

        public static async Task<EventArg> tryAttack(this Card card, CardEngine game, Player player, Card target)
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
        public static Task<EventArg> forceAttack(this Card card, CardEngine game, Player player, Card target)
        {
            if (!card.canForceAttack(game))
            {
                game.logger.logTrace($"{card}无法进行强制攻击");
                return Task.FromResult<EventArg>(null);
            }
            if (!card.isForceAttackable(game, player, target))
            {
                game.logger.logTrace($"{card}无法对{target}进行强制攻击");
                return Task.FromResult<EventArg>(null);
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
        static Task<EventArg> attack(this Card card, CardEngine game, Card target)
        {
            return AttackEventDefine.doEvent(game, card, target);
        }
        #endregion

        #region 伤害
        [Obsolete]
        public static async Task<EventArg[]> damageObsolete(this IEnumerable<Card> cards, CardEngine game, Card source, int value)
        {
            if (cards.Count() <= 0)
            {
                return default;
            }
            List<EventArg> args = new List<EventArg>();
            foreach (var target in cards.Where(p => p != null))
            {
                args.Add(await target.damage(game, source, value));
            }
            return args.ToArray();
        }
        [Obsolete]
        public static async Task<EventArg[]> damageByRandomObsolete(this IEnumerable<Card> cards, CardEngine game, Card source, int value)
        {
            List<EventArg> list = new List<EventArg>();
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
        public static Task<EventArg[]> damageByRandomCountObsolete(this IEnumerable<Card> cards, CardEngine game, Card source, int value, int count)
        {
            if (cards.Count() > count)
                cards = cards.randomTake(game, count);
            return cards.damageObsolete(game, source, value);
        }
        public static Task<EventArg> damage(this Card card, CardEngine game, Card source, int value)
        {
            if (card == null || value <= 0)
                return Task.FromResult<EventArg>(null);
            return DamageEventDefine.doEvent(game, card, source, value);
        }
        public static Task<EventArg> damage(this IEnumerable<Card> cards, CardEngine game, Card source, int value)
        {
            if (cards.Count() <= 0)
                return default;
            return MassDamageEventDefine.doEvent(game, source, cards.ToArray(), value);
        }
        public static Task<EventArg> damageItem(this Card item, CardEngine game, int value)
        {
            return item.damage(game, null, value);
        }
        public static Task<EventArg> damageByRandom(this IEnumerable<Card> cards, CardEngine game, Card source, int value)
        {
            if (cards.Count() <= 0)
                return default;
            return SplitDamageEventDefine.doEvent(game, source, cards.ToArray(), value);
        }
        public static Task<EventArg> damageByRandomCount(this IEnumerable<Card> cards, CardEngine game, Card source, int value, int count)
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
        public static async Task<EventArg[]> healObsolete(this IEnumerable<Card> cards, CardEngine game, Card sourse, int value)
        {
            List<EventArg> eventArgs = new List<EventArg>();
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
        public static Task<EventArg> heal(this IEnumerable<Card> cards, CardEngine game, Card source, int value)
        {
            return MassHealEventDefine.doEvent(game, source, cards.ToArray(), value);
        }
        public static Task<EventArg> heal(this Card card, CardEngine game, Card source, int value)
        {
            if (card == null || value <= 0)
                return null;
            return HealEventDefine.doEvent(game, card, source, value);

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
        public static Task<EventArg> kill(this Card card, IGame game)
        {
            return setDead(card, game, true);
        }
        /// <summary>
        /// 杀死多张卡牌。
        /// 实际作用为将其设置为已死亡。
        /// </summary>
        /// <param name="cards">卡牌列表。</param>
        /// <param name="game">游戏对象。</param>
        public static async Task<EventArg[]> kill(this IEnumerable<Card> cards, IGame game)
        {
            List<EventArg> eventArgs = new List<EventArg>();
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
        public static Task<EventArg> setDead(this Card card, IGame game, bool value)
        {
            return card.setProp(game, CARD.ISDEAD, value);
        }
        public static async Task<EventArg[]> setDead(this IEnumerable<Card> cards, CardEngine game, bool value)
        {
            List<EventArg> argList = new List<EventArg>();
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
        public static async Task<EventArg> destroy(this Card card, CardEngine game)
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
        public static async Task<EventArg[]> destroy(this IEnumerable<Card> cards, CardEngine game)
        {
            List<Player> remainPlayerList = new List<Player>(game.players);

            // 禁用卡牌不在墓地中触发的效果。
            // 用于解决“在一个友方仆从死亡后”，拥有诸如此类效果的仆从，如果和其他仆从同时死亡，这个也会触发的问题。
            foreach (Card card in cards)
            {
                // 卡牌定义自带的效果。
                foreach (var effect in card.define.getEffects().OfType<IPileRangedEffect>().Where(e => !e.getPiles().Contains(PILE.GRAVE) && !e.isDeathRattle()))
                {
                    await effect.onDisable(game, card, null);
                }
                // BUFF给予的效果。
                foreach (var buff in card.getBuffs())
                {
                    foreach (var effect in buff.getEffects().OfType<IPileRangedEffect>().Where(e => !e.getPiles().Contains(PILE.GRAVE) && !e.isDeathRattle()))
                    {
                        await effect.onDisable(game, card, buff);
                    }
                }
            }
            // 触发死亡事件。
            List<EventArg> eventArgs = new List<EventArg>();
            Dictionary<Card, (IEffect deathRattle, Buff buff)[]> deathrattles = new Dictionary<Card, (IEffect deathRattle, Buff buff)[]>();
            foreach (Card card in cards)
            {
                var eventArg = await DeathEventDefine.doEvent(game, card);
                var deadCard = eventArg.getVar<Card>(EVENTVAR.CARD);
                var deathrattleBuffs = eventArg.getVar<Buff[]>(DeathEventDefine.VAR_DEATHRATTLE_BUFFS);
                var deathrattleEffects = eventArg.getVar<IEffect[]>(DeathEventDefine.VAR_DEATHRATTLE_EFFECTS);

                var rattles = deathrattleBuffs.Select((b, i) => (deathrattleEffects[i], b)).ToArray();
                deathrattles.Add(deadCard, rattles);

                if (deadCard.pile.name == PILE.MASTER)//英雄阵亡
                {
                    remainPlayerList.Remove(deadCard.getOwner());
                    game.logger.logTrace($"{deadCard.getOwner()}失败");
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
        public static Task<EventArg> gainArmor(this Card card, CardEngine game, int value)
        {
            return setArmor(card, game, card.getArmor(game) + value);
        }
        public static Task<EventArg> loseArmor(this Card card, CardEngine game, int value)
        {
            return card.setArmor(game, card.getArmor(game) - value);
        }
        public static Task<EventArg> setArmor(this Card card, CardEngine game, int value)
        {
            int beforeValue = card.getArmor(game);
            if (value < 0)
                value = 0;
            if (beforeValue == value)
                return Task.FromResult<EventArg>(null);
            return SetArmorEventDefine.doEvent(game, card, value, beforeValue);
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
        public static async Task updateAuras(this Card card, CardEngine game)
        {
            var aura = card.getAuras(game);
            foreach (var (effect, buff) in aura)
            {
                if (effect is GeneratedAura gen)
                {
                    var effectEnv = new EffectEnv(game, card, card.define, buff, null, effect);
                    await gen.execute(effectEnv);
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
        public static Task<EventArg[]> activeEffect(this Card card, CardEngine game, Card[] targets)
        {
            if (card.isServant() || card.isItem() || card.isMaster())
            {
                return triggerBattlecries(card, game, targets);
            }
            else if (card.isSpell() || card.isSkill())
            {
                return triggerSpellActiveEffect(card, game, targets);
            }
            return Task.FromResult(Array.Empty<EventArg>());
        }
        /// <summary>
        /// 发动特定法术或技能卡牌定义的主动效果
        /// </summary>
        public static async Task<EventArg[]> triggerSpellCardDefineActiveEffect(this Card card, CardEngine game, CardDefine define, Card[] targets)
        {
            List<EventArg> eventArgs = new List<EventArg>();
            var effects = define.getSpellActiveEffects();
            foreach (var effect in effects)
            {
                var eventArg = await ActiveEventDefine.doEvent(game, card, targets, define, null, effect);
                eventArgs.Add(eventArg);
            }
            return eventArgs.ToArray();
        }
        public static async Task<EventArg[]> triggerSpellActiveEffect(this Card card, CardEngine game, Card[] targets)
        {
            List<EventArg> eventArgs = new List<EventArg>();
            var effects = card.getSpellActiveEffects(game);
            foreach (var (effect, buff) in effects)
            {
                var eventArg = await ActiveEventDefine.doEvent(game, card, targets, card.define, buff, effect);
                eventArgs.Add(eventArg);
            }
            return eventArgs.ToArray();
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
        public static async Task<EventArg[]> triggerCardDefineBattlecries(this Card card, CardEngine game, CardDefine define, Card[] targets)
        {
            List<EventArg> eventArgs = new List<EventArg>();
            var effects = define.getBattlecries();
            foreach (var effect in effects)
            {
                var eventArg = await ActiveEventDefine.doEvent(game, card, targets, define, null, effect);
                eventArgs.Add(eventArg);
            }
            return eventArgs.ToArray();
        }
        public static async Task<EventArg[]> triggerBattlecries(this Card card, CardEngine game, Card[] targets)
        {
            List<EventArg> eventArgs = new List<EventArg>();
            var effects = card.getBattlecries(game);
            foreach (var (effect, buff) in effects)
            {
                var eventArg = await ActiveEventDefine.doEvent(game, card, targets, card.define, buff, effect);
                eventArgs.Add(eventArg);
            }
            return eventArgs.ToArray();
        }
        /// <summary>
        /// 触发一个特定卡牌定义自带的遗言。
        /// </summary>
        /// <param name="card">触发遗言的仆从。</param>
        /// <param name="game">游戏对象。</param>
        /// <param name="define">要触发遗言的卡牌定义。</param>
        /// <returns></returns>
        public static async Task<EventArg[]> triggerCardDefineDeathRattles(this Card card, CardEngine game, CardDefine define)
        {
            List<EventArg> eventArgs = new List<EventArg>();
            var deathrattles = define.getDeathRattles();
            foreach (var effect in deathrattles)
            {
                var eventArg = await LastwordEventDefine.doEvent(game, card, define, null, effect);
                eventArgs.Add(eventArg);
            }
            return eventArgs.ToArray();
        }
        public static Task<EventArg[]> triggerDeathRattles(this Card card, CardEngine game)
        {
            var deathrattles = card.getDeathRattles(game);
            return card.triggerDeathRattles(game, deathrattles);
        }
        public static async Task<EventArg[]> triggerDeathRattles(this Card card, CardEngine game, (IEffect effect, Buff buff)[] deathrattles)
        {
            List<EventArg> eventArgs = new List<EventArg>();
            foreach (var (effect, buff) in deathrattles)
            {
                var eventArg = await LastwordEventDefine.doEvent(game, card, card.define, buff, effect);
                eventArgs.Add(eventArg);
            }
            return eventArgs.ToArray();
        }
        #endregion

        #endregion

        #region 移动卡牌

        #region 弃牌
        public static Task<EventArg> discard(this Card card, CardEngine game)
        {
            if (card == null)
                return Task.FromResult<EventArg>(null);
            return DiscardEventDefine.doEvent(game, card);
        }
        [Obsolete]
        public async static Task<EventArg[]> discardObsoleteMass(this IEnumerable<Card> cards, CardEngine game)
        {
            List<EventArg> eventArgs = new List<EventArg>();
            foreach (Card card in cards)
            {
                if (card == null)
                    continue;
                eventArgs.Add(await card.discard(game));
            }
            return eventArgs.ToArray();
        }
        public static Task<EventArg> discard(this IEnumerable<Card> cards, CardEngine game)
        {
            return MassDiscardEventDefine.doEvent(game, cards.ToArray());
        }
        #endregion

        #region 群体移动
        public static Task<EventArg> moveToField(this IEnumerable<Card> cards, CardEngine game, Player player)
        {
            return MassEnterFieldEventDefine.doEvent(game, cards.ToArray(), player);
        }
        public static Task<EventArg> moveToDeck(this IEnumerable<Card> cards, CardEngine game, Player player, bool top = false)
        {
            return MassShuffleToDeckEventDefine.doEvent(game, cards.ToArray(), player, top);
        }
        public static Task<EventArg> moveToGrave(this IEnumerable<Card> cards, CardEngine game, Player player)
        {
            var to = player.getGrave();
            return MassMoveCardEventDefine.doEvent(game, cards.ToArray(), to);
        }
        public static Task<EventArg> moveToHead(this IEnumerable<Card> cards, CardEngine game, Player player)
        {
            return MassMoveToHeadEventDefine.doEvent(game, cards.ToArray(), player);
        }
        public static Task<EventArg> moveToWarp(this IEnumerable<Card> cards, CardEngine game, Player player)
        {
            var to = player.getWarp();
            return MassMoveCardEventDefine.doEvent(game, cards.ToArray(), to);
        }
        /// <summary>
        /// 将多张卡牌返回到指定玩家的手牌。
        /// </summary>
        /// <param name="cards">卡牌。</param>
        /// <param name="game"></param>
        /// <param name="player">移动至手牌的玩家。</param>
        /// <returns></returns>
        public static Task<EventArg> returnToHand(this IEnumerable<Card> cards, CardEngine game, Player player)
        {
            if (cards == null || cards.Count() <= 0 || player == null)
                return null;
            return MassReturnHandEventDefine.doEvent(game, cards.ToArray(), player);
        }
        #endregion

        #region 单体移动
        public static Task<EventArg> moveToHead(this Card card, CardEngine game, Player player, int position = -1)
        {
            if (card == null || player == null)
                return Task.FromResult<EventArg>(null);
            return moveToHead(card, game, card.pile, player?.getHead());
        }
        /// <summary>
        /// 将一张卡牌返回到指定玩家的手牌。
        /// </summary>
        /// <param name="card">卡牌。</param>
        /// <param name="game"></param>
        /// <param name="player">移动至手牌的玩家。</param>
        /// <returns></returns>
        public static Task<EventArg> returnToHand(this Card card, CardEngine game, Player player)
        {
            if (card == null || player == null)
                return Task.FromResult<EventArg>(null);
            return moveToHand(card, game, card.pile, player?.getHand());
        }
        public static Task<EventArg> moveToField(this Card card, CardEngine game, Player player)
        {
            if (card == null || player == null)
                return Task.FromResult<EventArg>(null);
            return moveToField(card, game, card.pile, player?.getField());
        }
        public static Task<EventArg> moveToItem(this Card card, CardEngine game, Player player)
        {
            if (card == null || player == null)
                return Task.FromResult<EventArg>(null);
            return moveToItem(card, game, card.pile, player[PILE.ITEM]);
        }
        public static async Task<EventArg> moveToHead(this Card card, CardEngine game, Pile from, Pile to, int position = -1)
        {
            if (isInvalidMove(card, from, to))
                return null;
            if (!card.isTrap(game) && !card.hasKeyword(game, KEYWORD.MISSION))
            {
                game.logger.logTrace($"卡牌类型非法，无法将{string.Join(",", card)}置入延迟区");
                return null;
            }
            var player = to.owner;
            if (!player.canAddToHead(game, card, out _))
                return null;

            game.logger.logTrace($"将{string.Join("，", card)}置入{player}的延迟区");

            if (position < 0)
                position = to.count;
            return await from.moveTo(game, card, to, position);
        }
        /// <summary>
        /// 将一张卡牌返回到指定玩家的手牌。
        /// </summary>
        /// <param name="card">卡牌。</param>
        /// <param name="game"></param>
        /// <param name="player">移动至手牌的玩家。</param>
        /// <returns></returns>
        public static async Task<EventArg> moveToHand(this Card card, CardEngine game, Pile from, Pile to, int position = -1)
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
            if (position < 0)
                position = to.count;
            game.logger.logTrace($"将{card}置入{player}的手牌");
            return await from.moveTo(game, card, to, position);
        }
        public static async Task<EventArg> moveToField(this Card card, CardEngine game, Pile from, Pile to, int position = -1)
        {
            if (isInvalidMove(card, from, to))
                return null;
            if (position < 0)
                position = to.count;
            var enterFieldArg = await to.owner.tryEnterField(game, from, card, position);
            if (enterFieldArg == null)
                return null;
            return enterFieldArg.getChildEvents(EventState.Logic).OfType<EventArg>().FirstOrDefault();
        }
        public static async Task<EventArg> moveToItem(this Card card, CardEngine game, Pile from, Pile to)
        {
            if (isInvalidMove(card, from, to))
                return null;
            var equipArg = await to.owner.equip(game, card.pile, card);
            if (equipArg == null)
                return null;
            return equipArg.getChildEvents(EventState.Logic).OfType<EventArg>().FirstOrDefault();
        }
        public static async Task<EventArg> moveCardFromPileToPile(this Card card, CardEngine game, Pile fromPile, Pile toPile, int position = -1)
        {
            var toName = toPile?.name;
            switch (toName)
            {
                case PILE.HAND:
                    return await card.moveToHand(game, fromPile, toPile, position);
                case PILE.HEAD:
                    return await card.moveToHead(game, fromPile, toPile, position);
                case PILE.ITEM:
                    return await card.moveToItem(game, fromPile, toPile);
                case PILE.FIELD:
                    return await card.moveToField(game, fromPile, toPile, position);
            }
            return await fromPile.moveTo(game, card, toPile, position);
        }
        #endregion
        /// <summary>
        /// 获得一个仆从的控制权。
        /// </summary>
        /// <param name="card"></param>
        /// <param name="game"></param>
        /// <param name="player"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static async Task<EventArg> takeControl(this Card card, CardEngine game, Player player, int position = -1)
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
        #endregion

        #region 增益
        public static Task<EventArg> addBuffs(this Card[] cards, CardEngine game, BuffDefine buffDefine)
        {
            return MassAddBuffEventDefine.doEvent(game, cards, buffDefine);
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
        public static async Task updateSecret(this Card secret, CardEngine game)
        {
            if (secret.isSpell() && secret.isTrap(game)) // 是陷阱。
            {
                // 查找所有自定义事件效果。
                // 如果是陷阱效果，改变其启用或禁用。
                foreach (var effect in secret.define.getEffects().Where(e => e.isTrap()))
                {
                    if (effect is IPileRangedEffect rangedEffect && rangedEffect.getPiles().Contains(PILE.HEAD))
                    {
                        if (game.getCurrentPlayer() == secret.getOwner())
                            await effect.onDisable(game, secret, null);
                        else
                            await effect.onEnable(game, secret, null);
                    }
                }
            }
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
        public static async Task<EventArg> setDefine(this Card card, CardEngine game, CardDefine define, bool isreset = true)
        {
            var eventArg = await SetCardDefineEventDefine.doEvent(game, card, define);
            if (isreset) //卡牌重置
                await card.resetCard(game); 
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
            // 复制所有卡牌的属性可见性
            foreach (var invisiblePropName in card.getInvisibleProps())
            {
                var invisiblePlayers = card.getPropInvisiblePlayers(invisiblePropName);
                if (invisiblePlayers == null)
                    continue;
                foreach (var player in invisiblePlayers)
                {
                    clone.setPropInvisibleTo(game, invisiblePropName, player, true);
                }
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
            if (moveArg == null || !moveArg.getVar<bool>(MoveCardEventDefine.VAR_SUCCESS))
                return null;

            // 移动成功。
            // 该卡牌移动需要重置卡牌属性。
            if (moveArg.getVar<bool>(MoveCardEventDefine.VAR_RESET_PROPS) && THHPile.shouldCardMoveResetCardProp(from, targetPile))
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
                    await effect.onDisable(game, clone, buff as Buff);
                }
                var enabledEffects = card.getEnabledEffects();
                foreach (var (buff, effect) in enabledEffects)
                {
                    if (buff != null) // 该效果依附于增益上
                    {
                        // 寻找卡牌复制中拥有同样实例ID的增益。
                        var cloneBuff = clone.getBuffs().FirstOrDefault(b => b.instanceID == buff.instanceID);
                        if (cloneBuff != null) // 如果没有找到，说明该增益没有被复制，不启用。
                            await effect.onEnable(game, clone, buff as Buff);
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
        const string INFO_UNPLAYABLE = "该卡牌无法被使用";
        [TranslateMsg]
        const string NOT_ENOUGH_SPIRIT = "灵力值不足";
        [TranslateMsg]
        const string NO_TARGET = "没有目标";
        [TranslateMsg]
        const string SERVANT_FIELD_FULL = "位置已满";
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
    }
}