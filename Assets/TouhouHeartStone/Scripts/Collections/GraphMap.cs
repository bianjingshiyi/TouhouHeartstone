using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class GraphMap
    {
        public void SetMap(object key, object value)
        {
            dictionary[key] = value;
        }
        public void RemoveMap(object key)
        {
            dictionary.Remove(key);
        }
        public bool HasMap(object key)
        {
            return dictionary.ContainsKey(key);
        }
        public object GetMapValue(object key)
        {
            if (dictionary.TryGetValue(key, out var value))
                return value;
            return null;
        }
        private Dictionary<object, object> dictionary = new Dictionary<object, object>();
    }
}
