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
            if (card.getAttack(null) > 0)
            {
                //AttackText.text = card.getAttack(null).ToString();
                AttackImage.gameObject.SetActive(true);
            }
            else
            {
                AttackImage.gameObject.SetActive(false);
            }
            //DurabilityText.text = card.getCurrentLife(null).ToString();
        }
        public SimpleAnim onEquip;
        public SimpleAnim onDestroy;
    }
}
