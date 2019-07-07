using UnityEngine;
using System.Linq;

namespace TouhouHeartstone.Frontend
{
    [CreateAssetMenu(menuName = "IGSK/CardTextResLibrary")]
    [System.Obsolete("使用CardAppearResouces替代")]
    public class CardTextResources : ScriptableObject
    {
        string fallbackLang = "en-US";
        string fallbackID = "invalid";

        [SerializeField]
        CardTextResource[] cardTexts = new CardTextResource[0];

        public CardTextResource Get(string id, string lang)
        {
            var cards = cardTexts.Where(e => e.ID == id);
            if (cards.Count() == 0)
            {
                if (id != fallbackID) return Get(fallbackID, lang);
                else throw new ResourcesIDNotFoundException();
            }

            var langSpec = cards.Where(e => e.Lang == lang);
            if (langSpec.Count() != 0) return langSpec.First();

            langSpec = cards.Where(e => e.Lang == fallbackLang);
            if (langSpec.Count() != 0) return langSpec.First();

            throw new I18nLangNotFoundException();
        }
    }
}
