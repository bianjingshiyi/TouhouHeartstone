using System.Collections;
using System.Linq;
using TouhouHeartstone;
namespace Game.BattleTable
{
    class InitReplaceRequestAnimation : RequestAnimation<InitReplaceRequest>
    {
        public override IEnumerator display(BattleTableManager table, InitReplaceRequest request)
        {
            if (!request.playersId.Contains(table.player.id))
                yield break;

            table.showStartDialog(request);
        }
    }
}
