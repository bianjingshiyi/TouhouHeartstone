using System.Collections.Generic;
using TouhouCardEngine;
using TouhouHeartstone;
using TouhouHeartstone.Builtin;
using UI;
using BJSYGameCore;
using System;
using System.Linq;
using Game;
using UnityEngine;
using SFB;
using UnityEngine.Networking;
using System.Threading.Tasks;
using TouhouCardEngine.Interfaces;
using System.IO;

namespace Controllers
{
    class CardEditorCtrl : GameObjectCtrl<MainSceneCtrl>, IController<MainCtrl>
    {
        #region 公有方法
        /// <summary>
        /// 打开一张卡片进行编辑
        /// </summary>
        public async Task display(int cardPoolId, CardDefine cardDefine, CardDefine[] selectableCharacters)
        {
            gameObject.SetActive(true);
            _selectableCharacters = selectableCharacters;
            _cardPoolId = cardPoolId;
            _cardDefine = cardDefine;
            updateSkinNames();
            _ui.setId(_cardDefine.id);
            _ui.setType(Array.IndexOf(_types, _cardDefine.type));
            string[] charactersName = new string[selectableCharacters.Length + 1];
            charactersName[0] = StringTable.CARDVIEW_CHARACTER_NEUTRAL;
            for (int i = 1; i < charactersName.Length; i++)
            {
                charactersName[i] = game.card.getSkin(selectableCharacters[i - 1].id).name;
            }
            _ui.setSelectableCharacters(charactersName);
            //中立为0，有角色则为index+1
            _ui.setCharacter(Array.FindIndex(_selectableCharacters, c => c.id == cardDefine.getCharacterID()) + 1);
            _ui.setCost(_cardDefine.getProp<int>(DEFINE.COST).ToString());
            _ui.setAttack(_cardDefine.getProp<int>(DEFINE.ATTACK).ToString());
            _ui.setLife(_cardDefine.getProp<int>(DEFINE.LIFE).ToString());
            string[] tags = _cardDefine.getProp<string[]>(DEFINE.TAGS);
            if (tags != null && tags.Length > 0)
            {
                int index = Array.IndexOf(_tags, tags[0]);
                if (index < 0)
                    index = 0;
                _ui.setTag(index);
            }
            else
                _ui.setTag(0);
            GeneratedEffect[] effects = cardDefine.getEffects().OfType<GeneratedEffect>().ToArray();
            updateEffectList(effects);
            _ui.closeAllEdit();
            if (cardDefine.isSpell())
                _ui.switchToSpell();
            else if (cardDefine.isItem())
                _ui.switchToItem();
            else
                _ui.switchToServant();
            await setCurSkinIndex(0);
        }
        #endregion
        #region 私有方法
        #region 生命周期
        protected void Awake()
        {
            if (_ui == null)
            {
                _ui = GetComponentInChildren<CardEditorPanel>();
            }
            _types = new string[] { DEFINE.SERVANT, DEFINE.SPELL, DEFINE.ITEM };
            _ui.setTypes(_types);
            _ui.onChangeSkin += onChangeSkin;
            _ui.onChangeSkinName += onChangeSkinName;
            _ui.onClickDeleteSkinButton += onDeleteSkin;
            _ui.onChangeLanguage += onChangeLanguage;
            _ui.onChangeType += onChangeType;
            _ui.onChangeCharacter += onChangeCharacter;
            _ui.onChangeCost += onChangeCost;
            _ui.onChangeName += onChangeName;
            _ui.onChangeSubName += onChangeSubName;
            _ui.onNeedSelectImage += onNeedSelectImage;
            _ui.onChangeDesc += onChangeDesc;
            _ui.onChangeAttack += onChangeAttack;
            _ui.onChangeLife += onChangeLife;
            _tags = new string[] { string.Empty, TAG.BEAST, TAG.DEMON, TAG.ELEMENT, TAG.FAIRY, TAG.GOD, TAG.MACHINE };
            _ui.setTags(_tags);
            _ui.onChangeTag += onChangeTag;
            _ui.onClickEditEffectButton += onClickEditEffectButton;
            _ui.onClickRemoveEffectButton += onClickRemoveEffectButton;
            _ui.setEffectTypes(game.card.getEffectNames());
            _ui.onClickAddEffectMenu += onClickAddEffectMenu;
            _ui.onClickBackgroundButton += onClickBackgroundButton;
            _ui.onClickDeleteCardButton += onClickDeleteCardButton;
        }
        #endregion
        #endregion

        private void updateSkinNames()
        {
            _ui.setSkinNames(getSkins().GroupBy(s => s.skinId)
                .Select(g => g.FirstOrDefault(s => s.language == game.langManager.getLanguage()) is CardSkinData s2 ? s2.skinName : g.First().skinName)
                .ToArray());
        }
        private async Task setCurSkinIndex(int skinIndex)
        {
            var skinGroup = getSkins().GroupBy(s => s.skinId).ElementAt(skinIndex);
            //获取对应语言的皮肤
            _curCardSkin = skinGroup.FirstOrDefault(s => s.language == game.langManager.getLanguage());
            //如果没有对应语言，就第一个
            if (_curCardSkin == null)
                _curCardSkin = skinGroup.First();
            _ui.setSkin(skinIndex);
            _ui.setLanguage(Array.IndexOf(game.langManager.getLanguages(), _curCardSkin.language));
            _ui.setName(_curCardSkin.name);
            _ui.setSubName(_curCardSkin.subname);
            _ui.setDesc(_curCardSkin.desc);
            string path = _curCardSkin.imagePath;
            //除了StreamingAssets以外，因为无法区分可持续化文件和工程文件必须获取完整根目录
            string cardPoolPath = game.card.getCardPoolPath(game.card.getCardPool(_cardPoolId));
            Sprite image = cardPoolPath.StartsWith("sa:") ?
                await game.card.loadExternalImage(path, null) :
                await game.card.loadExternalImage(path, Path.Combine(game.card.getCardPoolPathRootDir(cardPoolPath), game.card.textureDirName));
            if (image != null)
            {
                _ui.setImage(image);
            }
        }
        int getSkinCount()
        {
            return getSkins().GroupBy(s => s.skinId).Count();
        }
        void updateEffectList(GeneratedEffect[] effects)
        {
            _ui.setEffects(game.card.getEffectNames(effects));
        }
        int allocSkinId()
        {
            return getSkins().GroupBy(c => c.skinId).Select(g => g.Key).Max() + 1;
        }
        IEnumerable<CardSkinData> getSkins()
        {
            return game.card.getAllSkins(_cardDefine.id);
        }
        #region UI回调
        private async void onChangeSkin(int skinIndex)
        {
            if (skinIndex == getSkinCount())
            {
                //新建皮肤
                _curCardSkin = new CardSkinData();
                _curCardSkin.id = _cardDefine.id;
                _curCardSkin.skinId = allocSkinId();
                _curCardSkin.skinName = "新建皮肤";
                scene.editor.addSkin(_curCardSkin);
                updateSkinNames();
            }
            await setCurSkinIndex(skinIndex);
        }
        private async void onChangeSkinName(string skinName)
        {
            var skinGroup = getSkins().GroupBy(c => c.skinId).ToList();
            int index = skinGroup.FindIndex(g => g.Key == _curCardSkin.skinId);
            var curSkinGroup = skinGroup[index];
            foreach (var skin in curSkinGroup)
            {
                skin.skinName = skinName;
            }
            updateSkinNames();
            await setCurSkinIndex(index);
        }
        private async void onDeleteSkin()
        {
            var skinGroup = getSkins().GroupBy(c => c.skinId).ToList();
            if (skinGroup.Count < 2)
            {
                Debug.LogWarning("你不能删除卡牌的最后一个皮肤");
                return;
            }
            scene.editor.removeSkins(c => c.skinId == _curCardSkin.skinId);
            updateSkinNames();
            await setCurSkinIndex(0);
        }
        async void onChangeLanguage(int languageIndex)
        {
            string language = game.langManager.getLanguages()[languageIndex];
            var skinGroup = getSkins().GroupBy(s => s.skinId).First(g => g.Key == _curCardSkin.skinId);
            var targetSkin = skinGroup.FirstOrDefault(s => s.language == language);
            if (targetSkin == null)
            {
                //不存在对应语言的皮肤，新建一个
                targetSkin = new CardSkinData(_curCardSkin)
                {
                    language = language
                };
                scene.editor.addSkin(targetSkin);
            }
            _curCardSkin = targetSkin;
            _ui.setLanguage(languageIndex);
            _ui.setName(_curCardSkin.name);
            _ui.setSubName(_curCardSkin.subname);
            _ui.setDesc(_curCardSkin.desc);
            Sprite image = await game.card.loadExternalImage(_curCardSkin.imagePath,
                game.card.getCardPoolPathRootDir(game.card.getCardPoolPath(game.card.getCardPool(_cardPoolId))));
            if (image != null)
            {
                _ui.setImage(image);
            }
        }
        /// <summary>
        /// 当用户更改类型
        /// </summary>
        /// <param name="typeIndex"></param>
        void onChangeType(int typeIndex)
        {
            _cardDefine.type = _types[typeIndex];
            //_ui.setType(typeIndex);
        }
        private void onChangeCharacter(int characterIndex)
        {
            if (characterIndex < 1)
            {
                _cardDefine.setProp(DEFINE.CHARACTER_ID, 0);
            }
            else
            {
                _cardDefine.setProp(DEFINE.CHARACTER_ID, _selectableCharacters[characterIndex - 1]);
            }
        }
        /// <summary>
        /// 当用户更改花费
        /// </summary>
        /// <param name="cost"></param>
        void onChangeCost(string cost)
        {
            if (int.TryParse(cost, out int value))
            {
                _cardDefine.setProp(DEFINE.COST, value);
                _ui.setCost(cost);
            }
        }
        /// <summary>
        /// 当用户更改卡名
        /// </summary>
        /// <param name="name"></param>
        void onChangeName(string name)
        {
            _curCardSkin.name = name;
            _ui.setName(name);
        }
        /// <summary>
        /// 当用户更改卡副名
        /// </summary>
        /// <param name="subName"></param>
        void onChangeSubName(string subName)
        {
            _curCardSkin.subname = subName;
            _ui.setSubName(subName);
        }
        /// <summary>
        /// 当需要用户选择图片，根据平台不同调用不同的插件方法，返回选择的贴图。新建的贴图需要缓存在内存中。
        /// </summary>
        Task<Sprite> onNeedSelectImage()
        {
            TaskCompletionSource<Sprite> tcs = new TaskCompletionSource<Sprite>();
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                //手机端
                var permission = NativeGallery.GetImageFromGallery(async path =>
                {
                    if (path != null)
                    {
                        Sprite sprite = await game.card.getSpriteFromCache(path);
                        if (sprite == null)
                        {
                            Texture2D texture = NativeGallery.LoadImageAtPath(path, markTextureNonReadable: false);
                            sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * .5f);
                            //缓存外部图片
                            game.card.addSpriteToCache(path, sprite);
                        }
                        //更改贴图路径为外部文件路径
                        _curCardSkin.imagePath = path;
                        tcs.SetResult(sprite);
                    }
                });
                if (permission == NativeGallery.Permission.Denied)
                {
                    Debug.LogError("没有打开相册的权限");
                }
            }
            else
            {
                //默认PC
                string[] paths = StandaloneFileBrowser.OpenFilePanel("", Application.persistentDataPath, new ExtensionFilter[]
                {
                    new ExtensionFilter("Image Files", "png","jpg")
                }, false);
                if (paths == null || paths.Length < 1)
                {
                    return null;
                }
                string path = paths[0];
                UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(path);
                uwr.SendWebRequest().completed += async op =>
                {
                    if (uwr.isNetworkError || uwr.isHttpError)
                    {
                        Debug.LogError(uwr.error);
                    }
                    else
                    {
                        Sprite sprite = await game.card.getSpriteFromCache(path);
                        if (sprite == null)
                        {
                            Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                            sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * .5f);
                            //缓存外部图片
                            game.card.addSpriteToCache(path, sprite);
                        }
                        //更改贴图路径为外部文件路径
                        _curCardSkin.imagePath = path;
                        tcs.SetResult(sprite);
                    }
                    uwr.Dispose();
                };
            }
            return tcs.Task;
        }
        /// <summary>
        /// 当用户更改卡片描述
        /// </summary>
        /// <param name="desc"></param>
        void onChangeDesc(string desc)
        {
            _curCardSkin.desc = desc;
            _ui.setDesc(desc);
        }
        /// <summary>
        /// 当用户更改攻击力
        /// </summary>
        /// <param name="attack"></param>
        void onChangeAttack(string attack)
        {
            if (int.TryParse(attack, out int value))
            {
                _cardDefine.setProp(DEFINE.ATTACK, value);
                _ui.setAttack(attack);
            }
        }
        /// <summary>
        /// 当用户更改生命值
        /// </summary>
        /// <param name="life"></param>
        void onChangeLife(string life)
        {
            if (int.TryParse(life, out int value))
            {
                _cardDefine.setProp(DEFINE.LIFE, value);
                _ui.setLife(life);
            }
        }
        /// <summary>
        /// 当用户更改标签
        /// </summary>
        void onChangeTag(int index)
        {
            if (index > 0)
            {
                _cardDefine.setProp(DEFINE.TAGS, new string[] { _tags[index] });
                _ui.setTag(index);
            }
            else
            {
                _cardDefine.setProp(DEFINE.TAGS, new string[0]);
                _ui.setTag(0);
            }
        }
        /// <summary>
        /// 当点击编辑效果按钮，打开编辑效果界面。
        /// </summary>
        private void onClickEditEffectButton(int index)
        {
            GeneratedEffect effect = _cardDefine.getEffects().OfType<GeneratedEffect>().ElementAt(index);
            ActionNodeGraphInfo graphInfo = scene.editor.actionNodeGraphInfoList.Find(g => g.cardId == _cardDefine.id && g.effectIndex == index);
            if (graphInfo == null)
            {
                graphInfo = new ActionNodeGraphInfo(_cardDefine.id, index);
                scene.editor.actionNodeGraphInfoList.Add(graphInfo);
            }
            scene.effectEditor.displayEffect(_cardPoolId, _cardDefine, effect, graphInfo);
        }
        private void onClickRemoveEffectButton(int index)
        {
            GeneratedEffect[] prevEffects = _cardDefine.getEffects().OfType<GeneratedEffect>().ToArray();
            GeneratedEffect[] newEffects = new GeneratedEffect[prevEffects.Length - 1];
            for (int i = 0; i < index; i++)
            {
                newEffects[i] = prevEffects[i];
            }
            for (int i = index; i < newEffects.Length; i++)
            {
                newEffects[i] = prevEffects[i + 1];
            }
            _cardDefine.setEffects(newEffects);
            updateEffectList(newEffects);
        }
        /// <summary>
        /// 当点击添加效果按钮，为卡牌定义添加一个新的空效果，并刷新效果列表。
        /// </summary>
        void onClickAddEffectMenu(int index)
        {
            GeneratedEffect[] effects;
            if (index == 1)
            {
                effects = _cardDefine.getEffects().OfType<GeneratedEffect>().Append(new GeneratedHalo()).ToArray();
            }
            else
            {
                effects = _cardDefine.getEffects().OfType<GeneratedEffect>().Append(new GeneratedEffect()).ToArray();
            }
            _cardDefine.setEffects(effects);
            updateEffectList(effects);
        }
        private void onClickBackgroundButton()
        {
            //返回上一个界面
            gameObject.SetActive(false);
            scene.editor.display(false, false, -1, false);
            //保存卡牌编辑信息
        }
        private void onClickDeleteCardButton()
        {
            //删除当前卡牌
            game.card.removeExternalCard(_cardDefine);
            //返回上个界面
            gameObject.SetActive(false);
            scene.editor.display(false, false, -1, true);
        }
        #endregion
        [Obsolete]
        public IAppManager app { get; }
        [Obsolete]
        public MainCtrl main { get; }
        CardEditorPanel _ui;
        string[] _types;
        string[] _tags;
        int _cardPoolId;
        CardDefine _cardDefine;
        CardSkinData _curCardSkin;
        CardDefine[] _selectableCharacters;
    }
}
