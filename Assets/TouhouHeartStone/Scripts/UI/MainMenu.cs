using UnityEngine;
using CustomBuilder;

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
            if (BuildInformation.Instance != null)
            {
                if (Application.version.StartsWith("0."))
                    VersionText.text = "测试版本 ";
                VersionText.text += BuildInformation.Instance.Version.Substring(BuildInformation.Instance.Version.Length - 7) + " - " + BuildInformation.Instance.BuildDate;
            }
            else
            {
                VersionText.text = "版本 " + Application.version;
            }
            AboutButton.onClick.AddListener(() =>
            {
                parent.display(parent.AboutPage);
            });
        }
    }
}
