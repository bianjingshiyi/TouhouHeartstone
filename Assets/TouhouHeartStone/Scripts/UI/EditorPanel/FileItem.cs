using UnityEngine;
using System;

namespace UI
{
    public partial class FileItem
    {
        public Action<int> onDeckRemove;

        private int id;
        public int ID => id;

        private void Awake()
        {
            delectButtonIconGlass.asButton.onClick.AddListener(() => { onDeckRemove?.Invoke(id); });
        }


    }
}