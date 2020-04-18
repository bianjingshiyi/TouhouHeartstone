using UnityEngine.EventSystems;
using UnityEngine;
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
    }
}
