using System;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.Events;
using BJSYGameCore.UI;
using Game;

namespace UI
{
    partial class Game : UIObject, IPointerDownHandler
    {
        [MukioI18n.TranslateMsg]
        const string QUIT_GAME_QUESTION = "是否要结束本场游戏？";

        protected override void Awake()
        {
            onAwake();
        }

        void onAwake()
        {
            Debug.Log("成功添加监听");

            quitButton.onClick.AddListener(() =>
            {
                ui.getObject<UIDialog>()?.ShowQuestion(StringTable.HINT_TITLE, QUIT_GAME_QUESTION, () => {
                    Main main = GetComponentInParent<Main>();
                    main.game.quitGame();
                    main.display(main.MainMenu);
                });
            });
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            table.clickNoWhere(eventData);
        }

        [SerializeField]
        UnityEvent onPageOpen;

        [SerializeField]
        UnityEvent onPageClose;

        void OnEnable()
        {
            onPageOpen?.Invoke();
        }

        void OnDisable()
        {
            onPageClose?.Invoke();
        }

        public GameResultDialog GameResultDialog => gameResultDialog;
        public Table Table => table;
        public LogScrollView LogScrollView => logPanel.logScrollView;
    }
}
