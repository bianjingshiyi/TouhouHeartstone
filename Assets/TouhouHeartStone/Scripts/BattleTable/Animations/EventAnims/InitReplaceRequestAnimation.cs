using System.Collections;
using TouhouHeartstone;
namespace Game.BattleTable
{
    class InitReplaceRequestAnimation : RequestAnimation<InitReplaceRequest>
    {
        public override IEnumerator display(BattleTableManager table, InitReplaceRequest request)
        {
            table.showInitReplaceDialog(request);
            yield break;
        }
    }
}
