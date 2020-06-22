using TouhouHeartstone;
using UI;
namespace Game
{
    class StartAnimation : EventAnimation<THHGame.StartEventArg>
    {
        public override bool update(TableManager table, THHGame.StartEventArg eventArg)
        {
            foreach (THHPlayer player in table.game.players)
            {
                if (player == table.player)
                {
                    table.ui.SelfHandList.clearItems();
                    foreach (TouhouCardEngine.Card card in player.hand)
                    {
                        table.createHand(card);
                    }
                }
                else
                {
                    table.ui.EnemyHandList.clearItems();
                    foreach (TouhouCardEngine.Card card in player.hand)
                    {
                        table.createHand(card);
                    }
                }
            }
            return true;
        }
    }
}
