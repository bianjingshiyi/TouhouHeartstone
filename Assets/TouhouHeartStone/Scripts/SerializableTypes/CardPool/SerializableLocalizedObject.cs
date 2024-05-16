using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

namespace Game
{
    [Serializable]
    [MessagePackObject]
    [Union(0, typeof(SerializableLocalizedString))]
    [Union(1, typeof(SerializableLocalizedResource))]
    public abstract class SerializableLocalizedObject<T>
    {
        public SerializableLocalizedObject()
        {
        }
        public SerializableLocalizedObject(LocalizedObject<T> obj)
        {
            this.value = obj.value;
            var pairs = obj.getAllLanguages()
                .Select(l => (l, obj.getTranslation(l)))
                .Where(p => p.Item2 != null);
            if (pairs.Count() > 0)
            {
                dictionary = pairs.ToDictionary(p => p.Item1, p => p.Item2);
            }
        }
        protected void applyDeserialize(LocalizedObject<T> obj)
        {
            if (dictionary != null)
            {
                foreach (var pair in dictionary)
                {
                    if (pair.Value == null)
                        continue;
                    obj.setTranslation(pair.Key, pair.Value);
                }
            }
        }
        [Key(0)]
        public T value;
        [Key(1)]
        public Dictionary<string, T> dictionary;
    }
}
