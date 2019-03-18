using UnityEngine;
using System.Linq;

namespace TouhouHeartstone.Frontend
{
    [CreateAssetMenu(menuName = "IGSK/CardImageResLibrary")]
    public class CardImageResources : ScriptableObject
    {
        string fallbackLang = "en-US";

        [SerializeField]
        CardImageResource[] cardTexts;

        public CardImageResource Get(string id, string lang)
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
