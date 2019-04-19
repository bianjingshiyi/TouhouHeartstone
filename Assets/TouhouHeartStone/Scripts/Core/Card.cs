using System;
using System.Collections.Generic;

namespace TouhouHeartstone
{
    public abstract class CardDefine
    {
        public abstract int id { get; }
        public object this[string propName]
        {
            get { return getProp<object>(propName); }
        }
        public virtual T getProp<T>(string propName)
        {
            if (propName == nameof(id))
                return (T)(object)id;
            else
                return default(T);
        }
    }
    public class Coin : CardDefine
    {
        public override int id
        {
            get { return 1; }
        }
    }
    [Serializable]
    public class Card
    {
        public Card(CardEngine game, CardDefine define)
        {
            id = game.registerCard(this);
            if (define != null)
                this.define = define;
            else
                throw new ArgumentNullException(nameof(define));
        }
        public Pile pile { get; set; } = null;
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
                return default;
        }
        internal Dictionary<string, object> dicProp { get; } = new Dictionary<string, object>();
        public int id { get; } = -1;
        public CardDefine define { get; }
        public CardInstance instance { get; protected set; }
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