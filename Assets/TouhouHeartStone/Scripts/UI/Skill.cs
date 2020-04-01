using TouhouHeartstone;

namespace UI
{
    partial class Skill
    {
        public TouhouCardEngine.Card card { get; private set; } = null;
        partial void onAwake()
        {
            asButton.onClick.AddListener(() =>
            {
                Table table = GetComponentInParent<Table>();
                table.player.cmdUse(table.game, card, 0);
            });
        }
        public void update(TouhouCardEngine.Card card, CardSkinData skin)
        {
            this.card = card;

            Image.sprite = skin.image;
            CostText.text = card.getCost().ToString();
        }
    }
}
