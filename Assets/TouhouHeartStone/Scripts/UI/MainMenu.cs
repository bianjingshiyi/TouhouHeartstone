using UnityEngine;

namespace UI
{
    partial class MainMenu
    {
        partial void onAwake()
        {
            BuildButtonButtonBlack.asButton.onClick.AddListener(() =>
            {
                parent.display(parent.Build);
            });
            QuitButtonButtonBlack.asButton.onClick.AddListener(() =>
            {
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            });
            AboutButtonButtonBlack.asButton.onClick.AddListener(() =>
            {
                parent.display(parent.AboutPage);
            });
        }
    }
}
