using System;
using System.Collections.Generic;
namespace TouhouCardEngine
{
    [Serializable]
    public class CardReferenceCollection : List<DefineReference>, ICloneable
    {
        public CardReferenceCollection() : base()
        {
        }
        public CardReferenceCollection(IEnumerable<DefineReference> defRefs) : base(defRefs)
        {
        }
        public object Clone()
        {
            return new CardReferenceCollection(this);
        }
    }
}