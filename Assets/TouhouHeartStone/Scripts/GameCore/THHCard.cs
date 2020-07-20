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
            int result = card.getProp<int>(game, nameof(ServantCardDefine.attack));
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
        public static int getArmor(this Card card, IGame game)
        {
            return card.getProp<int>(game, "armor");
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
        public static int getMaxAttackTimes(this Card card)
        {
            return 1;
        }
        /// <summary>
        /// 这个角色能否进行攻击？
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public static bool canAttack(this Card card, THHGame game)
        {
            if (card.pile.name != PileName.FIELD)//你只能在战场上进行攻击。是不是必须得活着，这是个哲学问题。
                return false;
            if (card.getAttack(game) <= 0)//没有攻击力
                return false;
            if (!card.isReady(game)//还没准备好
                && !card.isCharge(game)//且没有冲锋
                && !(card.isRush(game) && game.getOpponent(card.getOwner()).field.Any(c => card.isAttackable(game, card.getOwner(), c, out _)))//且并非有突袭且有可以攻击的敌方随从
                )
                return false;
            if (card.getAttackTimes(game) >= card.getMaxAttackTimes())//已经攻击过了
                return false;
            if (card.isFreeze(game))
            {
                game.logger.log(card + "被冰冻");
                return false;
            }
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
            if (target.getCurrentLife(game) <= 0)
            {
                tip = "目标随从已经死亡";
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
            tip = null;
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
            return card.getProp<bool>(game, Keyword.TAUNT);
        }
        public static void setTaunt(this Card card, bool value)
        {
            card.setProp(Keyword.TAUNT, value);
        }
        public static bool isCharge(this Card card, IGame game)
        {
            return card.getProp<bool>(game, Keyword.CHARGE);
        }
        public static void setCharge(this Card card, bool value)
        {
            card.setProp(Keyword.CHARGE, value);
        }
        public static bool isRush(this Card card, IGame game)
        {
            return card.getProp<bool>(game, Keyword.RUSH);
        }
        public static void setRush(this Card card, bool value)
        {
            card.setProp(Keyword.RUSH, value);
        }
        public static bool isShield(this Card card, IGame game)
        {
            return card.getProp<bool>(game, Keyword.SHIELD);
        }
        public static void setShield(this Card card, bool value)
        {
            card.setProp(Keyword.SHIELD, value);
        }
        public static bool isStealth(this Card card, IGame game)
        {
            return card.getProp<bool>(game, Keyword.STEALTH);
        }
        public static void setStealth(this Card card, bool value)
        {
            card.setProp(Keyword.STEALTH, value);
        }
        public static bool isDrain(this Card card, IGame game)
        {
            return card.getProp<bool>(game, Keyword.DRAIN);
        }
        public static void setDrain(this Card card, bool value)
        {
            card.setProp(Keyword.DRAIN, value);
        }
        public static bool isPoisonous(this Card card, IGame game)
        {
            return card.getProp<bool>(game, Keyword.POISONOUS);
        }
        public static void setPoisonous(this Card card, bool value)
        {
            card.setProp(Keyword.POISONOUS, value);
        }
        public static bool isElusive(this Card card, IGame game)
        {
            return card.getProp<bool>(game, Keyword.ELUSIVE);
        }
        public static void setElusive(this Card card, bool value)
        {
            card.setProp(Keyword.ELUSIVE, value);
        }
        public static bool isFreeze(this Card card, IGame game)
        {
            return card.getProp<bool>(game, Keyword.FREEZE);
        }
        public static void setFreeze(this Card card, bool value)
        {
            card.setProp(Keyword.FREEZE, value);
        }
        public static bool isUnique(this Card card, IGame game)
        {
            return card.getProp<bool>(game, Keyword.UNIQUE);
        }
        public static void setUnique(this Card card, bool value)
        {
            card.setProp(Keyword.UNIQUE, value);
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
        public static async Task silence(this IEnumerable<Card> cards, THHGame game)
        {
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
        }
        public static Task silence(this Card card, THHGame game)
        {
            return silence(new Card[] { card }, game);
        }
        public static bool isSilenced(this Card card, IGame game)
        {
            return card.getProp<bool>(game, nameof(silence));
        }
        public static bool hasTag(this Card card, IGame game, string tag)
        {
            return card.getProp<string[]>(game, nameof(ServantCardDefine.tags)).Contains(tag);
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
                if (player.item != null)
                {
                    info = "你只能装备一个道具";
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
        public static async Task<bool> tryAttack(this Card card, THHGame game, THHPlayer player, Card target)
        {
            if (!card.canAttack(game))
            {
                game.logger.log(card + "无法进行攻击");
                return false;
            }
            if (!card.isAttackable(game, player, target, out var reason))
            {
                game.logger.log(card + "无法攻击" + target + "，因为" + reason);
                return false;
            }
            await game.triggers.doEvent(new AttackEventArg() { card = card, target = target }, async arg =>
            {
                game.logger.log(arg.card + "攻击" + arg.target);
                arg.card.setAttackTimes(arg.card.getAttackTimes(game) + 1);
                if (arg.card.getAttack(game) > 0)
                    await arg.target.damage(game, arg.card, arg.card.getAttack(game));
                if (arg.target.getAttack(game) > 0)
                    await arg.card.damage(game, arg.target, arg.target.getAttack(game));
                if (arg.card.isDrain(game))
                    await player.master.heal(game, arg.card.getAttack(game));
                if (arg.target.isDrain(game))
                    await (arg.target.owner as THHPlayer).master.heal(game, arg.target.getAttack(game));
            });
            await game.updateDeath();
            return true;
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
            await game.triggers.doEvent(eventArg, arg =>
            {
                cards = arg.cards;
                source = arg.source;
                value = arg.value;
                if (source != null && source.isStealth(game))
                    source.setStealth(false);
                if (value < 1)
                {
                    arg.isCanceled = true;
                    return Task.CompletedTask;
                }
                foreach (Card card in cards)
                {
                    if (card.isShield(game))
                    {
                        game.logger.log(card + "受到伤害，失去圣盾");
                        card.setShield(false);
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
                            card.setCurrentLife(card.getCurrentLife(game) - damage);
                            game.logger.log(card + "受到" + damage + "点伤害，生命值=>" + card.getCurrentLife(game));
                            if (source != null && source.isPoisonous(game))
                                card.setDead(true);
                            arg.infoDic.Add(card, new DamageEventArg.Info()
                            {
                                damagedValue = damage,
                                currentLife = card.getCurrentLife(game)
                            });
                        }
                        else
                            game.logger.log(card + "的伤害" + value + "被减伤" + damageReduce + "抵消");
                    }
                }
                return Task.CompletedTask;
            });
            return eventArg;
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
                public int currentLife;
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
        public static Task backToHand(this Card card, THHGame game)
        {
            if (card.getOwner().hand.isFull)
            {
                game.logger.log(card.getOwner() + "的手牌已满，无法将" + card + "置入手牌");
                return card.die(game);
            }
            else
            {
                game.logger.log("将" + card + "置入" + card.getOwner() + "的手牌");
                return card.getOwner().field.moveTo(game, card, card.getOwner().hand);
            }
        }
    }
}