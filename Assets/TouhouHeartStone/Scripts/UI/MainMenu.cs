using UnityEngine;

namespace UI
{
    partial class MainMenu
    {
        partial void onAwake()
        {
            ManMachineButton.onClick.AddListener(() =>
            {
                parent.game.startLocalGame();
            });
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
            NetworkButton.onClick.AddListener(() =>
            {
                parent.display(parent.NetworkingPage);
            });
        }
    }
}
