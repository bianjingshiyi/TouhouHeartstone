using UnityEngine;

namespace Game.BattleTable
{
    [ExecuteAlways]
    public class CameraLimiter : MonoBehaviour
    {
        private void OnEnable()
        {
            var currentAspect = Screen.width / (float)Screen.height;
            updateCamera(currentAspect);
        }
        private void OnDisable()
        {
            var rect = _camera.rect;
            rect.size = Vector2.one;
            rect.center = new Vector2(0.5f, 0.5f);
            _camera.rect = rect;
        }
        private void Update()
        {
            var currentAspect = Screen.width / (float)Screen.height;
            if (currentAspect != lastAspect)
            {
                lastAspect = currentAspect;
                updateCamera(currentAspect);
            }
        }
        private void updateCamera(float currentAspect)
        {
            if (!_camera)
                return;
            var rectSize = new Vector2(1, 1);
            if (_safeAspectMin > 0 && currentAspect < _safeAspectMin)
            {
                rectSize = new Vector2(1, 1 / (_safeAspectMin / currentAspect));
            }
            else if (_safeAspectMax > 0 && currentAspect > _safeAspectMax)
            {
                rectSize = new Vector2(_safeAspectMax / currentAspect, 1);
            }
            var rect = _camera.rect;
            rect.size = rectSize;
            rect.center = new Vector2(0.5f, 0.5f);
            _camera.rect = rect;
        }

        private float lastAspect;
        [SerializeField]
        private float _safeAspectMin = -1;
        [SerializeField]
        private float _safeAspectMax = -1;
        [SerializeField]
        private Camera _camera;
    }
}
