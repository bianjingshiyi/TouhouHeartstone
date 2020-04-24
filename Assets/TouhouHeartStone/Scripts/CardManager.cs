using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using TouhouCardEngine;
using BJSYGameCore;
using UI;
using ExcelLibrary.SpreadSheet;
namespace Game
{
    public class CardManager : Manager
    {
        [SerializeField]
        string _defaultImagePath;
        [SerializeField]
        Sprite _defaultImage;
        Sprite defaultImage
        {
            get
            {
                if (_defaultImage == null)
                {
                    if (File.Exists(Application.streamingAssetsPath + "/" + _defaultImagePath))
                    {
                        using (FileStream stream = new FileStream(Application.streamingAssetsPath + "/" + _defaultImagePath, FileMode.Open))
                        {
                            Texture2D texture = new Texture2D(512, 512);
                            byte[] bytes = new byte[stream.Length];
                            stream.Read(bytes, 0, (int)stream.Length);
                            texture.LoadImage(bytes);
                            _defaultImage = Sprite.Create(texture, new Rect(0, 0, 512, 512), new Vector2(.5f, .5f), 100);
                        }
                    }
                }
                return _defaultImage;
            }
        }
        protected override void onAwake()
        {
            base.onAwake();
            Load();
        }
        [SerializeField]
        CardSkinData[] _skins = new CardSkinData[0];
        Dictionary<int, CardSkinData> skinDic { get; } = new Dictionary<int, CardSkinData>();
        public void Load()
        {
            skinDic.Clear();
            foreach (CardSkinData skin in _skins)
            {
                if (skin.image == null)
                    skin.image = defaultImage;
                skinDic.Add(skin.id, skin);
            }

            Dictionary<Workbook, string> workbooks = new Dictionary<Workbook, string>();
            foreach (var path in _externalCardPaths)
            {
                if (Directory.Exists(Application.streamingAssetsPath + "/" + path))
                {
                    foreach (var filePath in Directory.GetFiles(Application.streamingAssetsPath + "/" + path, "*.xls", SearchOption.AllDirectories))
                    {
                        using (FileStream stream = new FileStream(filePath, FileMode.Open))
                        {
                            workbooks.Add(Workbook.Load(stream), filePath);
                        }
                    }
                }
                else if (File.Exists(Application.streamingAssetsPath + "/" + path))
                {
                    using (FileStream stream = new FileStream(Application.streamingAssetsPath + "/" + path, FileMode.Open))
                    {
                        workbooks.Add(Workbook.Load(stream), Application.streamingAssetsPath + "/" + path);
                    }
                }
            }
            var cards = CardImporter.GetCardDefines(AppDomain.CurrentDomain.GetAssemblies(), workbooks, out var skins);
            foreach (CardDefine card in cards)
            {
                if (defineDic.ContainsKey(card.id))
                {
                    Debug.LogWarning("存在重复ID的卡片" + defineDic[card.id] + "和" + card);
                    continue;
                }
                defineDic.Add(card.id, card);
            }
            foreach (CardSkinData skin in skins)
            {
                if (skinDic.ContainsKey(skin.id))
                {
                    Debug.LogWarning("存在重复ID的皮肤" + skinDic[skin.id] + "和" + skin);
                    continue;
                }
                skinDic.Add(skin.id, skin);
            }
        }
        public void Unload()
        {

        }
        [SerializeField]
        string[] _externalCardPaths = new string[0];
        Dictionary<int, CardDefine> defineDic { get; set; } = new Dictionary<int, CardDefine>();
        public CardDefine GetCardDefine(int id)
        {
            return defineDic.ContainsKey(id) ? defineDic[id] : null;
        }
        public CardDefine[] GetCardDefines(Func<CardDefine, bool> filter = null)
        {
            if (filter != null)
                return defineDic.Values.Where(d => filter.Invoke(d)).ToArray();
            return defineDic.Values.ToArray();
        }
        public CardSkinData GetCardSkin(int id)
        {
            if (tempSkinDic.ContainsKey(id))
                return tempSkinDic[id];
            return skinDic.ContainsKey(id) ? skinDic[id] : null;
        }
        Dictionary<int, CardSkinData> tempSkinDic { get; } = new Dictionary<int, CardSkinData>();
        public void AddCardSkinTemp(CardSkinData skin)
        {
            if (skinDic.ContainsKey(skin.id))
            {
                Debug.LogError("已存在相同的皮肤" + skinDic[skin.id]);
                return;
            }
            if (tempSkinDic.ContainsKey(skin.id))
            {
                Debug.LogError("已存在相同的皮肤" + tempSkinDic[skin.id]);
                return;
            }
            tempSkinDic.Add(skin.id, skin);
        }
    }
}
