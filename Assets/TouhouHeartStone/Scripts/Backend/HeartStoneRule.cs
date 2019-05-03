using System;
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
            //pool = new CardPool(new CardDefine[]
            //{
            //    new BudFairy(),
            //    new FairyTwins(),
            //    new Reimu(),
            //    new Marisa()
            //});
            pool = new CardPool(typeof(HeartStoneRule).Assembly.GetTypes().
                                Where(t => { return !t.IsAbstract && t.IsSubclassOf(typeof(CardDefine)); }).
                                Select(t =>
                                {
                                    return t.GetConstructor(new Type[0]).Invoke(new object[0]) as CardDefine;
                                }).ToArray());
        }
        public override CardPool pool { get; } = null;
        public override void beforeEvent(CardEngine game, Event e)
        {
        }
        public override void afterEvent(CardEngine engine, Event e)
        {
            Player[] sortedPlayers = engine.getProp<Player[]>("sortedPlayers");
            if (e is InitReplaceEvent)
            {
                InitReplaceEvent E = e as InitReplaceEvent;
                //玩家准备完毕
                E.player.setProp("prepared", true);
                //判断是否所有玩家都准备完毕
                if (engine.getPlayers().All(p => { return p.getProp<bool>("prepared"); }))
                {
                    //对战开始
                    engine.doEvent(new StartEvent());
                }
            }
            else if (e is StartEvent)
            {
                engine.doEvent(new TurnStartEvent(sortedPlayers[0]));
            }
            else if (e is TurnEndEvent)
            {
                int index = Array.IndexOf(sortedPlayers, engine.getProp<Player>("currentPlayer"));
                index++;
                if (index >= sortedPlayers.Length)
                    index = 0;
                Player nextPlayer = sortedPlayers[index];
                engine.doEvent(new TurnStartEvent(nextPlayer));
            }
        }
    }
}