using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
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
        public static void setCost(this Card card, int value)
        {
            card.setProp(nameof(ServantCardDefine.cost), value);
        }
        public static int getCost(this CardDefine card)
        {
            return card.getProp<int>(nameof(ServantCardDefine.cost));
        }
        public static int getAttack(this Card card, IGame game)
        {
            THHGame tgame = (THHGame)game;
            int result = card.getProp<int>(game, nameof(ServantCardDefine.attack));
            if (card.getOwner() == tgame.currentPlayer)
                if (card.define is MasterCardDefine)
                {
                    if (card.getOwner().item != null)
                        result += card.getOwner().item.getAttack(game);
                }
            if (result < 0)
                return 0;
            return result;
        }
        public static int getAttack(this CardDefine card)
        {
            int result = card.getProp<int>(nameof(ServantCardDefine.attack));
            if (result < 0)
                return 0;
            return result;

        }
        public static void setAttack(this Card card, int value)
        {
            card.setProp(nameof(ServantCardDefine.attack), value);
        }
        public static int getLife(this Card card, IGame game)
        {
            return card.getProp<int>(game, nameof(ServantCardDefine.life));
        }
        public static int getLife(this CardDefine card)
        {
            return card.getProp<int>(nameof(ServantCardDefine.life));
        }
        public static void setLife(this Card card, int value)
        {
            card.setProp(nameof(ServantCardDefine.life), value);
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
        public static void setDead(this Card card, bool value)
        {
            card.setProp(nameof(THHCard.isDead), value);
        }
        public static void setDead(this IEnumerable<Card> cards, bool value)
        {
            foreach (var card in cards)
            {
                card.setProp(nameof(THHCard.isDead), value);
            }
        }
        public static int getArmor(this Card card, IGame game)
        {
            return card.getProp<int>(game, Property.ARMOR);
        }
        public static Task<SetArmorEventArg> gainArmor(this Card card, THHGame game, int value)
        {
            return setArmor(card, game, card.getArmor(game) + value);
        }
        public static Task<SetArmorEventArg> loseArmor(this Card card, THHGame game, int value)
        {
            return card.setArmor(game, card.getArmor(game) - value);
        }
        public static async Task<SetArmorEventArg> setArmor(this Card card, THHGame game, int value)
        {
            SetArmorEventArg eventArg = new SetArmorEventArg() { card = card, value = value };
            await game.triggers.doEvent(eventArg, arg =>
            {
                card = arg.card;
                value = arg.value;
                if (value < 0)
                    value = 0;
                card.setProp(Property.ARMOR, value);
                game.logger.log(card + "的护甲值变为" + value);
                return Task.CompletedTask;
            });
            return eventArg;
        }
        public class SetArmorEventArg : EventArg
        {
            public Card card;
            public int value;
        }
        public static int getCurrentLife(this Card card, IGame game)
        {
            return card.getProp<int>(game, "currentLife");
        }
        public static void setCurrentLife(this Card card, int value)
        {
            card.setProp("currentLife", value);
        }
        public static bool isReady(this Card card, IGame game)
        {
            return card.getProp<bool>(game, "isReady");
        }
        public static void setReady(this Card card, bool value)
        {
            card.setProp("isReady", value);
        }
        public static int getAttackTimes(this Card card, IGame game)
        {
            return card.getProp<int>(game, "attackTimes");
        }
        public static void setAttackTimes(this Card card, int value)
        {
            card.setProp("attackTimes", value);
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
            if (card.hasKeyword(game, Keyword.CANTATTACK))
                return false;
            if (card.getAttack(game) <= 0)//没有攻击力
                return false;
            if (!card.isReady(game)//还没准备好
                && !card.isCharge(game)//且没有冲锋
                && !(card.isRush(game) && game.getOpponent(card.getOwner()).field.Any(c => card.isAttackable(game, card.getOwner(), c, out _)))//且并非有突袭且有可以攻击的敌方随从
                )
                return false;
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
            if (!(card.pile.name == PileName.FIELD || card.pile.name == PileName.MASTER))//不在战场上
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
        public static bool isAttackable(this Card card, THHGame game, THHPlayer player, Card target, out string tip)
        {
            if (target == player.master || player.field.Contains(target))
            {
                tip = "你不能攻击友方角色";
                return false;
            }
            if (game.getOpponent(player).field.Any(c => c.isTaunt(game)) && !target.isTaunt(game))
            {
                tip = "你必须先攻击具有嘲讽的随从";
                return false;
            }
            if (card.isRush(game) && !card.isReady(game) && game.players.Any(p => p.master == target) && !card.isCharge(game))
            {
                tip = "具有突袭的随从在没有准备好的情况下不能攻击敌方英雄";//除非你具有冲锋
                return false;
            }
            if (target.isStealth(game))
            {
                tip = "无法攻击潜行的目标";
                return false;
            }
            if (!card.isForceAttackable(game, player, target))
            {
                tip = "无法攻击目标";
                return false;
            }
            tip = null;
            return true;
        }
        public static bool isForceAttackable(this Card card, THHGame game, THHPlayer player, Card target)
        {
            if (!(target.pile.name == PileName.FIELD || target.pile.name == PileName.MASTER))//不在战场上
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
        public static void setUsed(this Card card, bool value)
        {
            card.setProp("isUsed", value);
        }
        public static bool isTaunt(this Card card, IGame game)
        {
            return card.hasKeyword(game, Keyword.TAUNT);
        }
        public static bool isCharge(this Card card, IGame game)
        {
            return card.hasKeyword(game, Keyword.CHARGE);
        }
        public static bool isRush(this Card card, IGame game)
        {
            return card.hasKeyword(game, Keyword.RUSH);
        }
        public static void setRush(this Card card, IGame game, bool value)
        {
            if (value)
                card.addKeyword(game, Keyword.RUSH);
            else
                card.removeKeyword(game, Keyword.RUSH);
        }
        public static bool isShield(this Card card, IGame game)
        {
            return card.hasKeyword(game, Keyword.SHIELD);
        }
        public static void setShield(this Card card, IGame game, bool value)
        {
            if (value)
                card.addKeyword(game, Keyword.SHIELD);
            else
                card.removeKeyword(game, Keyword.SHIELD);
        }
        public static bool isStealth(this Card card, IGame game)
        {
            return card.hasKeyword(game, Keyword.STEALTH);
        }
        public static void setStealth(this Card card, IGame game, bool value)
        {
            if (value)
                card.addKeyword(game, Keyword.STEALTH);
            else
                card.removeKeyword(game, Keyword.STEALTH);
        }
        public static bool isDrain(this Card card, IGame game)
        {
            return card.hasKeyword(game, Keyword.DRAIN);
        }
        public static void setDrain(this Card card, IGame game, bool value)
        {
            if (value)
                card.addKeyword(game, Keyword.DRAIN);
            else
                card.removeKeyword(game, Keyword.DRAIN);
        }
        public static bool isPoisonous(this Card card, IGame game)
        {
            return card.hasKeyword(game, Keyword.POISONOUS);
        }
        public static void setPoisonous(this Card card, IGame game, bool value)
        {
            if (value)
                card.addKeyword(game, Keyword.POISONOUS);
            else
                card.removeKeyword(game, Keyword.POISONOUS);
        }
        public static bool isElusive(this Card card, IGame game)
        {
            return card.hasKeyword(game, Keyword.ELUSIVE);
        }
        public static void setElusive(this Card card, IGame game, bool value)
        {
            if (value)
                card.addKeyword(game, Keyword.ELUSIVE);
            else
                card.removeKeyword(game, Keyword.ELUSIVE);
        }
        public static bool isFreeze(this Card card, IGame game)
        {
            return card.hasKeyword(game, Keyword.FREEZE);
        }
        public static void setFreeze(this Card card, IGame game, bool value)
        {
            if (value)
                card.addKeyword(game, Keyword.FREEZE);
            else
                card.removeKeyword(game, Keyword.FREEZE);
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
            return card.hasKeyword(game, Keyword.WINDFURY);
        }
        public static void setWindfury(this Card card, IGame game, bool value)
        {
            if (value)
                card.addKeyword(game, Keyword.WINDFURY);
            else
                card.removeKeyword(game, Keyword.WINDFURY);
        }
        public static bool isImmunity(this Card card, IGame game)
        {
            return card.hasKeyword(game, Keyword.IMMUNITY);
        }
        public static void setImmunity(this Card card, IGame game, bool value)
        {
            if (value)
                card.addKeyword(game, Keyword.IMMUNITY);
            else
                card.removeKeyword(game, Keyword.IMMUNITY);
        }
        public static bool isUnique(this Card card, IGame game)
        {
            return card.hasKeyword(game, Keyword.UNIQUE);
        }
        public static void setUnique(this Card card, IGame game, bool value)
        {
            if (value)
                card.addKeyword(game, Keyword.UNIQUE);
            else
                card.removeKeyword(game, Keyword.UNIQUE);
        }
        public static void setKeywords(this Card card, IGame game, string[] keywords)
        {
            if (keywords == null)
                keywords = new string[0];
            foreach (var keyword in card.getKeywords(game))
            {
                card.setProp(keyword, false);
            }
            card.setProp(nameof(ServantCardDefine.keywords), keywords);
            foreach (var keyword in card.getKeywords(game))
            {
                card.setProp(keyword, true);
            }
        }
        public static string[] getKeywords(this Card card, IGame game)
        {
            return card.getProp<string[]>(game, nameof(ServantCardDefine.keywords));
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
            string[] keywords = define.getProp<string[]>(nameof(ServantCardDefine.keywords));
            if (keywords == null)
                return false;
            return keywords.Contains(keyword);
        }
        public static Task addKeyword(this Card card, IGame game, string keyword)
        {
            string[] keywords = card.getKeywords(game);
            if (!keywords.Contains(keyword))
                keywords = keywords.Append(keyword).ToArray();
            card.setKeywords(game, keywords);
            return Task.CompletedTask;
        }
        public static Task<bool> removeKeyword(this Card card, IGame game, string keyword)
        {
            List<string> keywordList = new List<string>(card.getKeywords(game));
            if (keywordList.Contains(keyword))
            {
                keywordList.Remove(keyword);
                card.setKeywords(game, keywordList.ToArray());
                return Task.FromResult(true);
            }
            else
                return Task.FromResult(false);
        }
        public static int getSpellDamage(this Card card, IGame game)
        {
            return card.getProp<int>(game, nameof(ServantCardDefine.spellDamage));
        }
        public static void setSpellDamage(this Card card, int value)
        {
            card.setProp(nameof(ServantCardDefine.spellDamage), value);
        }
        public static int getDamageReduce(this Card card, IGame game)
        {
            int result = card.getProp<int>(game, nameof(ServantCardDefine.damageReduce));
            if (result < 0)
                return 0;
            return result;
        }
        public static void setDamageReduce(this Card card, int value)
        {
            card.setProp(nameof(ServantCardDefine.damageReduce), value);
        }
        public static async Task<SilenceEventArg> silence(this IEnumerable<Card> cards, THHGame game)
        {
            SilenceEventArg eventArg = new SilenceEventArg() { cards = cards.ToArray() };
            await game.triggers.doEvent(eventArg, async arg =>
            {
                cards = arg.cards;
                foreach (var card in cards)
                {
                    card.setProp(nameof(silence), true);
                    await card.removeBuff(game, card.getBuffs());
                    foreach (var effect in card.define.effects.OfType<IPassiveEffect>())
                    {
                        effect.onDisable(game, card, null);
                    }
                    card.setKeywords(game, new string[0]);
                }
            });
            return eventArg;
        }
        public class SilenceEventArg : EventArg
        {
            public Card[] cards;
        }
        public static Task<SilenceEventArg> silence(this Card card, THHGame game)
        {
            return silence(new Card[] { card }, game);
        }
        public static bool isSilenced(this Card card, IGame game)
        {
            return card.getProp<bool>(game, nameof(silence));
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
        public static void setTags(this Card card, string[] tags)
        {
            card.setProp(nameof(ServantCardDefine.tags), tags);
        }
        public static bool isUsable(this Card card, THHGame game, THHPlayer player, out string info)
        {
            if (game.currentPlayer != player)//不是你的回合
            {
                info = "这不是你的回合";
                return false;
            }
            if (card.getOwner() != player)
            {
                info = "你不能使用不属于你的卡牌";
                return false;
            }
            if (card.define is ServantCardDefine servant)
            {
                if (player.gem < card.getCost(game))//费用不够
                {
                    info = "你没有足够的法力值";
                    return false;
                }
                if (player.field.count >= player.field.maxCount)
                {
                    info = "你无法将更多的随从置入战场";
                    return false;
                }
                info = null;
                return true;
            }
            if (card.define is ItemCardDefine item)
            {
                if (player.gem < card.getCost(game))//费用不够
                {
                    info = "你没有足够的法力值";
                    return false;
                }
                info = null;
                return true;
            }
            else if (card.define is SpellCardDefine spell)
            {
                if (player.gem < card.getCost(game))
                {
                    info = "你没有足够的法力值";
                    return false;
                }
                info = null;
                return true;
            }
            else if (card.define is SkillCardDefine skill)
            {
                if (card.isUsed(game))//已经用过了
                {
                    info = "你已经使用过技能了";
                    return false;
                }
                if (player.gem < card.getCost(game))//费用不够
                {
                    info = "你没有足够的法力值";
                    return false;
                }
                if (skill.getEffectOn<THHPlayer.ActiveEventArg>(game.triggers) is ITriggerEffect effect)
                {
                    if (!effect.checkCondition(game, card, new object[]
                    {
                        new THHPlayer.ActiveEventArg(player,card,new object[0])
                    }))
                    {
                        info = "技能不可用";
                        return false;
                    }
                    info = null;
                    return true;
                }
                if (skill.getActiveEffect() is IActiveEffect activeEffect)
                {
                    if (!activeEffect.checkCondition(game, card, null))
                    {
                        info = "技能不可用";
                        return false;
                    }
                    info = null;
                    return true;
                }
                info = "技能无效";
                return false;
            }
            else
            {
                info = "这是一张未知的卡牌";
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
                    if (targetEffect.checkTargets(game, null, card, new object[] { player.master }))
                        targetList.Add(player.master);
                    foreach (Card servant in player.field)
                    {
                        if (targetEffect.checkTargets(game, null, card, new object[] { servant }))
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
                    if (triggerEffect.checkTargets(game, null, card, new object[] { player.master }))
                        targetList.Add(player.master);
                    foreach (Card servant in player.field)
                    {
                        if (triggerEffect.checkTargets(game, null, card, new object[] { servant }))
                            targetList.Add(servant);
                    }
                }
                targets = targetList.ToArray();
                return true;
            }
            targets = null;
            return false;
        }
        public static Card[] getAvaliableTargets(this Card card, THHGame game)
        {
            isNeedTarget(card, game, out var targets);
            return targets;
        }
        public static bool isValidTarget(this Card card, THHGame game, Card target)
        {
            if (target.isStealth(game))
                return false;
            ITargetEffect targetEffect = card.define.getActiveEffect() as ITargetEffect;
            ITriggerEffect triggerEffect = card.define.getEffectOn<THHPlayer.ActiveEventArg>(game.triggers);
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
            if (card.define is ServantCardDefine servant)
            {
                card.setDead(false);
                card.setCurrentLife(servant.life);
                card.setReady(false);
                card.setCost(servant.cost);
                card.setAttack(servant.attack);
                card.setLife(servant.life);
                card.setSpellDamage(servant.spellDamage);
                await card.removeBuff(game, card.getBuffs());
                card.setKeywords(game, new string[0]);
                foreach (string keyword in servant.keywords)
                {
                    if (string.IsNullOrEmpty(keyword))
                        continue;
                    card.setProp(keyword, true);
                }
                card.setProp(nameof(ServantCardDefine.keywords), servant.keywords);
                card.setProp(nameof(ServantCardDefine.tags), servant.tags);
            }
        }
        public static async Task resetCard(this IEnumerable<Card> cards, THHGame game)
        {
            foreach (var card in cards)
            {
                await resetCard(card, game);
            }
        }

        public static async Task activeEffect(this Card card, THHGame game, THHPlayer player, Card[] targets)
        {
            ITriggerEffect triggerEffect = card.define.getEffectOn<THHPlayer.ActiveEventArg>(game.triggers);
            if (triggerEffect != null)
            {
                await triggerEffect.execute(game, card, new object[] { new THHPlayer.ActiveEventArg(player, card, targets) }, targets);
            }
            IActiveEffect activeEffect = card.define.getActiveEffect();
            if (activeEffect != null)
            {
                await activeEffect.execute(game, card, new object[] { new THHPlayer.ActiveEventArg(player, card, targets) }, targets);
            }
        }
        public static Task<AttackEventArg> tryAttack(this Card card, THHGame game, THHPlayer player, Card target)
        {
            if (!card.canAttack(game))
            {
                game.logger.log(card + "无法进行攻击");
                return Task.FromResult<AttackEventArg>(null);
            }
            if (!card.isAttackable(game, player, target, out var reason))
            {
                game.logger.log(card + "无法攻击" + target + "，因为" + reason);
                return Task.FromResult<AttackEventArg>(null);
            }
            return card.attack(game, player, target);
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
            return card.attack(game, player, target);
        }
        static async Task<AttackEventArg> attack(this Card card, THHGame game, THHPlayer player, Card target)
        {
            AttackEventArg eventArg = new AttackEventArg() { card = card, target = target };
            await game.triggers.doEvent(eventArg, async arg =>
            {
                card = arg.card;
                target = arg.target;

                game.logger.log(card + "攻击" + target);
                if (card.getAttack(game) > 0)
                    await target.damage(game, card, card.getAttack(game));
                if (target.getAttack(game) > 0)
                    await card.damage(game, target, target.getAttack(game));
                //消耗攻击次数和耐久
                card.setAttackTimes(card.getAttackTimes(game) + 1);
                foreach (var p in game.players)
                {
                    if (p.master == card && p.item != null)
                    {
                        await p.item.damage(game, null, 1);
                        break;
                    }
                }
            });
            await game.updateDeath();
            return eventArg;
        }
        public class AttackEventArg : EventArg
        {
            public Card card;
            public Card target;
        }
        public static Task<DamageEventArg> damage(this Card card, THHGame game, Card source, int value)
        {
            return damage(new Card[] { card }, game, source, value);
        }
        public static async Task<DamageEventArg> damage(this IEnumerable<Card> cards, THHGame game, Card source, int value)
        {
            DamageEventArg eventArg = new DamageEventArg() { cards = cards.ToArray(), source = source, value = value };
            await game.triggers.doEvent(eventArg, async arg =>
            {
                cards = arg.cards;
                source = arg.source;
                value = arg.value;
                if (source != null && source.isStealth(game))
                    source.setStealth(game, false);
                if (value < 1)
                {
                    arg.isCanceled = true;
                }
                foreach (Card card in cards)
                {
                    if (card == null)
                        continue;
                    else if (card.isImmunity(game))
                    {
                        game.logger.log(card + "免疫，不受伤害");
                    }
                    else if (card.isShield(game))
                    {
                        game.logger.log(card + "受到伤害，失去圣盾");
                        card.setShield(game, false);
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
                                    arg.infoDic.Add(card, new DamageEventArg.Info(damage, card.getArmor(game), card.getCurrentLife(game)));
                                }
                                else
                                {
                                    int leakDamage = damage - armor;
                                    await card.loseArmor(game, armor);
                                    card.setCurrentLife(card.getCurrentLife(game) - leakDamage);
                                    game.logger.log(card + "受到" + damage + "点伤害，护甲=>" + card.getArmor(game) + "，生命值=>" + card.getCurrentLife(game));
                                    await onDealKeywords(game, source, card, damage);
                                    arg.infoDic.Add(card, new DamageEventArg.Info(damage, card.getArmor(game), card.getCurrentLife(game)));
                                }
                            }
                            else
                            {
                                card.setCurrentLife(card.getCurrentLife(game) - damage);
                                game.logger.log(card + "受到" + damage + "点伤害，生命值=>" + card.getCurrentLife(game));
                                await onDealKeywords(game, source, card, damage);
                                arg.infoDic.Add(card, new DamageEventArg.Info(damage, card.getArmor(game), card.getCurrentLife(game)));
                            }
                        }
                        else
                            game.logger.log(card + "的伤害" + value + "被减伤" + damageReduce + "抵消");
                    }
                }
            });
            return eventArg;
            async Task onDealKeywords(THHGame g, Card s, Card c, int d)
            {
                if (s != null)
                {
                    if (s.isPoisonous(g) && !g.players.Any(p => p.master == c))
                        c.setDead(true);
                    if (s.isDrain(g))
                        await s.getOwner().master.heal(g, d);
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
        public class DamageEventArg : EventArg
        {
            public Card[] cards;
            public Card source;
            public int value;
            public Dictionary<Card, Info> infoDic = new Dictionary<Card, Info>();
            public class Info
            {
                public int damagedValue;
                public int currentArmor;
                public int currentLife;
                public Info(int damage, int armor, int life)
                {
                    damagedValue = damage;
                    currentArmor = armor;
                    currentLife = life;
                }
            }
        }
        public static async Task heal(this IEnumerable<Card> cards, THHGame game, int value)
        {
            cards = cards.Where(c => c.getCurrentLife(game) < c.getLife(game));
            if (cards.Count() < 1)
                return;
            await game.triggers.doEvent(new HealEventArg() { cards = cards.ToArray(), value = value }, arg =>
            {
                cards = arg.cards;
                value = arg.value;
                foreach (Card card in cards)
                {
                    if (card.getCurrentLife(game) + value < card.getLife(game))
                    {
                        card.setCurrentLife(card.getCurrentLife(game) + value);
                        arg.infoDic.Add(card, new HealEventArg.Info()
                        {
                            healedValue = value
                        });
                        game.logger.log(card + "恢复" + value + "点生命值，生命值=>" + card.getCurrentLife(game));
                    }
                    else
                    {
                        int healedValue = card.getLife(game) - card.getCurrentLife(game);
                        card.setCurrentLife(card.getLife(game));
                        arg.infoDic.Add(card, new HealEventArg.Info()
                        {
                            healedValue = healedValue
                        });
                        game.logger.log(card + "恢复" + healedValue + "点生命值，生命值=>" + card.getCurrentLife(game));
                    }
                }
                return Task.CompletedTask;
            });
        }
        public static async Task heal(this Card card, THHGame game, int value)
        {
            await heal(new Card[] { card }, game, value);
        }
        public class HealEventArg : EventArg
        {
            public Card[] cards;
            public int value;
            public Dictionary<Card, Info> infoDic = new Dictionary<Card, Info>();
            public class Info
            {
                public int healedValue;
            }
        }
        public static Task die(this Card card, THHGame game)
        {
            return die(new Card[] { card }, game);
        }
        public static async Task die(this IEnumerable<Card> cards, THHGame game)
        {
            List<THHPlayer> remainPlayerList = new List<THHPlayer>(game.players);
            await game.triggers.doEvent(new DeathEventArg() { infoDic = cards.ToDictionary(c => c, c => default(DeathEventArg.Info)) }, onDie);
            Task onDie(DeathEventArg arg)
            {
                Dictionary<Card, DeathEventArg.Info> infoDic = new Dictionary<Card, DeathEventArg.Info>();
                foreach (var card in arg.infoDic.Keys)
                {
                    if (card.pile.name == PileName.MASTER)//英雄阵亡
                    {
                        infoDic.Add(card, new DeathEventArg.Info()
                        {
                            player = card.getOwner(),
                            position = card.pile.indexOf(card)
                        });
                        remainPlayerList.Remove(card.getOwner());
                        game.logger.log(card.getOwner() + "失败");
                    }
                    else if (card.pile.name == PileName.FIELD)//随从阵亡
                    {
                        infoDic.Add(card, new DeathEventArg.Info()
                        {
                            player = card.getOwner(),
                            position = card.pile.indexOf(card)
                        });
                        card.pile.moveTo(game, card, card.getOwner().grave);
                        game.logger.log(card + "阵亡");
                    }
                    else if (card.pile.name == PileName.ITEM)//装备摧毁
                    {
                        infoDic.Add(card, new DeathEventArg.Info()
                        {
                            player = card.getOwner(),
                            position = card.pile.indexOf(card)
                        });
                        card.pile.moveTo(game, card, card.getOwner().grave);
                        game.logger.log(card + "摧毁");
                    }
                }
                arg.infoDic = infoDic;
                return Task.CompletedTask;
            }
            if (remainPlayerList.Count != game.players.Length)
            {
                if (remainPlayerList.Count > 0)
                    await game.gameEnd(remainPlayerList.ToArray());
                else
                    await game.gameEnd(new THHPlayer[0]);
            }
        }
        public class DeathEventArg : EventArg
        {
            public Dictionary<Card, Info> infoDic;
            public class Info
            {
                public THHPlayer player;
                public int position;
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
        public static async Task backToHand(this Card card, THHGame game)
        {
            if (card.getOwner().hand.isFull)
            {
                game.logger.log(card.getOwner() + "的手牌已满，无法将" + card + "置入手牌");
                card.setDead(true);
            }
            else
            {
                game.logger.log("将" + card + "置入" + card.getOwner() + "的手牌");
                await card.resetCard(game);
                await card.getOwner().field.moveTo(game, card, card.getOwner().hand);
            }
        }
        public static async Task backToHand(this IEnumerable<Card> cards, THHGame game)
        {
            foreach (IGrouping<THHPlayer, Card> g in cards.GroupBy(c => c.getOwner()))
            {
                if (g.Key.hand.isFull)
                {
                    game.logger.log(g.Key + "的手牌已满，无法将" + string.Join(",", g) + "置入手牌");
                    g.setDead(true);
                }
                else if (g.Key.hand.count + g.Count() <= g.Key.hand.maxCount)
                {
                    game.logger.log("将" + string.Join("，", g) + "置入" + g.Key + "的手牌");
                    await cards.resetCard(game);
                    await g.Key.field.moveTo(game, g, g.Key.hand);
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
    }
}