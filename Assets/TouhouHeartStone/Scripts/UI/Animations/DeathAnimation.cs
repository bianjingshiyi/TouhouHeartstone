using System.Linq;
using TouhouHeartstone;

namespace UI
{
    class DeathAnimation : UIAnimation<THHCard.DeathEventArg>
    {
        public DeathAnimation(THHCard.DeathEventArg eventArg) : base(eventArg)
        {
        }
        public override bool update(Table table)
        {
            foreach (var p in eventArg.infoDic)
            {
                if (p.Value.player == table.player)
                {
                    Servant servant = table.SelfFieldList.FirstOrDefault(s => s.card == p.Key);
                    if (servant != null)
                    {
                        table.SelfFieldList.removeItem(servant);
                    }
                }
                else
                {
                    Servant servant = table.EnemyFieldList.FirstOrDefault(s => s.card == p.Key);
                    if (servant != null)
                    {
                        table.EnemyFieldList.removeItem(servant);
                    }
                }
            }
            return true;
        }
    }
}
