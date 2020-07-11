using System;
namespace UI
{
    partial class Dialog
    {
        public void display(string info, Action onConfirm)
        {
            display();
            Text.text = info;
            Button.onClick.RemoveAllListeners();
            Button.onClick.AddListener(() =>
            {
                onConfirm?.Invoke();
                hide();
            });
        }
    }
}
