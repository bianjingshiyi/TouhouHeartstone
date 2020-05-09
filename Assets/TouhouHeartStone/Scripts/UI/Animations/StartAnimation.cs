using TouhouHeartstone;
namespace UI
{
    class StartAnimation : Animation<THHGame.StartEventArg>
    {
        public StartAnimation(THHGame.StartEventArg eventArg) : base(eventArg)
        {
        }
        public override bool update(Table table)
        {
            foreach (THHPlayer player in table.game.players)
            {
                if (player == table.player)
                {
                    foreach (TouhouCardEngine.Card card in player.hand)
                    {
                        var item = table.SelfHandList.addItem();
                        item.Card.update(card, table.getSkin(card));
                    }
                }
                else
                {
                    foreach (TouhouCardEngine.Card card in player.hand)
                    {
                        var item = table.EnemyHandList.addItem();
                        item.Card.update(card, null);
                    }
                }
            }
            return true;
        }
    }
}
