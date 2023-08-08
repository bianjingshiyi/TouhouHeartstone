using MukioI18n;
using TMPro;
using UI;
using UnityEngine;

namespace Game.BattleTable
{
    public class Fatigue : MonoBehaviour
    {
        public void setDamage(int damage, LangManager lang)
        {
            _desc.text = lang._(GAME_FATIGUE_DESC, damage);
        }
        public void setActive(bool active)
        {
            gameObject.SetActive(active);
        }

        public SimpleAnim onFatigue;
        public Animator animator => _animator;
        public PositionLerp positionLerp => _positionLerp;
        [TranslateMsg("疲劳描述（{0}为疲劳伤害）")]
        private const string GAME_FATIGUE_DESC = "没有牌了，你受到了{0}点伤害！";
        [SerializeField]
        private Animator _animator;
        [SerializeField]
        private PositionLerp _positionLerp;
        [SerializeField]
        private TextMeshPro _desc;
    }
}
