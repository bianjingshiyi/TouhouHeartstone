using UnityEngine;
using IGensoukyo.THDeckDefine;
using System.IO;
using System.Collections.Generic;

namespace TouhouHeartstone.Frontend
{
    [CreateAssetMenu(menuName = "IGSK/CardAppearResourcesLoader")]
    public class CardAppearResourcesLoader : ScriptableObject
    {
        string fallbackID = "invalid";

        [SerializeField]
        string folder = "appearance";

        [SerializeField]
        Sprite defaultMainSprite = null;
        [SerializeField]
        Sprite defaultRibbonSprite = null;
        [SerializeField]
        Sprite defaultBkgSprite = null;

        DeckModel[] DeckModels = null;

        void SearchDeckModel()
        {
            var path = Path.Combine(Application.streamingAssetsPath, folder);
            var files = Directory.GetFiles(path, "*.ths");

            List<DeckModel> models = new List<DeckModel>();
            foreach (var file in files)
            {
                models.Add(DeckReaderWriter.Read(file));
            }
            DeckModels = models.ToArray();
        }

        CardModel GetCard(string id)
        {
            foreach (var item in DeckModels)
            {
                var cards = item.GetCardsByDefineID(id);
                if (cards.Length > 0)
                    return cards[0];
            }
            return null;
        }

        /// <summary>
        /// 获取一个卡片资源
        /// </summary>
        /// <param name="id"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public CardAppearResouces Get(string id, string lang)
        {
            if (DeckModels == null)
                SearchDeckModel();

            var card = GetCard(id);
            if (card == null)
            {
                if (id != fallbackID) return Get(fallbackID, lang);
                else throw new ResourcesIDNotFoundException();
            }

            var item = card.GetMergedItemByLang(lang);
            var res = new CardAppearResouces(item);

            res.BkgSprite = loadSprite(item.FaceImage);
            res.BkgSprite = res.BkgSprite ?? defaultBkgSprite;

            res.MainSprite = loadSprite(item.MainImage);
            res.MainSprite = res.MainSprite ?? defaultMainSprite;

            res.RibbonSprite = loadSprite(item.DecorationImage);
            res.RibbonSprite = res.RibbonSprite ?? defaultRibbonSprite;

            return res;
        }

        Dictionary<string, Sprite> cachedSprites = new Dictionary<string, Sprite>();

        Sprite loadSprite(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            Debug.Log($"Getting image：{path}");

            if (cachedSprites.ContainsKey(path))
                return cachedSprites[path];

            var p = Path.Combine(Application.streamingAssetsPath, folder, path);
            var bytes = File.ReadAllBytes(p);
            var texture = new Texture2D(2, 2);
            texture.LoadImage(bytes);

            Sprite spr = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            cachedSprites.Add(path, spr);
            return spr;
        }
    }

    public class CardAppearResouces
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public Sprite MainSprite { get; set; } = null;
        public Sprite RibbonSprite { get; set; } = null;
        public Sprite BkgSprite { get; set; } = null;

        public CardAppearResouces() { }
        public CardAppearResouces(CardItemModel card) : this()
        {
            Name = card.Name;
            Description = card.Description;
            Type = card.Classification;
        }
    }
}
