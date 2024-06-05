using System;
using System.Collections;
using System.Collections.Generic;
using Game.Animations;
using TouhouCardEngine;
using UnityEngine;

namespace UI
{
    public class AnimationPreviewTable : MonoBehaviour, IVFXTable
    {
        public void play(AnimationDefine animDef, VisualEffectInfo info)
        {
            if (isPlaying())
            {
                stop();
            }
            if (animDef == null)
                return;
            var effect = animDef.createEffect(_effectParent);
            if (effect)
            {
                effect.init(this, animDef, info);
                effect.onFinished += onFinishedCallback;
            }
            _effect = effect;
        }
        public bool stop()
        {
            if (!isPlaying())
                return false;
            Destroy(_effect.gameObject);
            _effect = null;
            return true;
        }
        public Transform getPreviewTableTransform(int position)
        {
            switch (position)
            {
                case VisualEffectPosition.ENEMY_MASTER:
                    return _enemyMasterTransform;
                case VisualEffectPosition.THIS_CARD:
                    return _thisCardTransform;
                case VisualEffectPosition.TARGET_CARD:
                    return _targetCardTransform;
                default:
                    return _selfMasterTransform;
            }
        }
        public Vector3 WorldToTablePosition(Vector3 worldPos)
        {
            return worldPos * _worldToTableScale;
        }
        public Vector3 TableToWorldPosition(Vector3 tablePos)
        {
            return tablePos / _worldToTableScale;
        }
        public void modifyColor(ColorTarget target, Color color)
        {
            if (!isPlaying())
                return;
            _effect.modifyColorTarget(target, color);
        }
        public void pause()
        {
            if (!isPlaying())
                return;
            _effect.pause();
        }
        public void resume()
        {
            if (!isPlaying())
                return;
            _effect.unpause();
        }
        public bool isPlaying()
        {
            return _effect && _effect.gameObject;
        }
        public bool isPaused()
        {
            return isPlaying() && _effect.state == EffectState.Paused;
        }
        private void Update()
        {
            if (isPaused())
                return;
            if (!isPlaying())
                return;
            _effect.update(Time.deltaTime);
        }
        private void onFinishedCallback(TableEffect effect)
        {
            stop();
            onFinished?.Invoke();
        }
        public event Action onFinished;
        [SerializeField]
        private Transform _effectParent;
        [SerializeField]
        private Transform _selfMasterTransform;
        [SerializeField]
        private Transform _enemyMasterTransform;
        [SerializeField]
        private Transform _thisCardTransform;
        [SerializeField]
        private Transform _targetCardTransform;
        [SerializeField]
        private float _worldToTableScale = 100;
        private TableEffect _effect;
    }
}
