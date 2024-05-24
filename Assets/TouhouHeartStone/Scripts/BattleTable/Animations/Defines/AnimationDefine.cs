using System.Collections.Generic;
using System.Linq;
using Game.BattleTable;
using TouhouCardEngine;
using UnityEngine;

namespace Game.Animations
{
    /// <summary>
    /// 动画定义
    /// </summary>
    public abstract class AnimationDefine
    {
        #region 公有方法
        public AnimationDefine(long cardPoolId, int id, string name)
        {
            this.cardPoolId = cardPoolId;
            _id = id;
            _name = name;
            blockTime = -1;
        }
        /// <summary>
        /// 供序列化使用的默认构造器
        /// </summary>
        public AnimationDefine() : this(0, 0, string.Empty)
        {
        }
        public override bool Equals(object obj)
        {
            if (obj is AnimationDefine other)
                return cardPoolId == other.cardPoolId && id == other.id;
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            // 同DefineReference.GetHashCode()。
            int hashCode = 17;
            hashCode = hashCode * 31 + cardPoolId.GetHashCode();
            hashCode = hashCode * 31 + id.GetHashCode();
            return hashCode;
        }
        public abstract TableEffect createEffect(BattleTableManager table, Transform parent);
        public abstract ColorTargetGroup[] getColorTargetGroups();
        public virtual void setProp<T>(string propName, T value)
        {
            if (propName == nameof(id))
                id = (int)(object)value;
            _propDict[propName] = value;
        }
        public virtual T getProp<T>(string propName)
        {
            if (_propDict.ContainsKey(propName) && _propDict[propName] is T t)
                return t;
            else
                return default;
        }
        public virtual bool removeProp(string propName)
        {
            return _propDict.Remove(propName);
        }
        public virtual string[] getPropNames()
        {
            return _propDict.Keys.ToArray();
        }
        public virtual bool hasProp(string propName)
        {
            return _propDict.ContainsKey(propName);
        }
        public object this[string propName]
        {
            get { return getProp<object>(propName); }
        }
        public DefineReference getDefineRef()
        {
            return new DefineReference(cardPoolId, id);
        }
        #endregion

        #region 属性字段
        /// <summary>
        /// 动画定义ID，这个ID应该是独特的并用于区分不同的卡片。
        /// </summary>
        public virtual int id
        {
            get { return _id; }
            set { _id = value; }
        }
        int _id;
        public virtual string name
        {
            get { return _name; }
            set { _name = value; }
        }
        string _name;
        public long cardPoolId;
        public float duration;
        public float blockTime;
        public bool isPersistent;
        public AnimationType type;
        Dictionary<string, object> _propDict = new Dictionary<string, object>();
        #endregion
    }
    public enum AnimationType
    {
        Camera,
        Area,
        Card,
        Vector,
        Projectile
    }
}