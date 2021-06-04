using System.Collections.Generic;
using UnityEngine;
using Game;
using Controllers;

namespace UI
{
    public partial class EditorPanel
    {

        LangManager lang => ui.getManager<LangManager>();

        private EditorCtrl _EditorCtrl;

        EditorCtrl EditorCtrl
        {
            get
            {
                if (_EditorCtrl == null) _EditorCtrl = new EditorCtrl();
                return _EditorCtrl;
            }
        }

        public GameObject FileItem;

        protected override void Awake()
        {
            base.Awake();
            backButtonBlack.asButton.onClick.AddListener(onBackBtnClick);
        }

        void onBackBtnClick()
        {
            Main main = GetComponentInParent<Main>();
            main.display(main.MainMenu);
        }

        private void Start()
        {
            UpdateDeckList(EditorCtrl.getCardPoolsInfoAsync().Result);
        }

        private void UpdateDeckList(Dictionary<string, CardPoolInfo> now)
        {
            fileList.clearItems();
            foreach (var i in now.Values)
            {
                FileItem item = fileList.addItem();
                item.nameText.text = i.cardPoolName;
                item.iconImage.sprite = (Sprite)Resources.Load(i.imageResPath);
                item.dateText.text = null;
                item.onDeckRemove += OnFileItemRemove;
            }
        }

        private void OnFileItemRemove(int id)
        {
            //这里需要一个删除对应文件方法

            UpdateDeckList(EditorCtrl.getCardPoolsInfoAsync().Result);
        }
    }
}