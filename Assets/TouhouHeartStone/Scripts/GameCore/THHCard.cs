using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using TouhouCardEngine;

namespace TouhouHeartstone
{
    public static class THHCard
    {
        public static int getCost(this Card card)
        {
            return card.getProp<int>(nameof(ServantCardDefine.cost));
        }
        public static int getAttack(this Card card)
        {
            return card.getProp<int>(nameof(ServantCardDefine.attack));
        }
        public static int getLife(this Card card)
        {
            return card.getProp<int>(nameof(ServantCardDefine.life));
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
        public static bool isUsed(this Card card)
        {
            return card.getProp<bool>("isUsed");
        }
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
        public static bool isUsable(this Card card, THHGame game, THHPlayer player, out string info)
        {
            if (game.currentPlayer != player)//不是你的回合
            {
                info = "这不是你的回合";
                return false;
            }
            if (card.define is ServantCardDefine servant)
            {
                if (player.gem < card.getCost())//费用不够
                {
                    info = "你没有足够的法力值";
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
            }
            else
            {
                info = "这是一张未知的卡牌";
                return false;//不知道是什么卡
            }
            info = null;
            return true;
        }
        public static async Task<bool> tryAttack(this Card card, THHGame game, Card target)
        {
            if (!card.isReady())//还没准备好
                return false;
            if (card.getAttackTimes() >= card.getMaxAttackTimes())//已经攻击过了
                return false;
            await game.triggers.doEvent(new AttackEventArg() { card = card, target = target }, async arg =>
            {
                game.logger.log(arg.card + "攻击" + arg.target);
                arg.card.setAttackTimes(arg.card.getAttackTimes() + 1);
                if (arg.card.getAttack() > 0)
                    await arg.target.damage(game, arg.card.getAttack());
                if (arg.target.getAttack() > 0)
                    await arg.card.damage(game, arg.target.getAttack());
            });
            await game.updateDeath();
            return true;
        }
        public class AttackEventArg : EventArg
        {
            public Card card;
            public Card target;
        }
        public static Task damage(this Card card, THHGame game, int value)
        {
            return damage(new Card[] { card }, game, value);
        }
        public static async Task damage(this Card[] cards, THHGame game, int value)
        {
            await game.triggers.doEvent(new DamageEventArg() { cards = cards, value = value }, arg =>
            {
                foreach (Card card in arg.cards)
                {
                    card.setCurrentLife(card.getCurrentLife() - arg.value);
                    game.logger.log(card + "受到" + arg.value + "点伤害，生命值=>" + card.getCurrentLife());
                }
                return Task.CompletedTask;
            });
        }
        public class DamageEventArg : EventArg
        {
            public Card[] cards;
            public int value;
        }
        public static async Task heal(IEnumerable<Card> cards, THHGame game, int value)
        {
            foreach (Card card in cards)
            {
                await card.heal(game, value);
            }
        }
        public static async Task heal(this Card card, THHGame game, int value)
        {
            if (card.getCurrentLife() >= card.getLife())
                return;
            await game.triggers.doEvent(new HealEventArg() { card = card, value = value }, arg =>
            {
                card = arg.card;
                value = arg.value;
                if (card.getCurrentLife() + value < card.getLife())
                {
                    card.setCurrentLife(card.getCurrentLife() + value);
                    game.logger.log(card + "恢复" + value + "点生命值，生命值=>" + card.getCurrentLife());
                }
                else
                {
                    int healedValue = card.getLife() - card.getCurrentLife();
                    card.setCurrentLife(card.getLife());
                    game.logger.log(card + "恢复" + healedValue + "点生命值，生命值=>" + card.getCurrentLife());
                }
                return Task.CompletedTask;
            });
        }
        public class HealEventArg : EventArg
        {
            public Card card;
            public int value;
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
    }
}