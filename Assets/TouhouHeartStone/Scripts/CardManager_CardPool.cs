using System;
using BJSYGameCore;
using System.Threading.Tasks;
using UI;
using Controllers;
using TouhouCardEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Game
{
    partial class CardManager
    {
        #region 公有方法
        public async Task getCardPoolInfosAsync(Action<string, CardPoolInfo> onCardPoolInfoRead)
        {
            var files = game.fileManager.getFiles(CARDPOOLS_DIR_NAME, "*.ddcp", true);
            for (int i = 0; i < files.Length; i++)
            {
                string path = files[i];
                string[] lines = await game.fileManager.readTextFile(path, lineCount: 1);
                if (lines == null || lines.Length < 1)
                    continue;
                onCardPoolInfoRead?.Invoke(files[i], JsonHelper.fromJson<CardPoolInfo>(lines[0]));
            }
        }
        public void addExternalCardPool(int cardPoolId)
        {
            _externalCardDefineDict.Add(cardPoolId, new List<CardDefine>());
            _externalCardSkinDict.Add(cardPoolId, new List<CardSkinData>());
        }
        public async Task<(CardDefine[] defines, CardSkinData[] skins)> getExternalCardDefinesAsync(string[] paths)
        {
            List<CardDefine> defineList = new List<CardDefine>();
            List<CardSkinData> skinList = new List<CardSkinData>();
            for (int i = 0; i < paths.Length; i++)
            {
                var cards = await getExternalCardDefinesAsync(paths[i]);
                defineList.AddRange(cards.defines);
                skinList.AddRange(cards.skins);
            }
            return (defineList.ToArray(), skinList.ToArray());
        }
        public async Task<(CardDefine[] defines, CardSkinData[] skins, ActionNodeGraphInfo[] actionGraphInfos)> getExternalCardDefinesAsync(string path, bool readEditorInfo = false)
        {
            var lines = await game.fileManager.readTextFile(path, 0, 4);
            if (lines != null && lines.Length > 0)
            {
                //卡池
                string json = lines[0];
                CardPoolInfo cardPool = JsonHelper.fromJson<CardPoolInfo>(json);
                if (_externalCardDefineDict.ContainsKey(cardPool.cardPoolId))
                {
                    CardDefine[] defines = _externalCardDefineDict[cardPool.cardPoolId].ToArray();
                    CardSkinData[] skins = _externalCardSkinDict[cardPool.cardPoolId].ToArray();
                    ActionNodeGraphInfo[] actionGraphInfos = readEditorInfo ? _externalCardActionGraphInfoDict[cardPool.cardPoolId].ToArray() : null;
                    return (defines, skins, actionGraphInfos);
                }
                else
                {
                    //卡牌
                    json = lines[1];
                    CardDefine[] defines = JsonHelper.fromJson<CardDefine[]>(json);
                    _externalCardDefineDict.Add(cardPool.cardPoolId, new List<CardDefine>(defines));
                    //皮肤
                    json = lines[2];
                    CardSkinData[] skins = JsonHelper.fromJson<CardSkinData[]>(json);
                    _externalCardSkinDict.Add(cardPool.cardPoolId, new List<CardSkinData>(skins));
                    //编辑器信息
                    ActionNodeGraphInfo[] actionGraphInfos = null;
                    if (readEditorInfo)
                    {
                        json = lines[3];
                        actionGraphInfos = JsonHelper.fromJson<ActionNodeGraphInfo[]>(json);
                        _externalCardActionGraphInfoDict.Add(cardPool.cardPoolId, new List<ActionNodeGraphInfo>(actionGraphInfos));
                    }
                    return (defines, skins, actionGraphInfos);
                }
            }
            else
                throw new IOException("无法读取文件" + path);
        }
        public void addExternalCard(int cardPoolId, CardDefine cardDefine, CardSkinData cardSkin)
        {
            _externalCardDefineDict[cardPoolId].Add(cardDefine);
            _externalCardSkinDict[cardPoolId].Add(cardSkin);
        }
        public async Task<Sprite> loadExternalImage(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            if (tryGetSpriteFromCache(path, out Sprite sprite))
            {
                return sprite;
            }
            byte[] bytes = await game.fileManager.readBinaryFile(path);
            Texture2D texture = new Texture2D(512, 512);
            texture.LoadImage(bytes);
            sprite = Sprite.Create(texture, new Rect(Vector2.zero, new Vector2(texture.width, texture.height)), Vector2.one * .5f);
            //添加到缓存
            addSpriteToCache(path, sprite);
            return sprite;
        }
        /// <summary>
        /// 将外部图片添加到缓存
        /// </summary>
        /// <param name="path"></param>
        /// <param name="sprite"></param>
        public void addSpriteToCache(string path, Sprite sprite)
        {
            _externalSpriteCacheDict[path] = sprite;
        }
        /// <summary>
        /// 尝试获取缓存的外部图片
        /// </summary>
        /// <param name="path"></param>
        /// <param name="sprite"></param>
        /// <returns></returns>
        public bool tryGetSpriteFromCache(string path, out Sprite sprite)
        {
            return _externalSpriteCacheDict.TryGetValue(path, out sprite);
        }
        public Task<Sprite> loadCardImage(int cardId)
        {
            CardSkinData skin = getSkin(cardId);
            if (skin.image != null)
                return Task.FromResult(skin.image);
            else
            {
                return loadExternalImage(skin.imagePath);
            }
        }
        #endregion
        #region 私有方法
        #endregion
        #region 属性字段
        public Dictionary<string, Sprite> externalSpriteCacheDict
        {
            get { return _externalSpriteCacheDict; }
        }
        Dictionary<int, List<CardDefine>> _externalCardDefineDict = new Dictionary<int, List<CardDefine>>();
        Dictionary<int, List<CardSkinData>> _externalCardSkinDict = new Dictionary<int, List<CardSkinData>>();
        Dictionary<int, List<ActionNodeGraphInfo>> _externalCardActionGraphInfoDict = new Dictionary<int, List<ActionNodeGraphInfo>>();
        Dictionary<string, Sprite> _externalSpriteCacheDict = new Dictionary<string, Sprite>();
        const string CARDPOOLS_DIR_NAME = "CardPools";
        #endregion
    }
}
