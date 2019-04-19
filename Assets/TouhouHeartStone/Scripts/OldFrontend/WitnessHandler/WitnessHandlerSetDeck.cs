using IGensoukyo.Utilities;
namespace TouhouHeartstone.OldFrontend.WitnessHandler
{
    /// <summary>
    /// 设置玩家卡组大小
    /// </summary>
    public class WitnessHandlerSetDeck : WitnessHandlerBase<SetDeckWitness>
    {
        public override bool HasAnimation => false;

        public override void Exec(SetDeckWitness witness)
        {
            DebugUtils.Log($"[尚未实现功能]玩家{witness.playerId}的卡组大小设置为: {witness.number}");
        }
    }
}
