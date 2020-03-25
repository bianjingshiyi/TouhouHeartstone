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
            return card.getProp<bool>("taunt");
        }
        public static void setTaunt(this Card card, bool value)
        {
            card.setProp("taunt", value);
        }
        public static async Task<bool> tryAttack(this Card card, THHGame game, Card target)
        {
            if (card.getAttackTimes() >= card.getMaxAttackTimes())
                return false;
            await game.triggers.doEvent(new AttackEventArg() { card = card, target = target }, async arg =>
            {
                game.logger.log(arg.card + "攻击" + arg.target);
                arg.card.setAttackTimes(arg.card.getAttackTimes() + 1);
                if (arg.card.getAttack() > 0)
                    await arg.target.damage(game, arg.card.getAttack());
                if (arg.target.getAttack() > 0)
                    await arg.card.damage(game, arg.target.getAttack());
                await game.updateDeath();
            });
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
        public static async Task die(this Card[] cards, THHGame game, THHPlayer[] players)
        {
            List<THHPlayer> remainPlayerList = new List<THHPlayer>(game.players);
            await game.triggers.doEvent(new DeathEventArg() { cards = cards }, arg =>
            {
                for (int i = 0; i < arg.cards.Length; i++)
                {
                    Card card = arg.cards[i];
                    if (!players.Any(p => p.field.Contains(card) || p.master == card))//已经不在战场上了，没法死
                        continue;
                    THHPlayer player = players.FirstOrDefault(p => p.master == card);
                    if (player != null)
                    {
                        remainPlayerList.Remove(player);
                        game.logger.log(player + "失败");
                    }
                    else
                    {
                        players[i].field.moveTo(card, players[i].grave);
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
            public Card[] cards;
        }
    }
}