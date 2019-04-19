using TouhouHeartstone.OldFrontend.Manager;
using IGensoukyo.Utilities;

namespace TouhouHeartstone.OldFrontend.WitnessHandler
{
    public class WitnessHandlerRemoveCrystal : WitnessHandlerBase<RemoveCrystalWitness>
    {
        public override bool HasAnimation => false;

        public override void Exec(RemoveCrystalWitness witness)
        {
            if (witness.playerId == frontend.PlayerID)
            {
                var stoneBar = frontend.GetSubManager<FrontendUIManager>().StoneBar;
                stoneBar.CurrentStone -= witness.count;
            }
            else
            {
                DebugUtils.LogNoImpl($"非本地玩家{witness.playerId}减少水晶{witness.count}个");
            }
        }
    }
}
