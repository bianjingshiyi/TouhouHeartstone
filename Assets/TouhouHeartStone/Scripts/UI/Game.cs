using System;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.Events;
using BJSYGameCore.UI;
using Game;

namespace UI
{
    partial class Game : IPointerDownHandler
    {
        [MukioI18n.TranslateMsg]
        const string QUIT_GAME_QUESTION = "是否要结束本场游戏？";

        partial void onAwake()
        {
            QuitButtonButtonBlack.asButton.onClick.AddListener(() =>
            {
                ui.getManager<UIManager>().getObject<UIDialog>()?.ShowQuestion(StringTable.HINT_TITLE, QUIT_GAME_QUESTION, () => {
                    Main main = GetComponentInParent<Main>();
                    main.game.quitGame();
                    main.display(main.MainMenu);
                });
            });
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            Table.clickNoWhere(eventData);
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
    }
}
