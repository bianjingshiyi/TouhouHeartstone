using System.Collections;
using System.Linq;
using TouhouHeartstone;
namespace Game.BattleTable
{
    class DiscoverRequestAnim : RequestAnimation<DiscoverRequest>
    {
        public const float minDisplayTime = 1;
        public override IEnumerator display(BattleTableManager table, DiscoverRequest request)
        {
            if (request.cardRefArray != null)
                table.displayDiscoverDialog(request.cardRefArray, request.title);
            else
                table.displayDiscoverDialog(request.cardIdArray, request.title);
            // 最短持续1秒，防止看回放的时候什么都没看就直接选了
            while (_duration <= minDisplayTime || table.game.answers.getAllRequests().Contains(request))
            {
                yield return null;
            }
            table.closeDiscoverDialog();
            table.setAllCardHighlightDirty();
        }
    }
}
