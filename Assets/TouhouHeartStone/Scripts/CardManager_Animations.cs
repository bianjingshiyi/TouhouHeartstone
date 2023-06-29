using System.Collections.Generic;
using System.Linq;
using Game.Animations;
using TouhouCardEngine;
using TouhouCardEngine.Shared;
using UnityEngine;

namespace Game
{
    partial class CardManager
    {
        /// <summary>
        /// 从给出的路径中加载卡片和皮肤，支持通配符，比如“Cards/*.xls”
        /// </summary>
        /// <param name="excelPaths"></param>
        /// <returns></returns>
        public void loadBuiltinAnimations()
        {
            // TODO: 需要对内置的一些素材库进行动态加载。
            builtinAnimationDic.Add(BuiltinAnimationID.SUMMER_FIRE, new UnityAnimationDefine(0, BuiltinAnimationID.SUMMER_FIRE, "SummerFire")
            {
                prefabPath = "res:Prefabs/Projectiles/SummerFire",
                duration = 2f,
                blockTime = 1.5f
            });
        }
        public AnimationDefine getAnimationDefine(ICardPoolReference poolRef, int id)
        {
            if (poolRef.CardPoolID == 0)
            {
                if (builtinAnimationDic.TryGetValue(id, out AnimationDefine define))
                {
                    return define;
                }
            }
            if (_cardPoolDict.TryGetValue(poolRef, out var pool))
            {
                return pool.animationDefineList.FirstOrDefault(a => a.id == id);
            }
            return null;
        }
        public AnimationDefine[] getBuiltinAnimationDefines()
        {
            return builtinAnimationDic.Values.ToArray();
        }
        Dictionary<int, AnimationDefine> builtinAnimationDic { get; set; } = new Dictionary<int, AnimationDefine>();
    }
}
