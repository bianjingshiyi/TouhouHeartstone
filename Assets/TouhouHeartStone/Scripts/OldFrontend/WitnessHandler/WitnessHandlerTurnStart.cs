using TouhouHeartstone.OldFrontend.Manager;
using System;
using System.Runtime.InteropServices;

namespace TouhouHeartstone.OldFrontend.WitnessHandler
{
    [ComVisible(false)]
    public class WitnessHandlerTurnStart : WitnessHandlerBase<TurnStartWitness>
    {
        public override bool HasAnimation => false;

        public override void Exec(TurnStartWitness witness)
        {
            if (witness.playerId == frontend.PlayerID)
            {
                frontend.GetSubManager<FrontendUIManager>().RoundStart();
                var stoneBar = frontend.GetSubManager<FrontendUIManager>().StoneBar;
                stoneBar.CurrentStone = stoneBar.CurrentStone;
                DebugUtils.LogDebug("你的回合开始");
            }
            else
            {
                DebugUtils.LogNoImpl($"玩家{witness.playerId}回合开始");
            }
        }
    }
}
