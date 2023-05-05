using UnityEngine;
using UnityEngine.EventSystems;
using System.Diagnostics;
using UnityEngine.Events;

namespace IGensoukyo.Utilities
{
    class DebugLabelEntrance : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        int times = 10;

        [SerializeField]
        UnityEvent evt;

        int counter = 0;
        Stopwatch lastSw = null;

        public void OnPointerClick(PointerEventData eventData)
        {
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
    }
}
