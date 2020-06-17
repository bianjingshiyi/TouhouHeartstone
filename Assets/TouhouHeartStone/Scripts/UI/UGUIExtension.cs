using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
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
        public static void display(this RectTransform transform)
        {
            transform.gameObject.SetActive(true);
        }
        public static void hide(this RectTransform transform)
        {
            transform.gameObject.SetActive(false);
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
        public static float getAlpha(this Graphic graphic)
        {
            return graphic.color.a;
        }
        public static void setAlpha(this Graphic graphic, float value)
        {
            graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, value);
        }
        public static void set(this UnityEvent e, UnityAction action)
        {
            e.RemoveAllListeners();
            e.AddListener(action);
        }
        public static void set<T>(this UnityEvent<T> e, UnityAction<T> action)
        {
            e.RemoveAllListeners();
            e.AddListener(action);
        }
        public static bool isSelectable(this Selectable selectable)
        {
            return selectable.interactable;
        }
        public static void setSelectable(this Selectable selectable, bool value)
        {
            if (value)
            {
                selectable.interactable = true;
                selectable.image.color = Color.white;
            }
            else
            {
                selectable.interactable = false;
                selectable.image.color = Color.gray;
            }
        }
    }
}
