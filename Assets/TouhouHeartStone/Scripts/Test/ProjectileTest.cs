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
            ProjectileEffect projectile = Instantiate(_projectilePrefab);
            projectile.sourceTransform = transform;
            projectile.destTransform = _targetTransform;
            projectile.testMode = true;
            projectile.onFinished += e => Destroy(e.gameObject);
        }
        [SerializeField]
        Button _fireButton;
        [SerializeField]
        Transform _targetTransform;
        [SerializeField]
        ProjectileEffect _projectilePrefab;
    }
}