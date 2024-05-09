using BJSYGameCore;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class LocalizedInputField : Selectable
    {
        public void setLanguages(string[] names)
        {
            _languageDropdown.ClearOptions();
            _languageDropdown.AddOptions(names.ToList());
        }
        protected override void Awake()
        {
            _inputField.onEndEdit.AddListener(value => onEndEdit?.Invoke(value));
            _languageDropdown.onValueChanged.AddListener(value => OnLanguageChanged?.Invoke(value));
            initEvents();
        }
        private void OnSelectInputField(BaseEventData eventData)
        {
            onSelect?.Invoke();
        }
        private void OnDeselectInputField(BaseEventData eventData)
        {
            if (!shouldEventDeselect(eventData))
                return;
            onDeselect?.Invoke();
        }
        private void OnSelectDropdown(BaseEventData eventData)
        {
            onSelect?.Invoke();
        }
        private void OnDeselectDropdown(BaseEventData eventData)
        {
            if (!shouldEventDeselect(eventData))
                return;
            onDeselect?.Invoke();
        }
        private void initEvents()
        {
            EventTrigger trigger;
            EventTrigger.Entry entry;
            trigger = _inputField.gameObject.AddComponent<EventTrigger>();

            entry = new EventTrigger.Entry() { eventID = EventTriggerType.Select };
            entry.callback.AddListener(OnSelectInputField);
            trigger.triggers.Add(entry);

            entry = new EventTrigger.Entry() { eventID = EventTriggerType.Deselect };
            entry.callback.AddListener(OnDeselectInputField);
            trigger.triggers.Add(entry);

            trigger = _languageDropdown.gameObject.AddComponent<EventTrigger>();

            entry = new EventTrigger.Entry() { eventID = EventTriggerType.Select };
            entry.callback.AddListener(OnSelectDropdown);
            trigger.triggers.Add(entry);

            entry = new EventTrigger.Entry() { eventID = EventTriggerType.Deselect };
            entry.callback.AddListener(OnDeselectDropdown);
            trigger.triggers.Add(entry);
        }
        private bool shouldEventDeselect(BaseEventData eventData)
        {
            if (eventData is not PointerEventData pointerEventData)
                return false;
            var currentRaycastObj = pointerEventData.pointerCurrentRaycast.gameObject;
            if (!currentRaycastObj)
                return true;
            var raycastTransform = currentRaycastObj.transform;
            if (raycastTransform != _languageDropdown.transform && !raycastTransform.IsChildOf(_languageDropdown.transform) &&
                raycastTransform != _inputField.transform && !raycastTransform.IsChildOf(_inputField.transform))
                return true;
            return false;
        }
        public event Action onSelect;
        public event Action onDeselect;
        public event Action<string> onEndEdit;
        public event Action<int> OnLanguageChanged;
        public InputField inputField => _inputField;
        [SerializeField]
        private InputField _inputField;
        [SerializeField]
        private TMP_Dropdown _languageDropdown;
    }
}
