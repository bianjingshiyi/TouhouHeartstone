namespace TouhouHeartstone.OldFrontend.WitnessHandler
{
    public class WitnessHandlerSetHand : WitnessHandlerBase<SetHandWitness>
    {
        public override bool HasAnimation => false;

        public override void Exec(SetHandWitness witness)
        {
            DebugUtils.LogNoImpl($"设置玩家{witness.playerId}手牌为{witness.cards}");
        }
    }
}
