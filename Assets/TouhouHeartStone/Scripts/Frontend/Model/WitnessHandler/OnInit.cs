using IGensoukyo.Utilities;
using System;

namespace TouhouHeartstone.Frontend.Model.Witness
{
    public class OnInit : WitnessHandler
    {
        public override string Name => "onInit";

        protected override bool witnessSuccessHandler(EventWitness witness, DeckController deck, GenericAction callback)
        {
            // 角色卡的DefineID
            int[] charactersDID = witness.getVar<int[]>("masterCardsDID");
            // 角色卡的RuntimeID
            int[] charactersRID = witness.getVar<int[]>("masterCardsRID");
            // 玩家卡组
            int[] userCards = witness.getVar<int[]>("deck");
            // 玩家顺序
            int[] playerOrder = witness.getVar<int[]>("sortedPlayersIndex");
            // 初始手牌DefineID
            int[] initHandCard = witness.getVar<int[]>("initCardsDID");
            // 初始手牌RuntimeID
            int[][] initCardsRID = witness.getVar<int[][]>("initCardsRID");

            DebugUtils.NullCheck(charactersDID, "charactersDID");
            DebugUtils.NullCheck(charactersRID, "charactersRID");
            DebugUtils.NullCheck(userCards, "userCards");
            DebugUtils.NullCheck(playerOrder, "playerOrder");
            DebugUtils.NullCheck(initHandCard, "initHandCard");
            DebugUtils.NullCheck(initCardsRID, "initCardsRID");

            if (charactersDID.Length != charactersRID.Length)
                throw new LengthNotMatchException(charactersDID.Length, charactersRID.Length);

            deck.SetPlayer(playerOrder, CardID.ToCardIDs(charactersDID, charactersRID));
            deck.SetInitHandcard(initHandCard, initCardsRID);

            // 先抽卡再设置deck，防止deck中的被抽走（
            deck.SetSelfDeck(userCards);

            return false;
        }
    }
}
