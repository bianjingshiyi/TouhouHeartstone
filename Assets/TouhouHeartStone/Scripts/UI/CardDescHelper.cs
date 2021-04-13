using TouhouHeartstone;
using System.Text.RegularExpressions;
using TouhouCardEngine;

namespace UI
{
    public static class CardDescHelper
    {
        public static string replace(string desc, CardEngine game, Player player, TouhouCardEngine.Card card)
        {
            string result = Regex.Replace(desc, @"{(?<obj>\w+):(?<name>.+)}", m =>
            {
                string obj = m.Groups["obj"].Value;
                string name = m.Groups["name"].Value;
                if (obj == "card")
                    return card.getProp(game, name).ToString();
                else
                    return "???";
            });
            return result;
        }
    }
}
