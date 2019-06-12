using UnityEngine;
using System.Linq;

namespace TouhouHeartstone.Frontend
{
    [CreateAssetMenu(menuName = "IGSK/CardImageResLibrary")]
    public class CardImageResources : ScriptableObject
    {
        string fallbackLang = "en-US";
        string fallbackID = "invalid";

        [SerializeField]
        CardImageResource[] cardTexts = new CardImageResource[0];

        public CardImageResource Get(string id, string lang)
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
