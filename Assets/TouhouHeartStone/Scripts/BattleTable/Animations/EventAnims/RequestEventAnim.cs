using System.Collections;
using System.Linq;
using ExcelLibrary.BinaryFileFormat;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;
namespace Game.BattleTable
{
    class RequestEventAnim : EventAnimation<RequestEventDefine>
    {
        public override IEnumerator startUpdate(BattleTableManager table)
        {
            IRequest request = eventArg.getVar<IRequest>(RequestEventDefine.VAR_REQUEST);
            if(request is InitReplaceRequest initReplace)
            {
                if (!initReplace.playersId.Contains(table.player.id))
                    yield break;

                table.showStartDialog(initReplace);
            }
            else if (request is DiscoverRequest discover)
            {
                bool forSelf = discover.playersId.Contains(table.player.id);
                if (!forSelf)
                {
                    table.setOpponentWaiting(true);
                }
                else
                {
                    if (discover.cardRefArray != null)
                        table.displayDiscoverDialog(discover.cardRefArray, discover.title);
                    else
                        table.displayDiscoverDialog(discover.cardIdArray, discover.title);
                }

                // 最短持续1秒，防止看回放的时候什么都没看就直接选了
                while (_time <= minDisplayTime || table.game.answers.getAllRequests().Contains(discover))
                {
                    yield return null;
                }

                if (!forSelf)
                {
                    table.setOpponentWaiting(false);
                }
                else
                {
                    table.closeDiscoverDialog();
                    table.setAllCardHighlightDirty();
                }
            }
        }
        public const float minDisplayTime = 1;
    }
}
