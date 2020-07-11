using UnityEngine;

namespace UI
{
    partial class MainMenu
    {
        partial void onAwake()
        {
            BuildButton.onClick.AddListener(() =>
            {
                parent.display(parent.Build);
            });
            QuitButton.onClick.AddListener(() =>
            {
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            });
        }
    }
}
