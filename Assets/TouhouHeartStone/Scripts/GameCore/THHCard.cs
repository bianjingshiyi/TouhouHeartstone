using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
namespace TouhouHeartstone
{
    public static class THHCard
    {
        public static THHPlayer getOwner(this Card card)
        {
            return card.owner as THHPlayer;
        }
        public static int getCost(this Card card)
        {
            return card.getProp<int>(nameof(ServantCardDefine.cost));
        }
        public static void setCost(this Card card, int value)
        {
            card.setProp(nameof(ServantCardDefine.cost), value);
        }
        public static int getCost(this CardDefine card)
        {
            return card.getProp<int>(nameof(ServantCardDefine.cost));
        }
        public static int getAttack(this Card card)
        {
            int result = card.getProp<int>(nameof(ServantCardDefine.attack));
            if (result < 0)
                result = 0;
            return result;
        }
        public static int getAttack(this CardDefine card)
        {
            int result = card.getProp<int>(nameof(ServantCardDefine.attack));
            if (result < 0)
                result = 0;
            return result;
        }
        public static void setAttack(this Card card, int value)
        {
            card.setProp(nameof(ServantCardDefine.attack), value);
        }
        public static int getLife(this Card card)
        {
            return card.getProp<int>(nameof(ServantCardDefine.life));
        }
        public static int getLife(this CardDefine card)
        {
            return card.getProp<int>(nameof(ServantCardDefine.life));
        }
        public static void setLife(this Card card, int value)
        {
            card.setProp(nameof(ServantCardDefine.life), value);
        }
        public static int getArmor(this Card card)
        {
            return card.getProp<int>("armor");
        }
        public static int getCurrentLife(this Card card)
        {
            return card.getProp<int>("currentLife");
        }
        public static void setCurrentLife(this Card card, int value)
        {
            card.setProp("currentLife", value);
        }
        public static bool isReady(this Card card)
        {
            return card.getProp<bool>("isReady");
        }
        public static void setReady(this Card card, bool value)
        {
            card.setProp("isReady", value);
        }
        public static int getAttackTimes(this Card card)
        {
            return card.getProp<int>("attackTimes");
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
            if (card.getAttack() <= 0)//没有攻击力
                return false;
            if (!card.isReady()//还没准备好
                && !card.isCharge()//且没有冲锋
                && !(card.isRush() && game.getOpponent(card.getOwner()).field.Any(c => card.isAttackable(game, card.getOwner(), c, out _)))//且并非有突袭且有可以攻击的敌方随从
                )
                return false;
            if (card.getAttackTimes() >= card.getMaxAttackTimes())//已经攻击过了
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
            if (target.getCurrentLife() <= 0)
            {
                tip = "目标随从已经死亡";
                return false;
            }
            if (game.getOpponent(player).field.Any(c => c.isTaunt()) && !target.isTaunt())
            {
                tip = "你必须先攻击具有嘲讽的随从";
                return false;
            }
            if (card.isRush() && !card.isReady() && game.players.Any(p => p.master == target) && !card.isCharge())
            {
                tip = "具有突袭的随从在没有准备好的情况下不能攻击敌方英雄";//除非你具有冲锋
                return false;
            }
            if (target.isStealth())
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
        public static bool isUsed(this Card card)
        {
            return card.getProp<bool>("isUsed");
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
        public static bool isTaunt(this Card card)
        {
            return card.getProp<bool>(Keyword.TAUNT);
        }
        public static void setTaunt(this Card card, bool value)
        {
            card.setProp(Keyword.TAUNT, value);
        }
        public static bool isCharge(this Card card)
        {
            return card.getProp<bool>(Keyword.CHARGE);
        }
        public static void setCharge(this Card card, bool value)
        {
            card.setProp(Keyword.CHARGE, value);
        }
        public static bool isRush(this Card card)
        {
            return card.getProp<bool>(Keyword.RUSH);
        }
        public static void setRush(this Card card, bool value)
        {
            card.setProp(Keyword.RUSH, value);
        }
        public static bool isShield(this Card card)
        {
            return card.getProp<bool>(Keyword.SHIELD);
        }
        public static void setShield(this Card card, bool value)
        {
            card.setProp(Keyword.SHIELD, value);
        }
        public static bool isStealth(this Card card)
        {
            return card.getProp<bool>(Keyword.STEALTH);
        }
        public static void setStealth(this Card card, bool value)
        {
            card.setProp(Keyword.STEALTH, value);
        }
        public static bool isDrain(this Card card)
        {
            return card.getProp<bool>(Keyword.DRAIN);
        }
        public static void setDrain(this Card card, bool value)
        {
            card.setProp(Keyword.DRAIN, value);
        }
        public static bool isPoisonous(this Card card)
        {
            return card.getProp<bool>(Keyword.POISONOUS);
        }
        public static void setPoisonous(this Card card, bool value)
        {
            card.setProp(Keyword.POISONOUS, value);
        }
        public static bool isMagicImmune(this Card card)
        {
            return card.getProp<bool>(Keyword.MAGICIMMUNE);
        }
        public static void setMagicImmune(this Card card, bool value)
        {
            card.setProp(Keyword.MAGICIMMUNE, value);
        }

        public static int getSpellDamage(this Card card)
        {
            return card.getProp<int>(nameof(ServantCardDefine.spellDamage));
        }
        public static void setSpellDamage(this Card card, int value)
        {
            card.setProp(nameof(ServantCardDefine.spellDamage), value);
        }
        public static bool hasTag(this Card card, string tag)
        {
            return card.getProp<string[]>(nameof(ServantCardDefine.tags)).Contains(tag);
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
                if (player.gem < card.getCost())//费用不够
                {
                    info = "你没有足够的法力值";
                    return false;
                }
                if (player.field.count >= player.field.maxCount)
                {
                    info = "你无法将更多的随从置入战场";
                    return false;
                }
            }
            else if (card.define is SpellCardDefine spell)
            {
                if (player.gem < card.getCost())
                {
                    info = "你没有足够的法力值";
                    return false;
                }
            }
            else if (card.define is SkillCardDefine skill)
            {
                if (card.isUsed())//已经用过了
                {
                    info = "你已经使用过技能了";
                    return false;
                }
                if (player.gem < card.getCost())//费用不够
                {
                    info = "你没有足够的法力值";
                    return false;
                }
                if (card.define.getEffectOn<THHPlayer.ActiveEventArg>(game.triggers) is IActiveEffect effect && !effect.checkCondition(game, card, new object[]
                    {
                        new THHPlayer.ActiveEventArg(player,card,new object[0])
                    }))
                {
                    info = "技能不可用";
                    return false;
                }
            }
            else
            {
                info = "这是一张未知的卡牌";
                return false;//不知道是什么卡
            }
            info = null;
            return true;
        }
        public static Card[] getAvaliableTargets(this Card card, THHGame game)
        {
            IActiveEffect effect = card.define.getEffectOn<THHPlayer.ActiveEventArg>(game.triggers) as IActiveEffect;
            if (effect == null)
                return null;
            List<Card> targetList = new List<Card>();
            foreach (THHPlayer player in game.players)
            {
                if (effect.checkTarget(game, null, card, new object[] { player.master }))
                    targetList.Add(player.master);
                foreach (Card servant in player.field)
                {
                    if (effect.checkTarget(game, null, card, new object[] { servant }) && !servant.isMagicImmune())
                        targetList.Add(servant);
                }
            }
            return targetList.ToArray();
        }
        public static bool isValidTarget(this Card card, THHGame game, Card target)
        {
            IActiveEffect effect = card.define.getEffectOn<THHPlayer.ActiveEventArg>(game.triggers) as IActiveEffect;
            if (effect == null)
                return false;
            return effect.checkTarget(game, null, card, new object[] { target });
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
                arg.card.setAttackTimes(arg.card.getAttackTimes() + 1);
                if (arg.card.getAttack() > 0)
                    await arg.target.damage(game, arg.card, arg.card.getAttack());
                if (arg.target.getAttack() > 0)

                    await arg.card.damage(game, arg.target.getAttack());
                if (arg.card.isDrain())
                    await player.master.heal(game, arg.card.getAttack());
                if (arg.target.isDrain())
                    await (arg.target.owner as THHPlayer).master.heal(game, arg.target.getAttack());
                if (arg.card.isPoisonous() && arg.target.owner != null)
                {
                    DamageEventArg damage = game.triggers.getRecordedEvents().LastOrDefault(e => e is THHCard.DamageEventArg) as THHCard.DamageEventArg;
                    //剧毒角色造成伤害后，对方死亡
                    if (damage.value > 0)
                    {
                        await arg.target.die(game, new DeathEventArg.Info()
                        {
                            card = target,
                            player = (THHPlayer)arg.target.owner,
                            position = player.field.indexOf(card)
                        });
                    }
                }
                if (arg.target.isPoisonous() && arg.card != player.master)
                {
                    DamageEventArg damage = game.triggers.getRecordedEvents().LastOrDefault(e => e is THHCard.DamageEventArg) as THHCard.DamageEventArg;
                    if (damage.value > 0)
                    {
                        await arg.card.die(game, new DeathEventArg.Info()
                        {
                            card = card,
                            player = player,
                            position = player.field.indexOf(card)
                        });
                    }
                }
            });
            await game.updateDeath();
            return true;
        }
        public class AttackEventArg : EventArg
        {
            public Card card;
            public Card target;
        }
        public static Task damage(this Card card, THHGame game, Card source, int value)
        {
            return damage(new Card[] { card }, game, source, value);
        }
        public static async Task damage(this IEnumerable<Card> cards, THHGame game, Card source, int value)
        {
            await game.triggers.doEvent(new DamageEventArg() { cards = cards.ToArray(), source = source, value = value }, arg =>
            {
                cards = arg.cards;
                source = arg.source;
                value = arg.value;
                if (source != null && source.isStealth())
                    source.setStealth(false);
                foreach (Card card in arg.cards)
                {
                    if (card.isShield())
                    {
                        card.setShield(false);
                        arg.infoDic.Add(card, new DamageEventArg.Info()
                        {
                            damagedValue = 0,
                            currentLife = card.getCurrentLife()
                        });
                        game.logger.log(card + "受到伤害，失去圣盾");
                    }
                    else
                    {
                        card.setCurrentLife(card.getCurrentLife() - arg.value);
                        arg.infoDic.Add(card, new DamageEventArg.Info()
                        {
                            damagedValue = value,
                            currentLife = card.getCurrentLife()
                        });
                        game.logger.log(card + "受到" + arg.value + "点伤害，生命值=>" + card.getCurrentLife());
                    }
                }
                return Task.CompletedTask;
            });
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
            cards = cards.Where(c => c.getCurrentLife() < c.getLife());
            if (cards.Count() < 1)
                return;
            await game.triggers.doEvent(new HealEventArg() { cards = cards.ToArray(), value = value }, arg =>
            {
                cards = arg.cards;
                value = arg.value;
                foreach (Card card in cards)
                {
                    if (card.getCurrentLife() + value < card.getLife())
                    {
                        card.setCurrentLife(card.getCurrentLife() + value);
                        arg.infoDic.Add(card, new HealEventArg.Info()
                        {
                            healedValue = value
                        });
                        game.logger.log(card + "恢复" + value + "点生命值，生命值=>" + card.getCurrentLife());
                    }
                    else
                    {
                        int healedValue = card.getLife() - card.getCurrentLife();
                        card.setCurrentLife(card.getLife());
                        arg.infoDic.Add(card, new HealEventArg.Info()
                        {
                            healedValue = healedValue
                        });
                        game.logger.log(card + "恢复" + healedValue + "点生命值，生命值=>" + card.getCurrentLife());
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
        public static Task die(this Card card, THHGame game, DeathEventArg.Info info)
        {
            return die(new Card[] { card }, game, new Dictionary<Card, DeathEventArg.Info>() { { card, info } });
        }
        public static async Task die(this IEnumerable<Card> cards, THHGame game, Dictionary<Card, DeathEventArg.Info> infoDic)
        {
            List<THHPlayer> remainPlayerList = new List<THHPlayer>(game.players);
            await game.triggers.doEvent(new DeathEventArg() { infoDic = infoDic }, arg =>
            {
                infoDic = arg.infoDic;
                foreach (var pair in infoDic)
                {
                    Card card = pair.Key;
                    if (!game.players.Any(p => p.field.Contains(card) || p.master == card))
                        continue;
                    THHPlayer player = game.players.FirstOrDefault(p => p.master == card);
                    if (player != null)
                    {
                        remainPlayerList.Remove(player);
                        game.logger.log(player + "失败");
                    }
                    else
                    {
                        pair.Value.player.field.moveTo(game, card, pair.Value.player.grave);
                        game.logger.log(card + "阵亡");
                    }
                }
                return Task.CompletedTask;
            });
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
            public Dictionary<Card, Info> infoDic = new Dictionary<Card, Info>();
            public class Info
            {
                public THHPlayer player;
                public Card card;
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
    }
}