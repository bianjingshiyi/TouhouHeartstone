using UnityEngine;

namespace Game.BattleTable
{
    public class MulliganCardItem : CardUIItem
    {
        public void setMarkVisible(bool visible)
        {
            _mark.SetActive(visible);
        }
        [SerializeField]
        private GameObject _mark;
    }
}