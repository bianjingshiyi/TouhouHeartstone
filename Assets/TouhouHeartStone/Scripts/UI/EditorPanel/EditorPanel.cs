using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TouhouCardEngine;
using TouhouHeartstone;
using System.Reflection;
using Game;
using TouhouHeartstone.Builtin;
using System;
using BJSYGameCore.UI;
using System.IO.Compression;
using System.IO;
using MukioI18n;
using NitoriNetwork.Common;
using System.Threading.Tasks;
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
            Dictionary<string,CardPoolInfo> initDic = EditorCtrl.getCardPoolsInfoAsync().Result;
            
        }

        private void UpdateDeckList(Dictionary<string, CardPoolInfo> now)
        {
            deckList.clearItems();
            foreach (var i in now.Values) 
            {
                //FileItem.
                deckList.addChild((RectTransform)Instantiate(FileItem).transform);
            }
        }
    }
}