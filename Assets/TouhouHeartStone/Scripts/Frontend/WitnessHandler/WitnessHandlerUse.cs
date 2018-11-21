using System.Runtime.InteropServices;

[assembly: ComVisible(false)]
namespace TouhouHeartstone.Frontend.WitnessHandler
{
    public class WitnessHandlerUse : WitnessHandlerBase<UseWitness>
    {
        public override bool HasAnimation => false;

        public override void Exec(UseWitness witness)
        {
            DebugUtils.LogNoImpl($"玩家{witness.playerId}使用了卡{witness.card}，其作用对象是{witness.target}，位置是{witness.position}");
        }
    }
}
