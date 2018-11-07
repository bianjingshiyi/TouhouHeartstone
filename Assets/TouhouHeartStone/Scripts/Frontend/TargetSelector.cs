using UnityEngine;

namespace TouhouHeartstone.Frontend
{
    /// <summary>
    /// 目标选择器，用于选择一个目标
    /// </summary>
    public class TargetSelector : MonoBehaviour
    {
        Vector3 startPos;

        public delegate bool ContentFilter(RaycastHit hit);

        public void SetStartPosition(Vector3 globalPos)
        {
            startPos = globalPos;
        }

        public void HideSelector()
        {
            // todo:
        }

        RaycastHit lastHit;

        public RaycastHit LastSelectTarget => lastHit;

        public bool UpdatePos(Vector3 mousePos, ContentFilter filter)
        {
            // 假设检测底板平面的物体
            mousePos.z = Camera.main.transform.position.y;
            var worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            worldPos.y += 0.1f;

            var result = Physics.RaycastAll(worldPos, Vector3.down);
            foreach (var item in result)
            {
                if (filter(item))
                {
                    lastHit = item;
                    SetHintPos(worldPos, true);
                    return true;
                }
            }

            SetHintPos(worldPos, false);
            return false;
        }

        /// <summary>
        /// 设置hint的位置和显示
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="visible"></param>
        void SetHintPos(Vector3 pos, bool visible)
        {
            // todo: hint?
        }
    }
}
