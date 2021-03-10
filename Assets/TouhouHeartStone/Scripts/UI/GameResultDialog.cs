using BJSYGameCore.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using TouhouHeartstone;
using Game;
using BJSYGameCore;

namespace UI
{
    public partial class GameResultDialog : UIObject
    {
        protected override void Awake()
        {
            base.Awake();
            autoInit();
        }
        public UnityEvent onGameSuccess;
        public UnityEvent onGameFail;
        public void ShowGameResult(bool success)
        {
            gameObject.SetActive(true);
            this.findInstance<GameManager>()?.endGame();

            if (success)
            {
                onGameSuccess?.Invoke();
            }
            else
            {
                onGameFail?.Invoke();
            }
        }

        public void WindowClose()
        {
            gameObject.SetActive(false);
            this.findInstance<GameManager>()?.closeGameUI();
        }
    }
}
