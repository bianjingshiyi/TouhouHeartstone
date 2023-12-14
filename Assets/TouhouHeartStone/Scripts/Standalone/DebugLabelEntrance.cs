using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace IGensoukyo.Utilities
{
    class DebugLabelEntrance : MonoBehaviour, IPointerClickHandler
    {
        public void setEnabled(bool value)
        {
            isEnabled = value;
            if (!value)
            {
                lastSw?.Stop();
                counter = 0;
            }
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isEnabled)
                return;
            if (lastSw == null)
            {
                lastSw = new Stopwatch();
                lastSw.Start();
            }

            if (lastSw.ElapsedMilliseconds > 10000)
            {
                lastSw.Restart();
                counter = 0;
            }

            counter++;
            if (counter >= times)
            {
                if (lastSw.ElapsedMilliseconds < 10000)
                {
                    evt?.Invoke();
                }

                lastSw.Stop();
                lastSw = null;
                counter = 0;
            }
        }
        [SerializeField]
        bool isEnabled = true;
        [SerializeField]
        int times = 10;
        [SerializeField]
        UnityEvent evt;
        int counter = 0;
        Stopwatch lastSw = null;

    }
}
