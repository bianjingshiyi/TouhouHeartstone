using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using TouhouHeartstone.Backend.Builtin;

namespace TouhouHeartstone.Backend
{
    /// <summary>
    /// 这个炉石规则是测试用的。
    /// </summary>
    public class HeartStoneRule : Rule
    {
        public override CardPool pool { get; } = null;
        public HeartStoneRule(IGameEnvironment env, params CardDefine[] cards)
        {
            pool = new CardPool();
            //加载卡池
            //加载内置卡池
            Dictionary<int, CardDefine> dicCards = typeof(HeartStoneRule).Assembly.GetTypes().
                              Where(t =>
                              {
                                  return !t.IsAbstract && t.IsSubclassOf(typeof(CardDefine)) &&
                                         (t.GetConstructor(new Type[0]) != null ||
                                          t.GetConstructor(new Type[] { typeof(IGameEnvironment) }) != null);
                              }).
                              Select(t =>
                              {
                                  ConstructorInfo constructor = t.GetConstructor(new Type[0]);
                                  if (constructor != null)
                                      return constructor.Invoke(new object[0]) as CardDefine;
                                  else
                                  {
                                      constructor = t.GetConstructor(new Type[] { typeof(IGameEnvironment) });
                                      return constructor.Invoke(new object[] { env }) as CardDefine;
                                  }
                              }).ToDictionary(d =>
                              {
                                  return d.id;
                              });
            //加载参数卡池
            for (int i = 0; i < cards.Length; i++)
            {
                if (!dicCards.ContainsKey(cards[i].id))
                    dicCards.Add(cards[i].id, cards[i]);
                else
                    throw new ArgumentException("存在重复的卡片定义id" + cards[i].id);
            }
            //加载外置卡池
            if (env != null)
            {
                foreach (string path in env.getFiles("Cards", "*.thcd"))
                {
                    using (TextReader reader = env.getFileReader(path))
                    {
                        GeneratedCardDefine card = CardFileHelper.read(reader);
                        if (dicCards.ContainsKey(card.id))
                            throw new ArgumentException("存在重复的卡片定义id" + card.id);
                        else
                            dicCards.Add(card.id, card);
                    }
                }
            }
            pool = new CardPool(dicCards.Values.ToArray());
        }
        public override void beforeEvent(CardEngine game, Event e)
        {
        }
        public override void afterEvent(CardEngine engine, Event e)
        {
            Player[] sortedPlayers = engine.getProp<Player[]>("sortedPlayers");
            if (e.name is "onInitReplace")
            {
                //玩家准备完毕
                e.getProp<Player>("player").setProp("prepared", true);
                //判断是否所有玩家都准备完毕
                if (engine.getPlayers().All(p => { return p.getProp<bool>("prepared"); }))
                {
                    //对战开始
                    engine.start();
                }
            }
            else if (e.name == "onStart")
            {
                engine.turnStart(sortedPlayers[0]);
            }
            else if (e.name == "onTurnEnd")
            {
                int index = Array.IndexOf(sortedPlayers, engine.getProp<Player>("currentPlayer"));
                index++;
                if (index >= sortedPlayers.Length)
                    index = 0;
                Player nextPlayer = sortedPlayers[index];
                engine.turnStart(nextPlayer);
            }
        }
    }
}