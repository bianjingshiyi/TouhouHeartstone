using System;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    [Serializable]
    public class Card
    {
        public Card(CardEngine game, CardDefine define)
        {
            if (define != null)
                this.define = define;
            else
                throw new ArgumentNullException(nameof(define));
        }
        public Pile pile { get; internal set; } = null;
        public void setProp(string propName, PropertyChangeType changeType, string value)
        {
            if (changeType == PropertyChangeType.set)
                dicProp[propName] = value;
            else
                dicProp[propName] = getProp<string>(propName) + value;
        }
        public void setProp(string propName, PropertyChangeType changeType, float value)
        {
            if (changeType == PropertyChangeType.set)
                dicProp[propName] = value;
            else
                dicProp[propName] = getProp<float>(propName) + value;
        }
        public void setProp(string propName, PropertyChangeType changeType, int value)
        {
            if (changeType == PropertyChangeType.set)
                dicProp[propName] = value;
            else
                dicProp[propName] = getProp<int>(propName) + value;
        }
        public void setProp<T>(string propName, T value)
        {
            dicProp[propName] = value;
        }
        public T getProp<T>(string propName)
        {
            if (dicProp.ContainsKey(propName) && dicProp[propName] is T)
                return (T)dicProp[propName];
            else
                return default(T);
        }
        internal Dictionary<string, object> dicProp { get; } = new Dictionary<string, object>();
        public CardDefine define { get; }
        public override string ToString()
        {
            return "Card(" + this.getRID() + ")<" + define.id + ">";
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