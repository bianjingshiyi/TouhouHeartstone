using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class WaitingBlocker : MonoBehaviour
    {
        public void addWaitingTask(WaitingTask task, string text = null, bool showProgressBar = false)
        {
            waitingTasks.Add(task);
            _text.text = text ?? string.Empty;
            _progressBar.gameObject.SetActive(showProgressBar);
            updateGameObject();
        }
        public void removeWaitingTask(WaitingTask task)
        {
            waitingTasks.Remove(task);
            updateGameObject();
        }
        public void setProgress(float progress)
        {
            _progressBar.value = progress;
        }
        private void updateGameObject()
        {
            gameObject.SetActive(waitingTasks.Count > 0);
        }

        #region 字段属性
        private HashSet<WaitingTask> waitingTasks = new HashSet<WaitingTask>();
        [SerializeField]
        private TextMeshProUGUI _text;
        [SerializeField]
        private Slider _progressBar;
        #endregion
    }
    public enum WaitingTask
    {
        UserAccount,
        CardEditor,
        Build,
        Editor,
        LoadWorkshop
    }
}
