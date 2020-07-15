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
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using ILogger = TouhouCardEngine.Interfaces.ILogger;
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
        public void setDefaultImagePath(string path)
        {
            _defaultImagePath = path;
        }
        [SerializeField]
        CardSkinData[] _skins = new CardSkinData[0];
        Dictionary<int, CardSkinData> skinDic { get; } = new Dictionary<int, CardSkinData>();
        /// <summary>
        /// 加载配置路径中的所有卡片和皮肤。
        /// </summary>
        /// <returns></returns>
        public Task load(Assembly[] assemblies = default, ILogger logger = null)
        {
            return load(_externalCardPaths, assemblies, logger);
        }
        /// <summary>
        /// 从给出的路径中加载卡片和皮肤，支持通配符，比如“Cards/*.xls”
        /// </summary>
        /// <param name="excelPaths"></param>
        /// <returns></returns>
        public async Task load(string[] excelPaths, Assembly[] assemblies = default, ILogger logger = null)
        {
            //加载内置卡片
            if (assemblies == default)
                assemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<CardDefine> cardList = new List<CardDefine>(CardHelper.getCardDefines(assemblies, logger));

            List<string> pathList = new List<string>();
            foreach (var excelPath in excelPaths)
            {
                // 判断文件夹
                if (excelPath.Contains("*") || excelPath.Contains("?"))
                {
                    if (!PlatformCompability.Current.RequireWebRequest)
                    {
                        var pathes = ResourceManager.GetDirectoryFiles(excelPath);
                        // 去重
                        foreach (var item in pathes)
                        {
                            if (!pathList.Contains(item)) pathList.Add(item);
                        }
                    }
                }
                else
                {
                    if (!pathList.Contains(excelPath)) pathList.Add(excelPath);
                }
            }

            //加载外置卡片
            foreach (string path in pathList)
            {
                try
                {
                    CardDefine[] cards = await loadCards(path);
                    foreach (var card in cards)
                    {
                        CardDefine older = cardList.Find(c => c.id == card.id);
                        if (older != null)
                        {
                            older.merge(card);
                            UberDebug.LogChannel("Load", "数据覆盖：" + card.ToJson());
                        }
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
            foreach (string path in pathList)
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
        public async Task<CardDefine[]> loadCards(string path, PlatformCompability platform = null)
        {
            platform = platform ?? PlatformCompability.Current;

            DataSet dataset = platform.SupportExcelReading ?
                await getManager<ResourceManager>().loadExcelAsDataSet(path, platform) :
                await getManager<ResourceManager>().loadDataSet(path, platform);

            if (dataset.Tables.Count < 1) throw new Exception("空数据集");
            var table = dataset.Tables[0];
            List<CardDefine> cards = new List<CardDefine>();

            for (int i = 1; i < table.Rows.Count; i++)
            {
                try
                {
                    var datarow = table.Rows[i];

                    if (datarow.ReadCol("Ignore", false))
                        continue;

                    var card = new GeneratedCardDefine();
                    card.id = datarow.ReadCol<int>("ID");
                    card.type = datarow.ReadCol<string>("Type");
                    card.setProp(nameof(ServantCardDefine.cost), datarow.ReadCol("Cost", 0));
                    card.setProp(nameof(ServantCardDefine.attack), datarow.ReadCol("Attack", 0));
                    card.setProp(nameof(ServantCardDefine.life), datarow.ReadCol("Life", 0));
                    card.setProp(nameof(ServantCardDefine.tags), datarow.ReadCol("Tags", "").Split(','));
                    card.setProp(nameof(ServantCardDefine.keywords), datarow.ReadCol("Keywords", "").Split(','));
                    card.setProp(nameof(ServantCardDefine.isToken), datarow.ReadCol("IsToken", false));
                    card.setProp(nameof(ServantCardDefine.isActive), datarow.ReadCol("IsActive", false));

                    cards.Add(card);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Error when loading {path} (row {i}): " + e);
                }
            }

            return cards.ToArray();
        }
        /// <summary>
        /// 加载文件中的皮肤
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<CardSkinData[]> loadSkins(string path, PlatformCompability platform = null)
        {
            platform = platform ?? PlatformCompability.Current;

            DataSet dataset = platform.SupportExcelReading ?
                await getManager<ResourceManager>().loadExcelAsDataSet(path, platform) :
                await getManager<ResourceManager>().loadDataSet(path, platform);

            if (dataset.Tables.Count < 1) throw new Exception("空数据集");
            var table = dataset.Tables[0];
            List<CardSkinData> skins = new List<CardSkinData>();

            for (int i = 1; i < table.Rows.Count; i++)
            {
                try
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
                        Debug.LogWarning("加载贴图 " + imagePath + " 失败：" + e);
                        skin.image = await getDefaultSprite();
                    }

                    skins.Add(skin);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Error when loading {path} (row {i}): " + e);
                }
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
        public CardSkinData getSkin(TouhouCardEngine.Card card)
        {
            return getSkin(card.define.id);
        }
        /// <summary>
        /// 根据id获取卡片皮肤
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CardSkinData getSkin(int id)
        {
            if (tempSkinDic.ContainsKey(id))
                return tempSkinDic[id];
            if (skinDic.ContainsKey(id))
                return skinDic[id];
            return null;
        }
        public bool tryGetSkin(int id, out CardSkinData skin)
        {
            if (tempSkinDic.ContainsKey(id))
            {
                skin = tempSkinDic[id];
                return true;
            }
            else if (skinDic.ContainsKey(id))
            {
                skin = skinDic[id];
                return true;
            }
            else
            {
                skin = null;
                return false;
            }
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
        public bool isStandardCard(CardDefine c)
        {
            if (c.id == 0)
                return false;
            if (c.GetType().Assembly != typeof(THHGame).Assembly)
                return false;
            //if (!(c is SpellCardDefine))
            //{
            //    if (!(c is ServantCardDefine) && !(c is GeneratedCardDefine))
            //        return false;
            //}
            if (!(c is ServantCardDefine) && !(c is GeneratedCardDefine))
                return false;
            if (c is ServantCardDefine servant)
            {
                if (servant.isToken)
                    return false;
            }
            else if (c is GeneratedCardDefine generated)
            {
                if (generated.type != CardDefineType.SERVANT)
                    return false;
                if (generated.getProp<bool>(nameof(ServantCardDefine.isToken)))
                    return false;
            }
            return true;
        }
    }

    [Serializable]
    public class SkinNotFoundException : Exception
    {
        public SkinNotFoundException() { }
        public SkinNotFoundException(string message) : base(message) { }
        public SkinNotFoundException(string message, Exception inner) : base(message, inner) { }
        protected SkinNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
