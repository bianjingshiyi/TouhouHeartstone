using System;
using System.Linq;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    public abstract class Buff
    {
        internal protected abstract void onAdded(Card card);
        internal protected abstract void onRemoved(Card card);
    }
    public class GeneratedBuff : Buff
    {
        public PropertyModifier[] modifiers { get; }
        public GeneratedBuff(params PropertyModifier[] modifiers)
        {
            this.modifiers = modifiers;
        }
        protected internal override void onAdded(Card card)
        {
            foreach (var modifier in modifiers)
            {
                if (modifier.propName == "life" && modifier.changeType == PropertyChangeType.add)
                    card.setProp("life", PropertyChangeType.add, (int)modifier.value);
            }
        }
        protected internal override void onRemoved(Card card)
        {
        }
    }
    [Serializable]
    public class Card
    {
        public int id { get; internal set; } = 0;
        public Pile pile { get; internal set; } = null;
        public CardDefine define { get; }
        List<Buff> buffList { get; } = new List<Buff>();
        public Buff[] buffs
        {
            get { return buffList.ToArray(); }
        }
        internal Dictionary<string, object> propDic { get; } = new Dictionary<string, object>();
        public Card(CardEngine game, CardDefine define)
        {
            if (define != null)
                this.define = define;
            else
                throw new ArgumentNullException(nameof(define));
        }
        public void addBuff(Buff buff)
        {
            if (buff == null)
                throw new ArgumentNullException(nameof(buff));
            buffList.Add(buff);
            buff.onAdded(this);
        }
        public bool removeBuff(Buff buff)
        {
            return buffList.Remove(buff);
        }
        public void setProp(string propName, PropertyChangeType changeType, string value)
        {
            if (changeType == PropertyChangeType.set)
                propDic[propName] = value;
            else
                propDic[propName] = getProp<string>(propName) + value;
        }
        public void setProp(string propName, PropertyChangeType changeType, float value)
        {
            if (changeType == PropertyChangeType.set)
                propDic[propName] = value;
            else
                propDic[propName] = getProp<float>(propName) + value;
        }
        public void setProp(string propName, PropertyChangeType changeType, int value)
        {
            if (changeType == PropertyChangeType.set)
                propDic[propName] = value;
            else
                propDic[propName] = getProp<int>(propName) + value;
        }
        public void setProp<T>(string propName, T value)
        {
            propDic[propName] = value;
        }
        public T getProp<T>(string propName)
        {
            //整数属性
            if (typeof(T) == typeof(int))
            {
                //基础值
                int value = 0;
                if (propDic.ContainsKey(propName) && propDic[propName] is int)
                    value = (int)propDic[propName];
                //加值，乘值
                foreach (GeneratedBuff buff in buffList)
                {
                    foreach (PropertyModifier modifier in buff.modifiers)
                    {
                        if (modifier.propName == propName)
                        {
                            if (modifier.changeType == PropertyChangeType.add)
                            {
                                if (modifier.value is int || modifier.value is float)
                                    value += (int)modifier.value;
                                else
                                    throw new InvalidCastException("卡片属性" + propName + "的类型为" + propDic[propName].GetType().Name + "，属性修正器的类型为" + modifier.value.GetType().Name + "，无法相互转化");
                            }
                            else if (modifier.changeType == PropertyChangeType.set)
                            {
                                if (modifier.value is int || modifier.value is float)
                                    value = (int)modifier.value;
                                else
                                    throw new InvalidCastException("卡片属性" + propName + "的类型为" + propDic[propName].GetType().Name + "，属性修正器的类型为" + modifier.value.GetType().Name + "，无法相互转化");
                            }
                        }
                    }
                }
                return (T)(object)value;
            }//暂时只支持整数属性的复杂运算
            else
            {
                if (propDic.ContainsKey(propName) && propDic[propName] is T)
                    return (T)propDic[propName];
                else
                    return default;
            }
        }
        public override string ToString()
        {
            return "Card(" + id + ")<" + define.id + ">";
        }
        public static implicit operator Card[] (Card card)
        {
            if (card != null)
                return new Card[] { card };
            else
                return new Card[0];
        }
    }
}