using UnityEngine;

namespace TouhouHeartstone.Frontend
{
    [ExecuteInEditMode]
    public class TwoFaceUIFix : MonoBehaviour
    {
        [SerializeField]
        CanvasRenderer frontFace;

        [SerializeField]
        CanvasRenderer backFace;

        bool front;

        private void Update()
        {
            var r = transform.localEulerAngles.y;
            var current = r < 90 || r > 270;
            if (front == current) return;
            front = current;

            if (front)
            {
                frontFace?.SetAlpha(1);
                backFace?.SetAlpha(0);
            }
            else
            {
                frontFace?.SetAlpha(0);
                backFace?.SetAlpha(1);
            }
        }
    }
}