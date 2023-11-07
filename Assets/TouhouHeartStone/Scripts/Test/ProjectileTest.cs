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
        }
        private void OnFireButtonClick()
        {
            var srcTransform = _sourceTransform ?? transform;
            ProjectileEffect projectile = Instantiate(_projectilePrefab, srcTransform.position, _projectilePrefab.transform.rotation);
            projectile.sourceTransform = srcTransform;
            projectile.destTransform = _targetTransform;
            projectile.testMode = true;
            projectile.onFinished += e => Destroy(e.gameObject);
        }
        [SerializeField]
        Button _fireButton;
        [SerializeField]
        Transform _sourceTransform;
        [SerializeField]
        Transform _targetTransform;
        [SerializeField]
        ProjectileEffect _projectilePrefab;
    }
}