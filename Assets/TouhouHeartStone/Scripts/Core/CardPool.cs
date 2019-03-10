using System;
using System.Collections.Generic;
using System.Collections;

namespace TouhouHeartstone
{
    [Serializable]
    public class CardPool : IEnumerable<CardDefine>
    {
        public CardPool()
        {
            dicCardDefine = new Dictionary<int, CardDefine>();
        }
        public CardPool(IEnumerable<CardDefine> cardDefines)
        {
            dicCardDefine = new Dictionary<int, CardDefine>();
            foreach (CardDefine define in cardDefines)
            {
                dicCardDefine.Add(define.id, define);
            }
        }
        public CardDefine this[int id]
        {
            get { return dicCardDefine.ContainsKey(id) ? dicCardDefine[id] : null; }
        }
        Dictionary<int, CardDefine> dicCardDefine = null;
        public IEnumerator<CardDefine> GetEnumerator()
        {
            return dicCardDefine.Values.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return dicCardDefine.Values.GetEnumerator();
        }
    }
}