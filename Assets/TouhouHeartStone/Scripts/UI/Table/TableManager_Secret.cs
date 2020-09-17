using UI;
using System.Collections.Generic;
using Card = TouhouCardEngine.Card;
namespace Game
{
    partial class TableManager
    {
        Dictionary<Card, Secret> cardSecretDic { get; } = new Dictionary<Card, Secret>();
        public Secret createSecret(Card card)
        {
            Secret secret;
            if (card.pile == player.head)
                secret = ui.SelfMaster.Head.addSecret();
            else
                secret = ui.EnemyMaster.Head.addSecret();
            cardSecretDic.Add(card, secret);
            return secret;
        }
        public bool tryGetSecret(Card card, out Secret secret)
        {
            return cardSecretDic.TryGetValue(card, out secret);
        }
        public bool removeSecret(Card card)
        {
            if (cardSecretDic.TryGetValue(card, out var secret))
            {
                cardSecretDic.Remove(card);
                Destroy(secret.gameObject);
                return true;
            }
            return false;
        }
    }
}
