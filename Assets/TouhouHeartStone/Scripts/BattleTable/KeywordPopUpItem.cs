using TMPro;
using UnityEngine;

namespace Game.BattleTable.UI
{
    public class KeywordPopUpItem : MonoBehaviour
    {
        #region 公有方法

        public void update(string title, string desc)
        {
            _title.text = title;
            _desc.text = desc;
        }

        #endregion

        #region 属性字段
        [SerializeField]
        private TextMeshProUGUI _title;
        [SerializeField]
        private TextMeshProUGUI _desc;
        #endregion
    }
}