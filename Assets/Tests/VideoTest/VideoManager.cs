using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.EventSystems;
namespace BJSYGameCore.Video
{
    public class VideoManager : MonoBehaviour
    {
        [SerializeField]
        VideoPlayerController _controller;
        VideoPlayerController controller
        {
            get
            {
                if (_controller == null)
                    _controller = GetComponentInChildren<VideoPlayerController>();
                return _controller;
            }
        }
        [SerializeField]
        VideoClip _clip;
        [SerializeField]
        Slider _progressBar;
        [SerializeField]
        Toggle _toggle;
        protected void Awake()
        {
            _progressBar.onValueChanged.AddListener(onSliderValueChanged);
            _toggle.onValueChanged.AddListener(onToggleValueChanged);

            controller.play(_clip);
            controller.isPlaying = false;
        }
        bool _isTriggedByInput = true;
        bool? _isPlayingBeforeDrag = null;
        private void onSliderValueChanged(float value)
        {
            if (_isTriggedByInput)
            {
                controller.setTime(value);
                _isDragging = true;
                if (_isPlayingBeforeDrag == null)
                {
                    _isPlayingBeforeDrag = controller.isPlaying;
                    controller.isPlaying = false;
                }
            }
        }
        [SerializeField]
        bool _isDragging = false;
        public void onReleaseSlider()
        {
            _isDragging = false;
            controller.isPlaying = _isPlayingBeforeDrag != null ? _isPlayingBeforeDrag.Value : default;
            _isPlayingBeforeDrag = null;
        }
        private void onToggleValueChanged(bool value)
        {
            if (_isTriggedByInput)
            {
                controller.isPlaying = value;
            }
        }
        protected void Update()
        {
            if (!_isDragging)
                updateProgressBar();
            updatePlayButton();
        }
        private void updatePlayButton()
        {
            _isTriggedByInput = false;
            _toggle.isOn = controller.isPlaying;
            _isTriggedByInput = true;
        }
        private void updateProgressBar()
        {
            _isTriggedByInput = false;
            _progressBar.value = controller.time;
            _isTriggedByInput = true;
        }
    }
}