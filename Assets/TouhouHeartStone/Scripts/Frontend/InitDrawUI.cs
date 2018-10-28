
using UnityEngine;

using TouhouHeartstone.Frontend.Manager;

namespace TouhouHeartstone.Frontend
{
    /// <summary>
    /// 初始抽卡的UI
    /// </summary>
    public class InitDrawUI : MonoBehaviour
    {
        [SerializeField]
        UIButton okButton;

        [SerializeField]
        InitDrawManager initDraw;

        protected void Awake()
        {
            okButton.OnMouseClick += initDraw.Confirm;
        }

        private void OnEnable()
        {
            
        }

        private void OnDisable()
        {
            
        }
    }
}
