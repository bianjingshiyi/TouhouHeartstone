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
        public override TableEffect createEffect(Transform parent)
        {
            if (!prefab)
                return null;
            Vector3 position = prefab.transform.position + Vector3.up * prefab.heightOffset;
            if (parent != null)
                position = parent.TransformPoint(position);
            Vector3 forward = prefab.transform.forward;
            if (parent != null)
                forward = parent.TransformDirection(forward);
            Quaternion rotation = Quaternion.Euler(forward);
            TableEffect effect = Object.Instantiate(prefab, position, rotation, parent);
            return effect;
        }
        public override ColorTargetGroup[] getColorTargetGroups()
        {
            return colorTargets;
        }
        public TableEffect prefab;
        public ColorTargetGroup[] colorTargets;
    }
}