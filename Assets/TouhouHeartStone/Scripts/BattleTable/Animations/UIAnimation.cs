using System;
using TouhouCardEngine.Interfaces;
using UnityEngine;
using Game.Animations;
using System.Collections;
using System.Collections.Generic;

namespace Game.BattleTable
{
    [Serializable]
    public class UIAnimation
    {
        [SerializeField]
        string _name;
        public string name
        {
            get { return _name; }
        }
#if UNITY_EDITOR
        [SerializeField]
        string _content;
#endif
        public UIAnimation()
        {
            _name = GetType().Name;
#if UNITY_EDITOR
            _content = ToString();
#endif
        }
        public UIAnimation(string name)
        {
            _name = name;
#if UNITY_EDITOR
            _content = ToString();
#endif
        }
#if UNITY_EDITOR
        public void updateDebugContent()
        {
            _content = ToString();
        }
#endif
    }
    [Serializable]
    public abstract class UIAnimation<T> : UIAnimation where T : IEventArg
    {
        public T eventArg { get; protected set; }
        public UIAnimation(T eventArg)
        {
            this.eventArg = eventArg;
        }
    }
}
