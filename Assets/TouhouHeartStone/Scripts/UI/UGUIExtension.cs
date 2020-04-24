using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
namespace UI
{
    public static class UGUIExtension
    {
        public static RectTransform getChild(this RectTransform transform, string name)
        {
            return transform.Find(name) as RectTransform;
        }
        public static void display(this UIBehaviour ui)
        {
            ui.gameObject.SetActive(true);
        }
        public static void hide(this UIBehaviour ui)
        {
            ui.gameObject.SetActive(false);
        }
        public static string getText(this Button button)
        {
            return button.GetComponentInChildren<Text>(true).text;
        }
        public static void setText(this Button button, string value)
        {
            button.GetComponentInChildren<Text>(true).text = value;
        }
        public static Sprite getSprite(this Button button)
        {
            return button.GetComponentInChildren<Image>().sprite;
        }
        public static void setSprite(this Button button, Sprite value)
        {
            button.GetComponentInChildren<Image>().sprite = value;
        }
    }
}
