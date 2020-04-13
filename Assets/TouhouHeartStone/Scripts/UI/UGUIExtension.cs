using UnityEngine.EventSystems;

namespace UI
{
    public static class UGUIExtension
    {
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
