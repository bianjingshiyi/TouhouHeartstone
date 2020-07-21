using System;

using UnityEngine;
using TouhouCardEngine.Interfaces;
using Game;
namespace UI
{
    public abstract class RequestAnimation : TableAnimation
    {
        public abstract IRequest request { get; set; }
    }
    [Serializable]
    public abstract class RequestAnimation<T> : RequestAnimation where T : IRequest
    {
        [SerializeField]
        T _request;
        public override IRequest request
        {
            get { return _request; }
            set { _request = value is T t ? t : default; }
        }
        public override bool update(TableManager table)
        {
            return display(table, request is T t ? t : default);
        }
        public abstract bool display(TableManager table, T request);
    }
}
