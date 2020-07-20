using UnityEngine.EventSystems;
namespace UI
{
    partial class Game : IPointerDownHandler
    {
        partial void onAwake()
        {
            QuitButtonButtonGlass.asButton.onClick.AddListener(() =>
            {
                parent.game.game.Dispose();
                parent.display(parent.MainMenu);
            });
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            Table.clickNoWhere(eventData);
        }
    }
}
