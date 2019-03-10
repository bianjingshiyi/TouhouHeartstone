using System.Linq;
using System.Collections.Generic;

using TouhouHeartstone.Backend.Builtin;

namespace TouhouHeartstone.Backend
{
    public static class Keywords
    {
        public const string init = "Init";
        public const string deck = "Deck";
        public const string hand = "Hand";
    }
    /// <summary>
    /// 这个炉石规则是测试用的。
    /// </summary>
    public class HeartStoneRule : Rule
    {
        public HeartStoneRule()
        {
            pool = new CardPool(new CardDefine[]
            {
                new BudFairy(),
                new Reimu(),
                new Marisa()
            });
        }
        public override CardPool pool { get; } = null;
        public override void beforeEvent(CardEngine game, Event e)
        {
        }
        public override void afterEvent(CardEngine game, Event e)
        {
            if (e.name == "init")
                onInit(game);
            else if (e.name == "initReplace")
            {
                Player player = e.getVar<Player>(nameof(player));
                Card[] cards = e.getVar<Card[]>(nameof(cards));
                onInitReplace(game, player, cards);
            }
            else if (e.name == nameof(onInitReplace))
            {
                afterInitReplace(game, e.getVar<Player>("player"));
            }
        }
        internal void onInit(CardEngine core)
        {
            //确定行动顺序
            initPlayerOrder(core);
            //抽初始卡牌
            for (int i = 0; i < orderedPlayers.Length; i++)
            {
                initDraw(core, orderedPlayers[i], i == 0 ? 3 : 4);
            }
        }
        private void initPlayerOrder(CardEngine core)
        {
            List<Player> remainedList = new List<Player>(core.getPlayers());
            orderedPlayers = new Player[remainedList.Count];
            for (int i = 0; i < orderedPlayers.Length; i++)
            {
                int index = core.randomInt(0, remainedList.Count - 1);
                orderedPlayers[i] = remainedList[index];
                remainedList.RemoveAt(index);
            }
        }
        Player[] orderedPlayers { get; set; } = null;
        private void initDraw(CardEngine game, Player player, int count)
        {
            if (player == null)
                return;
            if (count < 1)
                return;
            for (int i = 0; i < count; i++)
            {
                player[Keywords.deck].moveCardTo(player[Keywords.deck][player[Keywords.deck].count - 1], player[Keywords.init], player[Keywords.init].count);
            }
        }
        private void onInitReplace(CardEngine core, Player player, Card[] cards)
        {
            if (player == null)
                return;
            if (cards == null || cards.Length < 1)
                return;
            for (int i = 0; i < cards.Length; i++)
            {
                player[Keywords.init].moveCardTo(cards[i], player[Keywords.deck], player[Keywords.deck].count);
            }
            //player[Keywords.deck].shuffle(core);
            for (int i = 0; i < cards.Length; i++)
            {
                player[Keywords.deck].moveCardTo(player[Keywords.deck][player[Keywords.deck].count - 1], player[Keywords.init], player[Keywords.init].count);
            }
        }
        private void afterInitReplace(TouhouHeartstone.CardEngine game, Player player)
        {
            preparedPlayerList.Add(player);
            if (game.getPlayers().All(p => { return preparedPlayerList.Contains(p); }))
            {
                start(game);
            }
        }
        List<Player> preparedPlayerList { get; } = new List<Player>();
        private void start(CardEngine game)
        {
            turnStart(orderedPlayers[0]);
        }
        private void turnStart(Player player)
        {

        }
        private void draw(CardEngine core, Player[] players)
        {
            if (players == null || players.Length == 0)
                return;
            foreach (Player player in players)
                player[Keywords.deck].moveCardTo(player[Keywords.deck][player[Keywords.deck].count - 1], player[Keywords.hand], player[Keywords.hand].count);
        }
        private void draw(CardEngine game, Player[] players, int count)
        {
            for (int i = 0; i < count; i++)
                draw(game, players);
        }
    }
    public abstract class MasterCardDefine : CardDefine
    {
        public abstract int category { get; }
        public override T getProp<T>(string propName)
        {
            if (propName == nameof(category))
                return (T)(object)category;
            else
                return base.getProp<T>(propName);
        }
    }
    public abstract class ServantCardDefine : CardDefine
    {
        public abstract int cost { get; }
        public abstract int attack { get; }
        public abstract int life { get; }
        public abstract int category { get; }
        public override T getProp<T>(string propName)
        {
            if (propName == nameof(cost))
                return (T)(object)cost;
            else if (propName == nameof(attack))
                return (T)(object)attack;
            else if (propName == nameof(life))
                return (T)(object)life;
            else
                return base.getProp<T>(propName);
        }
    }
    abstract class SpellCardDefine : CardDefine
    {
        public abstract int cost { get; }
        public abstract int category { get; }
        public override T getProp<T>(string propName)
        {
            if (propName == nameof(cost))
                return (T)(object)cost;
            else if (propName == nameof(category))
                return (T)(object)category;
            else
                return base.getProp<T>(propName);
        }
    }
}