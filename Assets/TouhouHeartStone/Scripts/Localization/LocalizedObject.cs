using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Game
{
    public abstract class LocalizedObject<T>
    {
        public LocalizedObject(T value)
        {
            this.value = value;
            dictionary = new Dictionary<string, T>();
        }
        public LocalizedObject(LocalizedObject<T> other)
        {
            this.value = other.value;
            dictionary = new Dictionary<string, T>();
            foreach (var pair in other.dictionary)
            {
                dictionary.Add(pair.Key, pair.Value);
            }
        }
        public void setTranslation(string language, T translation)
        {
            dictionary[language] = translation;
        }
        public bool tryGetTranslation(string language, out T translation)
        {
            return dictionary.TryGetValue(language, out translation);
        }
        public T getTranslation(string language)
        {
            if (tryGetTranslation(language, out T translation))
                return translation;
            return default;
        }
        public bool containsTranslation(string language)
        {
            return dictionary.ContainsKey(language);
        }
        public T localize(string language)
        {
            if (tryGetTranslation(language, out var translation))
            {
                return translation;
            }
            return value;
        }
        public T[] getAllTranslations(bool includePrimary = false)
        {
            if (includePrimary)
                return dictionary.Values.Prepend(value).ToArray();
            return dictionary.Values.ToArray();
        }
        public string[] getAllLanguages()
        {
            return dictionary.Keys.ToArray();
        }
        public T value;
        protected Dictionary<string, T> dictionary;
    }
    public static class LocalizeHelper
    {
        public static T2 getLocalizedObject<T1, T2>(this IEnumerable<T2> set, T1 key) where T2: LocalizedObject<T1>
        {
            if (set == null)
                return null;
            var obj = set.FirstOrDefault(s => s.value.Equals(key));
            return obj;
        }
        public static LocalizedObject<T> getLocalizedObject<T>(this IEnumerable<LocalizedObject<T>> set, T key)
        {
            return set.getLocalizedObject<T, LocalizedObject<T>>(key);
        }
        public static T localize<T>(this IEnumerable<LocalizedObject<T>> set, T key, string language)
        {
            var localizedString = set.getLocalizedObject(key);
            if (localizedString != null)
                return localizedString.localize(language);
            return key;
        }
        public static T[] getAllLocalizations<T>(this LocalizedObject<T> obj)
        {
            var languages = obj.getAllLanguages();
            return languages.Select(l => obj.getTranslation(l)).Where(e => e != null).Prepend(obj.value).ToArray();
        }
    }
}
