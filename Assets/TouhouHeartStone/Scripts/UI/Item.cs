using TouhouHeartstone;

namespace UI
{
    partial class Item
    {
        public TouhouCardEngine.Card card { get; private set; } = null;
        public void update(TouhouCardEngine.Card card, CardSkinData skin)
        {
            this.card = card;

            Image.sprite = skin.image;
            if (card.getAttack() > 0)
            {
                AttackText.text = card.getAttack().ToString();
                AttackImage.gameObject.SetActive(true);
            }
            else
            {
                AttackImage.gameObject.SetActive(false);
            }
            DurabilityText.text = card.getCurrentLife().ToString();
        }
    }
}
