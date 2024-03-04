using TMPro;
using UnityEngine;

namespace Game.BattleTable
{
    public class Gem : MonoBehaviour
    {
        #region 公有事件
        public void setExpand(bool expand)
        {
            _animator.SetBool("Expand", expand);
        }
        public void setGem(int v)
        {
            value = Mathf.Max(v, 0);
            _gemText.text = $"{value}/{maxValue}";
        }
        public void setMaxGem(int v)
        {
            maxValue = Mathf.Max(v, 0);
            _gemText.text = $"{value}/{maxValue}";
        }
        #endregion

        #region 属性字段
        private int value;
        private int maxValue;
        [SerializeField]
        private Animator _animator;
        [SerializeField]
        private TextMeshPro _gemText;
        #endregion
    }
}