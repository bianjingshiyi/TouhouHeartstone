using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(Text))]
    [ExecuteAlways]
    public class TextWithEllipsis : MonoBehaviour
    {
        private void OnEnable()
        {
            text.RegisterDirtyVerticesCallback(updateTextWithEllipsis);
        }
        private void OnDisable()
        {
            text.UnregisterDirtyVerticesCallback(updateTextWithEllipsis);
        }
        public void updateTextWithEllipsis()
        {
            var generator = new TextGenerator();
            var settings = text.GetGenerationSettings(text.rectTransform.rect.size);

            string shownText = text.text;
            string fullText = shownText;
            generator.Populate(fullText, settings);

            int visibleCount = generator.characterCountVisible;
            if (shownText.Length > visibleCount && visibleCount >= 0)
            {
                shownText = shownText.Remove(visibleCount);
                while (true)
                {
                    if (shownText.Length <= 0)
                    {
                        fullText = string.Empty;
                        break;
                    }
                    fullText = shownText + "...";
                    generator.Populate(fullText, settings);
                    if (fullText.Length <= generator.characterCountVisible)
                    {
                        break;
                    }
                    shownText = shownText.Remove(shownText.Length - 1);
                }
                text.text = fullText;
            }
        }
        public Text text
        {
            get
            {
                if (!_text)
                {
                    _text = GetComponent<Text>();
                }
                return _text;
            }
        }
        private Text _text;
    }
}