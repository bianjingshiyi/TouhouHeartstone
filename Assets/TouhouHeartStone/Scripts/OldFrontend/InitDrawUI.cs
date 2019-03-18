
using UnityEngine;

using TouhouHeartstone.OldFrontend.Manager;

namespace TouhouHeartstone.OldFrontend
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
