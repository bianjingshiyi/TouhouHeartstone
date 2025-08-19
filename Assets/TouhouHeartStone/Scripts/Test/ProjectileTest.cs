using Game.Animations;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    class ProjectileTest : MonoBehaviour
    {
        protected void Awake()
        {
            _fireButton.onClick.AddListener(OnFireButtonClick);
            _timeSlider.value = Time.timeScale;
            _timeSlider.onValueChanged.AddListener(OnTimeScaleValueChanged);
        }
        private void OnFireButtonClick()
        {
            var srcTransform = _sourceTransform == null ? transform : _sourceTransform;
            TableEffect projectile = Instantiate(_effectPrefab, srcTransform.position + Vector3.up * _effectPrefab.heightOffset, _effectPrefab.transform.rotation, _root);
            projectile.sourceTransform = srcTransform;
            projectile.destTransform = _targetTransform;
            projectile.setTestMode(true);
            projectile.init(null, null, null);
            projectile.onFinished += e => Destroy(e.gameObject);
        }
        private void OnTimeScaleValueChanged(float value)
        {
            Time.timeScale = value;
        }
        [SerializeField]
        Button _fireButton;
        [SerializeField]
        Slider _timeSlider;
        [SerializeField]
        Transform _root;
        [SerializeField]
        Transform _sourceTransform;
        [SerializeField]
        Transform _targetTransform;
        [SerializeField]
        TableEffect _effectPrefab;
    }
}