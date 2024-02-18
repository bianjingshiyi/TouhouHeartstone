using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class WaitingBlocker : MonoBehaviour
    {
        public void addWaitingTask(WaitingTask task)
        {
            waitingTasks.Add(task);
            updateGameObject();
        }
        public void removeWaitingTask(WaitingTask task)
        {
            waitingTasks.Remove(task);
            updateGameObject();
        }
        private void updateGameObject()
        {
            gameObject.SetActive(waitingTasks.Count > 0);
        }

        #region 字段属性
        private HashSet<WaitingTask> waitingTasks = new HashSet<WaitingTask>();
        #endregion
    }
    public enum WaitingTask
    {
        UserAccount
    }
}
