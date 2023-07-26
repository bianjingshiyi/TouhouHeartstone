using UnityEngine;

namespace Game.BattleTable
{
    public class PilePanelItem : MonoBehaviour
    {
        public void destroy()
        {
            Destroy(gameObject);
        }
        public TableCard card => _card;
        [SerializeField]
        private TableCard _card;
    }
}