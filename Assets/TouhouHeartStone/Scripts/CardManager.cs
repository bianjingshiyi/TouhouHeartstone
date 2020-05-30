using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TouhouCardEngine;
using BJSYGameCore;
using UI;
using ExcelLibrary.SpreadSheet;
using System.Threading.Tasks;
using System.Reflection;
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
            skinDic.Clear();
            foreach (CardSkinData skin in _skins)
            {
                if (skin.image == null)
                    skin.image = await getDefaultSprite();
                skinDic.Add(skin.id, skin);
            }

            Dictionary<Workbook, string> workbooks = new Dictionary<Workbook, string>();
            foreach (var path in excelPaths)
            {
                try
                {
                    Workbook workbook = await getManager<ResourceManager>().loadExcel(path);
                    workbooks.Add(workbook, path);
                }
                catch (Exception e)
                {
                    UberDebug.LogError("读取外部卡牌文件" + path + "失败，发生异常：" + e);
                }
            }
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var result = await CardImporter.GetCardDefines(getManager<ResourceManager>(), assemblies, workbooks, await getDefaultSprite());
            foreach (CardDefine card in result.Key)
            {
                if (defineDic.ContainsKey(card.id))
                {
                    Debug.LogWarning("存在重复ID的卡片" + defineDic[card.id] + "和" + card);
                    continue;
                }
                defineDic.Add(card.id, card);
            }
            foreach (CardSkinData skin in result.Value)
            {
                if (skinDic.ContainsKey(skin.id))
                {
                    Debug.LogWarning("存在重复ID的皮肤" + skinDic[skin.id] + "和" + skin);
                    continue;
                }
                skinDic.Add(skin.id, skin);
            }
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
