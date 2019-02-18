using UnityEngine;
using System.Linq;

namespace TouhouHeartstone.Frontend
{
    [CreateAssetMenu(menuName = "IGSK/CardTextResLibrary")]
    public class CardTextResources : ScriptableObject
    {
        string fallbackLang = "en-US";

        [SerializeField]
        CardTextResource[] cardTexts;

        public CardTextResource Get(string id, string lang)
        {
            var cards = cardTexts.Where(e => e.ID == id);
            if (cards.Count() == 0) throw new ResourcesIDNotFoundException(id);

            var langSpec = cards.Where(e => e.Lang == lang);
            if (langSpec.Count() != 0) return langSpec.First();

            langSpec = cards.Where(e => e.Lang == fallbackLang);
            if (langSpec.Count() != 0) return langSpec.First();

            throw new I18nLangNotFoundException();
        }
    }
}
