using System;
using System.Collections.Generic;
namespace UI
{
    public class ActionEvent<T0, T1>
    {
        List<Action<T0, T1>> _actionList = new List<Action<T0, T1>>();
        public bool preventRepeatRegister { get; set; } = true;
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
        public void clear()
        {
            _actionList.Clear();
        }
        public void invoke(T0 t0, T1 t1)
        {
            foreach (var action in _actionList.ToArray())
            {
                if (action != null)
                    action.Invoke(t0, t1);
            }
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