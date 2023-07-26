using TMPro;
using UnityEngine;

namespace Game.BattleTable
{
    public class Gem : MonoBehaviour
    {
        #region 公有事件
        public void setGem(int value)
        {
            value = Mathf.Max(value, 0);
            _gemText.text = value.ToString();
        }
        #endregion

        #region 属性字段
        [SerializeField]
        private TextMeshPro _gemText;
        #endregion
    }
}