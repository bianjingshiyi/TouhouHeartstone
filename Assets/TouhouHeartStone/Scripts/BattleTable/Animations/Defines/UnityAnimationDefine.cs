using Game.BattleTable;
using UnityEngine;

namespace Game.Animations
{
    /// <summary>
    /// 动画定义
    /// </summary>
    public class UnityAnimationDefine : AnimationDefine
    {
        public UnityAnimationDefine(long cardPoolId, int id, string name) : base(cardPoolId, id, name)
        {

        }
        public UnityAnimationDefine(int id, string name) : this(0, id, name)
        {

        }
        public override TableEffect createEffect(BattleTableManager table, Transform parent)
        {
            if (!prefab)
                return null;
            return Object.Instantiate(prefab, parent, false);
        }
        public TableEffect prefab;

    }
}