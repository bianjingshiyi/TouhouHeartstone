using System.Linq;
using TouhouHeartstone;
using UI;

namespace Game
{
    class DeathAnimation : EventAnimation<THHCard.DeathEventArg>
    {
        public override bool update(TableManager table, THHCard.DeathEventArg eventArg)
        {
            foreach (var p in eventArg.infoDic)
            {
                if (table.tryGetServant(p.Key, out var servant))
                {
                    table.ui.SelfFieldList.removeItem(servant);
                    table.ui.EnemyFieldList.removeItem(servant);
                }
            }
            return true;
        }
    }
}
