using TouhouCardEngine;

namespace TouhouHeartstone
{
    public class GeneratedBattlecry : GeneratedEffect
    {
        #region 公有方法
        /// <summary>
        /// 构造一个战吼效果
        /// </summary>
        public GeneratedBattlecry(ActionValueRef condition, TargetChecker[] targets, ActionNode action) : base(new string[0], null, null,
            new TriggerGraph[] { new TriggerGraph("ActiveEvent", condition, targets, action) }, new string[] { KEYWORD.BATTLECRY })
        {
        }
        /// <summary>
        /// 构造一个空的战吼效果
        /// </summary>
        public GeneratedBattlecry() : this(new ActionValueRef(), new TargetChecker[0], null)
        {
        }
        public override object getProp(string name)
        {
            if (name == nameof(activeTrigger))
                return activeTrigger;
            return base.getProp(name);
        }
        public override void setProp(string name, object value)
        {
            if (name == nameof(activeTrigger))
            {
                activeTrigger = value as TriggerGraph;
                return;
            }
            base.setProp(name, value);
        }
        public override EffectPropertyInfo[] getPropInfos()
        {
            return propertyInfos;
        }
        #endregion
        #region 属性字段
        public TriggerGraph activeTrigger
        {
            get { return triggerList[0]; }
            set { triggerList[0] = value; }
        }
        static EffectPropertyInfo[] propertyInfos = new EffectPropertyInfo[]
        {
            new EffectPropertyInfo(typeof(TriggerGraph), nameof(activeTrigger))
        };
        #endregion
    }
}