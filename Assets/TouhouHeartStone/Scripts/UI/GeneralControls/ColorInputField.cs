using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UIElements;

namespace HSVPicker
{

    [RequireComponent(typeof(TMP_InputField))]
    public class ColorInputField : MonoBehaviour
    {
        public ColorPicker picker;
        public ColorValues type;

        public string prefix = "R: ";
        public float minValue = 0;
        public float maxValue = 255;

        public int precision = 0;

        [SerializeField, HideInInspector]
        private TMP_InputField inputField;

        private void Awake()
        {
            inputField = GetComponent<TMP_InputField>();

            inputField.onEndEdit.AddListener(onInputFieldChanged);
        }

        private void OnEnable()
        {
            if (Application.isPlaying && picker != null)
            {
                picker.onValueChanged.AddListener(ColorChanged);
                picker.onHSVChanged.AddListener(HSVChanged);
            }
        }

        private void OnDestroy()
        {
            if (picker != null)
            {
                picker.onValueChanged.RemoveListener(ColorChanged);
                picker.onHSVChanged.RemoveListener(HSVChanged);
            }
            inputField.onEndEdit.RemoveListener(onInputFieldChanged);
        }

    #if UNITY_EDITOR
        private void OnValidate()
        {
            inputField = GetComponent<TMP_InputField>();
            UpdateValue();
        }
    #endif

        private void ColorChanged(Color color)
        {
            UpdateValue();
        }

        private void HSVChanged(float hue, float sateration, float value)
        {
            UpdateValue();
        }

        private void onInputFieldChanged(string value)
        {
            picker.AssignColor(type, (int.Parse(value) - minValue) / (maxValue - minValue));
        }

        private void UpdateValue()
        {
            if(inputField == null)
                return;

            if (picker == null)
            {
                inputField.SetTextWithoutNotify(prefix + "-");
            }
            else
            {
                float value = minValue + (picker.GetValue(type) * (maxValue - minValue));

                inputField.SetTextWithoutNotify(prefix + ConvertToDisplayString(value));
            }
        }

        private string ConvertToDisplayString(float value)
        {
            if (precision > 0)
                return value.ToString("f " + precision);
            else
                return Mathf.FloorToInt(value).ToString();
        }
    }

}