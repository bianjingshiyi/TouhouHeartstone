using TouhouHeartstone;

namespace UI
{
    partial class Skill
    {
        public TouhouCardEngine.Card card { get; private set; } = null;
        public void update(THHGame game, THHPlayer self, THHPlayer player, TouhouCardEngine.Card card, CardSkinData skin)
        {
            this.card = card;

            Image.sprite = skin.image;
            CostText.text = card.getCost().ToString();
            if (player == self && card.isUsable(game, player, out _))
            {
                IsUsableController = IsUsable.True;
            }
            else
            {
                IsUsableController = IsUsable.False;
            }
            if (card.isUsed())
            {
                IsUsedController = IsUsed.True;
            }
            else
                IsUsedController = IsUsed.False;
        }
    }
}
