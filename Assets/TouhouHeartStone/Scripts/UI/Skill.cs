using TouhouHeartstone;

namespace UI
{
    partial class Skill
    {
        public TouhouCardEngine.Card card { get; private set; } = null;
        public void update(TouhouCardEngine.Card card, CardSkinData skin)
        {
            this.card = card;

            Image.sprite = skin.image;
            CostText.text = card.getCost().ToString();
        }
    }
}
