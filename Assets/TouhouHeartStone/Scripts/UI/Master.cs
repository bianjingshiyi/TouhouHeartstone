using UnityEngine;
using TouhouHeartstone;

namespace UI
{
    partial class Master
    {
        public TouhouCardEngine.Card card { get; private set; } = null;
        public void update(TouhouCardEngine.Card card, CardSkinData skin)
        {
            this.card = card;

            Image.sprite = skin.image;
            HpText.text = card.getCurrentLife().ToString();
            if (card.getCurrentLife() == card.getLife())
                HpText.color = Color.white;
            else
                HpText.color = Color.red;
            if (card.getAttack() > 0)
            {
                AttackText.text = card.getAttack().ToString();
                AttackText.enabled = true;
            }
            else
                AttackText.enabled = false;
            if (card.getArmor() > 0)
            {
                ArmorText.text = card.getArmor().ToString();
                ArmorText.enabled = true;
            }
            else
                ArmorText.enabled = false;
        }
    }
}
