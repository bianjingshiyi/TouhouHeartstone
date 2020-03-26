namespace UI
{
    partial class Servant
    {
        public TouhouCardEngine.Card card { get; private set; }
        public void update(TouhouCardEngine.Card card, CardSkinData skin)
        {
            this.card = card;

            if (skin != null)
            {
                Image.sprite = skin.image;
            }
        }
    }
}
