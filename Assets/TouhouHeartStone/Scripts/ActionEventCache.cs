using System;
using System.Collections.Generic;
namespace Game
{
    public class ActionEventCache
    {
        List<CachedAction> cacheList { get; } = new List<CachedAction>();
        public void cache<T1>(Action<T1> handler, T1 t1)
        {
            cacheList.Add(new CachedAction<T1>(handler, t1));
        }
        abstract class CachedAction
        {
            public abstract void handle();
            public abstract string toString();
        }
        class CachedAction<T1> : CachedAction
        {
            Action<T1> handler;
            T1 t1;
            public CachedAction(Action<T1> handler, T1 t1)
            {
                this.handler = handler;
                this.t1 = t1;
            }
            public override void handle()
            {
                handler.Invoke(t1);
            }

            public override string toString()
            {
                return handler.Method.Name + "(" + t1 + ")";
            }
        }
        public void poll()
        {
            foreach (var cachedEvent in cacheList)
            {
                try
                {
                    cachedEvent.handle();
                }
                catch (Exception e)
                {
                    UberDebug.LogErrorChannel("Event", "处理事件" + cachedEvent + "发生异常：" + e);
                }
            }
            cacheList.Clear();
        }
    }
}
