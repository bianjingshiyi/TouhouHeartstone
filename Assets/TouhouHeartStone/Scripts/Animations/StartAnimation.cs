using TouhouHeartstone;
using UI;
namespace Game
{
    class StartAnimation : EventAnimation<THHGame.StartEventArg>
    {
        public override bool update(TableManager table, THHGame.StartEventArg eventArg)
        {
            foreach (var pair in eventArg.startHandDic)
            {
                if (pair.Key == table.player)
                {
                    table.ui.SelfHandList.clearItems();
                    foreach (TouhouCardEngine.Card card in pair.Value.hands)
                    {
                        table.createHand(card);
                    }
                }
                else
                {
                    table.ui.EnemyHandList.clearItems();
                    foreach (TouhouCardEngine.Card card in pair.Value.hands)
                    {
                        table.createHand(card);
                    }
                }
            }
            return true;
        }
    }
}
