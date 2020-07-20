using UnityEngine;
using CustomBuilder;

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
            AboutButtonButtonBlack.asButton.onClick.AddListener(() =>
            {
                parent.display(parent.AboutPage);
            });
        }
    }
}
