using System;
using System.Collections.Generic;
namespace TouhouHeartstone
{
    [Serializable]
    public class CardTagCollection : List<string>, ICloneable
    {
        public CardTagCollection() : base()
        {
        }
        public CardTagCollection(IEnumerable<string> keywords) : base(keywords)
        {
        }
        public object Clone()
        {
            return new CardTagCollection(this);
        }
    }
}