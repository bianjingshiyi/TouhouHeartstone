using System;
using UnityEngine.UI;

namespace UI
{
    [Obsolete("使用GenericDialog作为替代")]
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
