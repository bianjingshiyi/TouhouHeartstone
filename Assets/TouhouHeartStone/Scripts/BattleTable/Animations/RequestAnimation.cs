using System;
using TouhouCardEngine.Interfaces;
using UnityEngine;
namespace Game.BattleTable
{
    public abstract class RequestAnimation : TableAnimation
    {
        public override bool forceQueueAnimations => true;
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
        public override bool update(BattleTableManager table, float time)
        {
            return display(table, request is T t ? t : default);
        }
        public abstract bool display(BattleTableManager table, T request);
    }
}
