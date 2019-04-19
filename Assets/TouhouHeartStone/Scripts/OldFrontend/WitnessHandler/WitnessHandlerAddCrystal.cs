using TouhouHeartstone.OldFrontend.Manager;
using IGensoukyo.Utilities;

namespace TouhouHeartstone.OldFrontend.WitnessHandler
{
    public class WitnessHandlerAddCrystal : WitnessHandlerBase<AddCrystalWitness>
    {
        public override bool HasAnimation => false;

        public override void Exec(AddCrystalWitness witness)
        {
            if (witness.playerId == frontend.PlayerID)
            {
                var stoneBar = frontend.GetSubManager<FrontendUIManager>().StoneBar;
                stoneBar.MaxStone += witness.count;
            }
            else
            {
                DebugUtils.LogNoImpl($"非本地玩家{witness.playerId}增加{witness.state}水晶{witness.count}个");
            }
        }
    }
}
