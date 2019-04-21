
using UnityEngine;

namespace TouhouHeartstone.Frontend.Model
{
    public class DeckModel : MonoBehaviour
    {
        [SerializeField]
        DeckController deskController;

        GameModel gm;

        private void Awake()
        {
            gm = GetComponentInParent<GameModel>();
        }

        private void Start()
        {
            gm.Game.addPlayer(deskController, new int[] { 1000, 1, 1, 1, 1, 1, 1, 1 });
            deskController.selfID = 0;

            gm.Game.init();
        }

        public void InitReplace(int uid, int[] cards)
        {
            gm.Game.initReplace(uid, cards);
        }

        public void Roundend(int playerID)
        {
            gm.Game.turnEnd(playerID);
        }
    }
}
