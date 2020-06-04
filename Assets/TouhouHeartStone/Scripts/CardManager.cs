using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TouhouCardEngine;
using BJSYGameCore;
using UI;
using System.Threading.Tasks;
using System.Reflection;
using System.Data;
using IGensoukyo.Utilities;
using TouhouHeartstone;

namespace Game
{
    public class CardManager : Manager
    {
        [SerializeField]
        string _defaultImagePath;
        [SerializeField]
        Sprite _defaultImage;
        /// <summary>
        /// 获取默认卡片贴图
        /// </summary>
        /// <returns></returns>
        public async Task<Sprite> getDefaultSprite()
        {
            if (_defaultImage == null)
            {
                Texture2D texture = await getManager<ResourceManager>().loadTexture(_defaultImagePath);
                _defaultImage = Sprite.Create(texture, new Rect(0, 0, 512, 512), new Vector2(.5f, .5f), 100);
            }
            return _defaultImage;
        }
        protected override void onAwake()
        {
            base.onAwake();
            _ = Load(_externalCardPaths);
        }
        [SerializeField]
        CardSkinData[] _skins = new CardSkinData[0];
        Dictionary<int, CardSkinData> skinDic { get; } = new Dictionary<int, CardSkinData>();
        public Task load()
        {
            return Load(_externalCardPaths);
        }
        /// <summary>
        /// 从给出的路径中加载卡片和皮肤
        /// </summary>
        /// <param name="excelPaths"></param>
        /// <returns></returns>
        public async Task Load(string[] excelPaths)
        {
            //加载内置卡片
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<CardDefine> cardList = new List<CardDefine>(CardHelper.getCardDefines(assemblies));
            //加载外置卡片
            foreach (string path in excelPaths)
            {
                try
                {
                    CardDefine[] cards = await loadCards(path);
                    foreach (var card in cards)
                    {
                        CardDefine older = cardList.Find(c => c.id == card.id);
                        if (older != null)
                            older.merge(card);
                        else
                            cardList.Add(card);
                    }
                }
                catch (Exception e)
                {
                    UberDebug.LogError("读取外部卡牌文件" + path + "失败，发生异常：" + e);
                }
            }
            foreach (CardDefine card in cardList)
            {
                if (defineDic.ContainsKey(card.id))
                {
                    Debug.LogWarning("存在重复ID的卡片" + defineDic[card.id] + "和" + card);
                    continue;
                }
                defineDic.Add(card.id, card);
            }
            //加载内置皮肤
            skinDic.Clear();
            foreach (CardSkinData skin in _skins)
            {
                if (skin.image == null)
                    skin.image = await getDefaultSprite();
                skinDic.Add(skin.id, skin);
            }
            foreach (string path in excelPaths)
            {
                try
                {
                    CardSkinData[] skins = await loadSkins(path);
                    foreach (var skin in skins)
                    {
                        if (skinDic.ContainsKey(skin.id))
                        {
                            Debug.LogWarning("存在重复ID的皮肤" + skinDic[skin.id] + "和" + skin);
                            continue;
                        }
                        skinDic.Add(skin.id, skin);
                    }
                }
                catch (Exception e)
                {
                    UberDebug.LogError("读取外部皮肤文件" + path + "失败，发生异常：" + e);
                }
            }
        }
        /// <summary>
        /// 加载文件中的卡片
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<CardDefine[]> loadCards(string path, RuntimePlatform? platform = null)
        {
#if UNITY_ANDROID
            if (platform == null)
                platform = RuntimePlatform.Android;
#endif
            var dataset = platform != RuntimePlatform.Android ?
                await getManager<ResourceManager>().loadExcelAsDataSet(path, platform) :
                await getManager<ResourceManager>().loadDataSet(path, platform);

            if (dataset.Tables.Count < 1) throw new Exception("空数据集");
            var table = dataset.Tables[0];
            List<CardDefine> cards = new List<CardDefine>();

            for (int i = 1; i < table.Rows.Count; i++)
            {
                var datarow = table.Rows[i];

                if (datarow.ReadCol("Ignore", false))
                    continue;

                var card = new GeneratedCardDefine();
                card.id = datarow.ReadCol<int>("ID");
                card.type = datarow.ReadCol<string>("Type");
                card.setProp(nameof(ServantCardDefine.cost), datarow.ReadCol<int>("Cost", 0));
                card.setProp(nameof(ServantCardDefine.attack), datarow.ReadCol<int>("Attack", 0));
                card.setProp(nameof(ServantCardDefine.life), datarow.ReadCol<int>("Life", 0));
                card.setProp(nameof(ServantCardDefine.tags), datarow.ReadCol<string>("Tags", "").Split(','));
                card.setProp(nameof(ServantCardDefine.keywords), datarow.ReadCol<string>("Keywords", "").Split(','));
                card.setProp(nameof(ServantCardDefine.isToken), datarow.ReadCol<bool>("IsToken", false));

                cards.Add(card);
            }

            return cards.ToArray();
        }
        /// <summary>
        /// 加载文件中的皮肤
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<CardSkinData[]> loadSkins(string path, RuntimePlatform? platform = null)
        {
#if UNITY_ANDROID
            if (platform == null)
                platform = RuntimePlatform.Android;
#endif
            var dataset = platform != RuntimePlatform.Android ?
                await getManager<ResourceManager>().loadExcelAsDataSet(path, platform) :
                await getManager<ResourceManager>().loadDataSet(path, platform);

            if (dataset.Tables.Count < 1) throw new Exception("空数据集");
            var table = dataset.Tables[0];
            List<CardSkinData> skins = new List<CardSkinData>();

            var defSprite = await getDefaultSprite();

            for (int i = 1; i < table.Rows.Count; i++)
            {
                var datarow = table.Rows[i];

                if (datarow.ReadCol("Ignore", false))
                    continue;

                var skin = new CardSkinData()
                {
                    id = datarow.ReadCol<int>("ID"),
                    name = datarow.ReadCol<string>("Name"),
                    desc = datarow.ReadCol<string>("Desc", "")
                };
                string imagePath = datarow.ReadCol("Image", "");
                try
                {
                    Texture2D texture = await getManager<ResourceManager>().loadTexture(imagePath);
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f), 100);
                    skin.image = sprite;
                }
                catch (Exception e)
                {
                    skin.image = defSprite;
                    Debug.LogWarning("加载贴图" + imagePath + "失败：" + e);
                }

                skins.Add(skin);
            }
            return skins.ToArray();
        }
        /// <summary>
        /// 卸载所有加载的资源
        /// </summary>
        public void Unload()
        {

        }
        [SerializeField]
        string[] _externalCardPaths = new string[0];
        Dictionary<int, CardDefine> defineDic { get; set; } = new Dictionary<int, CardDefine>();
        /// <summary>
        /// 根据id获取卡片定义
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CardDefine GetCardDefine(int id)
        {
            return defineDic.ContainsKey(id) ? defineDic[id] : null;
        }
        /// <summary>
        /// 根据条件获取卡片定义
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public CardDefine[] GetCardDefines(Func<CardDefine, bool> filter = null)
        {
            if (filter != null)
                return defineDic.Values.Where(d => filter.Invoke(d)).ToArray();
            return defineDic.Values.ToArray();
        }
        /// <summary>
        /// 根据id获取卡片皮肤
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CardSkinData GetCardSkin(int id)
        {
            if (tempSkinDic.ContainsKey(id))
                return tempSkinDic[id];
            return skinDic.ContainsKey(id) ? skinDic[id] : null;
        }
        Dictionary<int, CardSkinData> tempSkinDic { get; } = new Dictionary<int, CardSkinData>();
        /// <summary>
        /// 添加临时的卡片皮肤
        /// </summary>
        /// <param name="skin"></param>
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
