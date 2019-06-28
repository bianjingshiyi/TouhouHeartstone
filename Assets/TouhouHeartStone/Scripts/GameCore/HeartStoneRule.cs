﻿using System;
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
        public HeartStoneRule(IGameEnvironment env)
        {
            pool = new CardPool();
            //加载卡池
            //加载内置卡池
            Dictionary<int, CardDefine> cards = typeof(HeartStoneRule).Assembly.GetTypes().
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
            //加载外置卡池
            if (env != null)
            {
                foreach (string path in env.getFiles("Cards", "*.thcd"))
                {
                    using (TextReader reader = env.getFileReader(path))
                    {
                        GeneratedCardDefine card = CardFileHelper.read(reader);
                        if (cards.ContainsKey(card.id))
                            throw new ArgumentException("存在重复的卡片定义id" + card.id);
                        else
                            cards.Add(card.id, card);
                    }
                }
            }
            pool = new CardPool(cards.Values.ToArray());
        }
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