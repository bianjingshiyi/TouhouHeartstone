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

        [SerializeField]
        Rect ReserveSize = new Rect();

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

                // quickfix: 这个保留空间先暴力取这么大吧，我也不知道为啥这里计算出来会和实际的不一样
                var reserveHeight = size * 0.5;
                if (height < (rect.height - reserveHeight) * pixelsPerUnit)
                {
                    var c = gen.GetCharactersArray();
                    var pos = c[c.Length - 1].cursorPos;
                    
                    // 判断是否有文字处于右下角的预留区域，有就继续缩小
                    if (pos.y > rect.yMin + ReserveSize.height + fontSize || pos.x < rect.xMax - ReserveSize.width)
                    {
                        break;
                    }
                }
            }
            fontSize = size;
        }
    }
}
