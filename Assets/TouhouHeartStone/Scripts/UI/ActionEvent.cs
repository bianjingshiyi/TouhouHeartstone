using System;
using System.Collections.Generic;
namespace UI
{
    public class ActionEvent
    {
        List<Action> _actionList = new List<Action>();
        public bool preventRepeatRegister { get; set; } = true;
        public virtual void clear()
        {
            _actionList.Clear();
        }
    }
    public class ActionEvent<T0> : ActionEvent
    {
        List<Action<T0>> _actionList = new List<Action<T0>>();
        public void add(Action<T0> action)
        {
            if (action == null)
                return;
            if (preventRepeatRegister && _actionList.Contains(action))
                return;
            _actionList.Add(action);
        }
        public void set(Action<T0> action)
        {
            clear();
            add(action);
        }
        public bool remove(Action<T0> action)
        {
            return _actionList.Remove(action);
        }
        public void invoke(T0 t0)
        {
            foreach (var action in _actionList.ToArray())
            {
                if (action != null)
                    action.Invoke(t0);
            }
        }
        public override void clear()
        {
            base.clear();
            _actionList.Clear();
        }
        public static ActionEvent<T0> operator +(ActionEvent<T0> actionEvent, Action<T0> action)
        {
            actionEvent.add(action);
            return actionEvent;
        }
        public static ActionEvent<T0> operator -(ActionEvent<T0> actionEvent, Action<T0> action)
        {
            actionEvent.remove(action);
            return actionEvent;
        }
    }
    public class ActionEvent<T0, T1> : ActionEvent
    {
        List<Action<T0, T1>> _actionList = new List<Action<T0, T1>>();
        public void add(Action<T0, T1> action)
        {
            if (action == null)
                return;
            if (preventRepeatRegister && _actionList.Contains(action))
                return;
            _actionList.Add(action);
        }
        public void set(Action<T0, T1> action)
        {
            clear();
            add(action);
        }
        public bool remove(Action<T0, T1> action)
        {
            return _actionList.Remove(action);
        }
        public void invoke(T0 t0, T1 t1)
        {
            foreach (var action in _actionList.ToArray())
            {
                if (action != null)
                    action.Invoke(t0, t1);
            }
        }
        public override void clear()
        {
            base.clear();
            _actionList.Clear();
        }
        public static ActionEvent<T0, T1> operator +(ActionEvent<T0, T1> actionEvent, Action<T0, T1> action)
        {
            actionEvent.add(action);
            return actionEvent;
        }
        public static ActionEvent<T0, T1> operator -(ActionEvent<T0, T1> actionEvent, Action<T0, T1> action)
        {
            actionEvent.remove(action);
            return actionEvent;
        }
    }
}