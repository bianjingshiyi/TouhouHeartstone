using UnityEngine;
using CustomBuilder;
using UnityEngine.UI;

namespace IGensoukyo.Utilities
{
    [RequireComponent(typeof(Text))]
    class VersionText : MonoBehaviour
    {
        [SerializeField]
        string versionPrefix = "";

        [SerializeField]
        bool includeBuildDate = true;

        private void Awake()
        {
            if (BuildInformation.Instance != null)
            {
                if (Application.version.StartsWith("0."))
                    GetComponent<Text>().text = "测试版本 ";
                GetComponent<Text>().text += versionPrefix + BuildInformation.Instance.Version.Substring(BuildInformation.Instance.Version.Length - 7);

                if (includeBuildDate)
                    GetComponent<Text>().text += " - " + BuildInformation.Instance.BuildDate;
            }
            else
            {
                GetComponent<Text>().text = versionPrefix + Application.version;
            }
        }
    }
}
