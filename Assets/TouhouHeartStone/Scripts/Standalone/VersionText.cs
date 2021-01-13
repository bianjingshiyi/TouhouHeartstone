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
            GetComponent<Text>().text = versionPrefix + BuildInformation.Instance.Version;

            if (includeBuildDate)
                GetComponent<Text>().text += " - " + BuildInformation.Instance.BuildDate;
        }
    }
}
