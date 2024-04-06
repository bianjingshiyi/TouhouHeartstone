using System;
using System.Collections.Generic;
using Game.BattleTable;
using MessagePack;
using UnityEngine;

namespace Game.Animations
{
    /// <summary>
    /// 动画定义
    /// </summary>
    public class GeneratedAnimationDefine : AnimationDefine
    {
        public GeneratedAnimationDefine(long cardPoolId, int id, string name) : base(cardPoolId, id, name)
        {

        }
        public GeneratedAnimationDefine(int id, string name) : this(0, id, name)
        {

        }
        public override TableEffect createEffect(BattleTableManager table, Transform parent)
        {
            throw new NotImplementedException();
        }
    }
}