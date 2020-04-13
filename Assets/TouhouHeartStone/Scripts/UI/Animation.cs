using System;

using UnityEngine;
using TouhouCardEngine.Interfaces;

namespace UI
{
    [Serializable]
    abstract class Animation
    {
        [SerializeField]
        string _name;
        public string name
        {
            get { return _name; }
        }
        public Animation()
        {
            _name = GetType().Name;
        }
        public Animation(string name)
        {
            _name = name;
        }
        public abstract bool update(Table table);
    }
    [Serializable]
    abstract class Animation<T> : Animation where T : IEventArg
    {
        public abstract T eventArg { get; }
    }
}
