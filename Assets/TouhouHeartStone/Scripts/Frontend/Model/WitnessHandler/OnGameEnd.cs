using TouhouCardEngine;

namespace TouhouHeartstone.Frontend.Model.Witness
{
    /// <summary>
    /// 游戏结束
    /// </summary>
    public class OnGameEnd : WitnessHandler
    {
        public override string Name => "onGameEnd";

        protected override bool witnessSuccessHandler(EventWitness witness, DeckController deck, GenericAction callback = null)
        {
            var wit = Utilities.CheckType<GameEndWitness>(witness);

            bool win = false;
            for (int i = 0; i < wit.winnerPlayersIndex.Length; i++)
            {
                if (wit.winnerPlayersIndex[i] == deck.selfID)
                {
                    win = true;
                    break;
                }
            }
            deck.CommonDeck.GameEnd(win, callback);
            return true;
        }
    }
}
