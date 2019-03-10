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
        public static Card create(CardInstance instance)
        {
            return new LuckyCoin(instance);
        }
        public Card(CardEngine game, CardDefine define)
        {
            id = game.registerCard(this);
            if (define != null)
                this.define = define;
            else
                throw new ArgumentNullException(nameof(define));
        }
        public Card(CardInstance instance)
        {
            this.instance = instance;
        }
        public int id { get; } = -1;
        public CardDefine define { get; }
        public void use(CardEngine game, int position, Card target)
        {
        }
        public CardInstance instance { get; protected set; }
        public override string ToString()
        {
            return instance.ToString();
        }
        public static implicit operator Card[] (Card card)
        {
            if (card != null)
                return new Card[] { card };
            else
                return new Card[0];
        }
    }
    class LuckyCoin : Card
    {
        public LuckyCoin(CardInstance instance) : base(instance)
        {
        }
    }
}