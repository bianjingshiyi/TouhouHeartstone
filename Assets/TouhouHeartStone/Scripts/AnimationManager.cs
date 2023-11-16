using System.Collections.Generic;
using System.Linq;
using BJSYGameCore;
using Game.Animations;
using TouhouCardEngine;
using TouhouCardEngine.Shared;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// 用于管理卡牌的特效等内容。
    /// </summary>
    public class AnimationManager : Manager
    {
        #region 公有方法
        public IEnumerable<string> getBuiltinEffectNames()
        {
            return builtinEffectList.Select(e => e.defineName).ToArray();
        }
        /// <summary>
        /// 从给出的路径中加载卡片和皮肤，支持通配符，比如“Cards/*.xls”
        /// </summary>
        /// <param name="excelPaths"></param>
        /// <returns></returns>
        public void loadBuiltinAnimations()
        {
            for (int i = 0; i < builtinEffectList.Length; i++)
            {
                var effect = builtinEffectList[i];
                if (effect == null)
                    continue;
                var define = effect.toAnimationDefine();
                define.id = i + 1;
                builtinAnimationDic.Add(define.id, define);
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
        public AnimationDefine[] getAllAnimationDefines()
        {
            return getBuiltinAnimationDefines();
        }
        public bool canActionAddEffect(ActionReference actionRef)
        {
            return main.card.getActionBindedEvents(actionRef) != null;
        }
        #endregion

        #region 属性字段
        public MainManager main => getManager<MainManager>();
        Dictionary<int, AnimationDefine> builtinAnimationDic { get; set; } = new Dictionary<int, AnimationDefine>();
        [SerializeField]
        private BuiltinEffect[] builtinEffectList;
        #endregion
    }
}
