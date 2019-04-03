using System;
using System.Collections.Generic;

namespace TouhouHeartstone.Frontend
{
    /// <summary>
    /// 带有执行完毕回调的Event
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    /// <param name="callback"></param>
    public delegate void CallbackEvent(object sender, EventArgs args, GenericAction callback = null);

    /// <summary>
    /// 回调
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public delegate void GenericAction(object sender, EventArgs args);

    /// <summary>
    /// 回调Event集合，可以按顺序执行某些东西
    /// </summary>
    public class CallbackEventCollection
    {
        List<CallbackEvent> list = new List<CallbackEvent>();

        public static CallbackEventCollection operator +(CallbackEventCollection a, CallbackEvent b)
        {
            a.Add(b);
            return a;
        }

        public static CallbackEventCollection operator -(CallbackEventCollection a, CallbackEvent b)
        {
            a.Remove(b);
            return a;
        }

        public void Add(CallbackEvent evt)
        {
            list.Add(evt);
        }

        public void Remove(CallbackEvent evt)
        {
            list.Remove(evt);
        }

        public void Clear()
        {
            list.Clear();
        }

        public void Execute(object sender, EventArgs args, Action<object, EventArgs> callback = null)
        {
            new ExecuteEnvironment(list.ToArray(), sender, args, ref callback).Execute();
        }

        class ExecuteEnvironment
        {
            readonly CallbackEvent[] actions;
            readonly object sender;
            readonly EventArgs args;
            readonly Action<object, EventArgs> onFinish;

            int currentIndex = 0;

            public ExecuteEnvironment(CallbackEvent[] actions, object sender, EventArgs args, ref Action<object, EventArgs> onFinish)
            {
                this.actions = actions;
                this.args = args;
                this.sender = sender;
                this.onFinish = onFinish;
            }

            private void executeNext(object sender, EventArgs args)
            {
                if (currentIndex >= actions.Length)
                {
                    onFinish?.Invoke(sender, args);
                    return;
                }
                else
                {
                    var item = actions[currentIndex++];
                    item.Invoke(this.sender, this.args, executeNext);
                }
            }

            public void Execute()
            {
                executeNext(null, new EventArgs());
            }
        }
    }
}
