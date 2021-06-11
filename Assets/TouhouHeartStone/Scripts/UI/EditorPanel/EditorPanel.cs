using System.Collections.Generic;
using UnityEngine;
using Game;
using Controllers;
using BJSYGameCore;
using System.IO;
namespace UI
{
    public partial class EditorPanel
    {
        //protected override void Awake()
        //{
        //    base.Awake();
        //    backButtonBlack.asButton.onClick.AddListener(onBackBtnClick);
        //}

        //void onBackBtnClick()
        //{
        //    Main main = GetComponentInParent<Main>();
        //    main.display(main.MainMenu);
        //}

        //private void Start()
        //{
        //    UpdateDeckList(EditorCtrl.getCardPoolsInfoAsync().Result);
        //}
        //public void setImage(string imageResPath)
        //{
        //    throw new System.NotImplementedException();
        //}
        //private void UpdateDeckList(Dictionary<string, CardPoolInfo> now)
        //{
        //    fileList.clearItems();
        //    foreach (var i in now.Values)
        //    {
        //        FileItem item = fileList.addItem();
        //        item.nameText.text = i.cardPoolName;
        //        item.iconImage.sprite = await ui.getManager<THHResourceManager>().loadTexture(i.imageResPath, Path.GetDirectoryName(path));
        //        //(Sprite)Resources.Load(i.imageResPath);
        //        item.dateText.text = null;
        //        item.onDeckRemove += OnFileItemRemove;
        //    }
        //}

        //private void OnFileItemRemove(int id)
        //{
        //    //这里需要一个删除对应文件方法

        //    UpdateDeckList(EditorCtrl.getCardPoolsInfoAsync().Result);
        //}
    }
}