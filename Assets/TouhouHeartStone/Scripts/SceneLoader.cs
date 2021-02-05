using UnityEngine;
using BJSYGameCore;
using System.Collections;

namespace Game
{
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField]
        string path;

        [SerializeField]
        UnityEngine.UI.Slider slider;

        LoadSceneOperation op = null;

        private void Start()
        {
            StartCoroutine(loading());
        }

        private IEnumerator loading()
        {
            op = new LoadSceneOperation(path, UnityEngine.SceneManagement.LoadSceneMode.Single);
            while (!op.operation.isDone)
            {
                slider.value = op.operation.progress;
                yield return null;
            }
        }

        private void Update()
        {
            if (op != null)
            {
            }
        }
    }
}
