using System.Collections.Generic;
using System.Linq;
using BJSYGameCore;
using Game.Animations;
using TouhouCardEngine.Shared;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// 用于管理卡牌的特效等内容。
    /// </summary>
    public class AnimationManager : Manager
    {
        /// <summary>
        /// 从给出的路径中加载卡片和皮肤，支持通配符，比如“Cards/*.xls”
        /// </summary>
        /// <param name="excelPaths"></param>
        /// <returns></returns>
        public void loadBuiltinAnimations()
        {
            foreach (var effect in builtinEffectList)
            {
                var define = effect.toAnimationDefine();
                builtinAnimationDic.Add(effect.id, define);
            }
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
            if (main.card.tryGetCardPool(poolRef, out var pool))
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
        public MainManager main => getManager<MainManager>();
        [SerializeField]
        private BuiltinEffect[] builtinEffectList;
    }
}
