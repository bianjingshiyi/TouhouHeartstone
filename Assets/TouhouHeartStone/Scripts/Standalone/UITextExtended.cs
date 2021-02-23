using UnityEngine;
using UnityEngine.UI;

namespace IGensoukyo.Utilities
{
    [ExecuteInEditMode]
    class UITextExtended : Text
    {
        [Header("Extended")]
        [SerializeField]
        bool isBestFit = false;

        [SerializeField]
        int MinSize = 0;

        [SerializeField]
        int MaxSize = 0;

        public override string text 
        { 
            get => base.text;
            set { base.text = value; updateTextSize(); }
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            updateTextSize();
        }

        void updateTextSize()
        {
            if (!isBestFit) return;

            TextGenerator gen = cachedTextGenerator;
            Rect rect = rectTransform.rect;

            var genSetting = GetGenerationSettings(rect.size);

            int size;
            for (size = MaxSize; size > MinSize; size--)
            {
                genSetting.fontSize = size;
                var height = gen.GetPreferredHeight(text, genSetting);

                if (height < (rect.height - 1) * pixelsPerUnit)
                    break;
            }
            fontSize = size;
        }
    }
}
