using System.Collections.Generic;
using BJSYGameCore;
using UI;
using UnityEngine;

namespace Game
{
    public class TutorialManager : Manager
    {
        #region 公有方法

        #region 教程数据
        public bool isTutorialFinished(TutorialID id)
        {
            return (getTutorialState(id) & SKIPPED_STATE_FLAG) > 0;
        }
        public bool isTutorialSkipped(TutorialID id)
        {
            return (getTutorialState(id) & FINISHED_STATE_FLAG) > 0;
        }
        public bool isTutorialRunning(TutorialID id)
        {
            return getTutorialStep(id) > 0;
        }
        public bool isTutorialClosed(TutorialID id)
        {
            return isTutorialSkipped(id) || isTutorialFinished(id);
        }
        public bool isTutorialActive(TutorialID id)
        {
            return !isTutorialClosed(id) && isTutorialRunning(id);
        }
        public int getTutorialStep(TutorialID id)
        {
            return getTutorialState(id) & STEP_STATE_MASK;
        }
        public int getTutorialState(TutorialID id)
        {
            if (tutorialStates.TryGetValue(id, out var value))
                return value;
            return 0;
        }
        public void skipTutorial(TutorialID id)
        {
            var state = getTutorialState(id);
            setTutorialState(id, state | SKIPPED_STATE_FLAG);
        }
        public void finishTutorial(TutorialID id)
        {
            var state = getTutorialState(id);
            setTutorialState(id, state | FINISHED_STATE_FLAG);
        }
        public void setTutorialState(TutorialID id, int value)
        {
            tutorialStates[id] = value;
            var key = getTutorialKey(id);
            PlayerPrefs.SetInt(key, value);
        }
        public void nextStep(TutorialID id)
        {
            var state = getTutorialState(id);
            setTutorialState(id, state + 1);
        }
        public void clearTutorialFinishedData()
        {
            foreach (var pair in tutorialStates)
            {
                var id = pair.Key;
                var key = getTutorialKey(id);
                PlayerPrefs.SetInt(key, 0);
            }
            tutorialStates.Clear();
        }
        #endregion

        public bool nextTutorialStep(TutorialID id, int expectedStep)
        {
            var step = getTutorialStep(id);
            if (step != expectedStep)
                return false;
            nextStep(id);
            return true;
        }
        public bool finishTutorial(TutorialID id, int expectedStep)
        {
            if (!isTutorialActive(id))
                return false;
            var step = getTutorialStep(id);
            if (step != expectedStep)
                return false;
            finishTutorial(id);
            return true;
        }
        #endregion

        #region 私有方法

        #region 生命周期
        private void Start()
        {
            loadTutorialData();
        }
        #endregion

        private void loadTutorialData()
        {
            foreach (var tutorialId in tutorialID)
            {
                var key = getTutorialKey(tutorialId);
                tutorialStates.Add(tutorialId, PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key, 0) : 0);
            }
        }
        private string getTutorialKey(TutorialID id)
        {
            return string.Format(keyTutorialFinishedTemplate, (int)id);
        }

        #endregion

        #region 属性字段
        public MainManager main => getManager<MainManager>();
        const string keyTutorialFinishedTemplate = "TutorialFinished_{0}";
        private Dictionary<TutorialID, int> tutorialStates = new Dictionary<TutorialID, int>();
        private const int SKIPPED_STATE_FLAG = 1 << 9;
        private const int FINISHED_STATE_FLAG = 1 << 8;
        private const int STEP_STATE_MASK = (1 << 8) - 1;
        private static readonly TutorialID[] tutorialID = new TutorialID[]
        {
            TutorialID.PVE,
            TutorialID.Build
        };
        #endregion
    }

    public enum TutorialID
    {
        PVE = 1,
        Build = 2,
    }
}
