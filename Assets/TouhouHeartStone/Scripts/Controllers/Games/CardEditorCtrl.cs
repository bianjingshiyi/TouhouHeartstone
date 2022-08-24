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
            _ui.setType(Array.IndexOf(_types, _cardDefine.type), _cardDefine.type);
            _ui.setIsToken(_cardDefine.isToken());
            bool[] keywordsValue = new bool[keywords.Length];
            for (int i = 0; i < keywordsValue.Length; i++)
            {
                keywordsValue[i] = _cardDefine.hasKeyword(keywords[i]);
            }
            _ui.setKeywords(keywordsValue);
            bool[] tagsValue = new bool[_tags.Length];
            for (int i = 0; i < _tags.Length; i++)
            {
                tagsValue[i] = _cardDefine.getTags().Contains(_tags[i]);
            }
            _ui.setInitTags(_cardDefine.getTags());
            string[] charactersName = new string[selectableCharacters.Length + 1];
            charactersName[0] = StringTable.CARDVIEW_CHARACTER_NEUTRAL;
            for (int i = 1; i < charactersName.Length; i++)
            {
                charactersName[i] = game.card.getCardSkin(selectableCharacters[i - 1].id).name;
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
                int index = Array.IndexOf(_races, tags[0]);
                if (index < 0)
                    index = 0;
                _ui.setRace(index);
            }
            else
                _ui.setRace(0);
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
            _types = new string[] { DEFINE.SERVANT, DEFINE.SPELL, DEFINE.ITEM, DEFINE.MASTER, DEFINE.SKILL };
            string[] typeNames = new string[]
            {
                StringTable.GAME_CARD_TYPE_SERVANT,
                StringTable.GAME_CARD_TYPE_SPELL,
                StringTable.GAME_CARD_TYPE_ITEM,
                StringTable.GAME_CARD_TYPE_MASTER,
                StringTable.GAME_CARD_TYPE_SKILL,

            };
            _ui.initTypes(typeNames);
            _ui.onChangeSkin += onChangeSkin;
            _ui.onChangeSkinName += onChangeSkinName;
            _ui.onClickDeleteSkinButton += onDeleteSkin;
            string[] languages = game.langManager.getLanguages();
            for (int i = 0; i < languages.Length; i++)
            {
                languages[i] = game.langManager.getString(game.ui.getTranslateNameForLanguage(languages[i]));
            }
            _ui.initLanguages(languages);
            _ui.onChangeLanguage += onChangeLanguage;
            _ui.initKeywords(Array.ConvertAll(keywordNames, s => game.langManager.getString(s)));
            _ui.onChangeKeywords += onChangeKeywords;
            _ui.onChangeType += onChangeType;
            _ui.onIsTokenToggleValueChanged += onIsTokenToggleValueChanged;
            string[] tagNames = new string[]
            {
                StringTable.GAME_TAG_METAL,
                StringTable.GAME_TAG_WOOD,
                StringTable.GAME_TAG_WATER,
                StringTable.GAME_TAG_FIRE,
                StringTable.GAME_TAG_EARTH,
            };
            _tags = new string[]
            {
                Patchouli.METAL,
                Patchouli.WOOD,
                Patchouli.WATER,
                Patchouli.FIRE,
                Patchouli.EARTH
            };
            _ui.initTags(tagNames);
            _ui.onTagDropdownValueChanged += onTagDropdownValueChanged;
            _ui.onChangeCharacter += onChangeCharacter;
            _ui.onChangeCost += onChangeCost;
            _ui.onChangeName += onChangeName;
            _ui.onChangeSubName += onChangeSubName;
            _ui.onNeedSelectImage += onNeedSelectImage;
            _ui.onChangeDesc += onChangeDesc;
            _ui.onChangeAttack += onChangeAttack;
            _ui.onChangeLife += onChangeLife;
            _races = new string[]
            {
                null,
                TAG.BEAST,
                TAG.DEMON,
                TAG.ELEMENT,
                TAG.FAIRY,
                TAG.GOD,
                TAG.MACHINE
            };
            string[] racesName = new string[]
            {
                string.Empty,
                game.langManager.getString(StringTable.GAME_RACE_BEAST),
                game.langManager.getString(StringTable.GAME_RACE_DEMON),
                game.langManager.getString(StringTable.GAME_RACE_ELEMENT),
                game.langManager.getString(StringTable.GAME_RACE_FAIRY),
                game.langManager.getString(StringTable.GAME_RACE_GOD),
                game.langManager.getString(StringTable.GAME_RACE_MACHINE)
            };
            _ui.setTags(racesName);
            _ui.onChangeRace += onChangeRace;
            _ui.onClickEditEffectButton += onClickEditEffectButton;
            _ui.onClickRemoveEffectButton += onClickRemoveEffectButton;
            _ui.setEffectTypes(game.card.getEffectNames());
            _ui.onClickAddEffectMenu += onClickAddEffectMenu;
            _ui.onClickBackgroundButton += onClickBackgroundButton;
            _ui.onClickDeleteCardButton += onClickDeleteCardButton;
            _ui.onChangeTag += onChangeTag;
        }
        #endregion
        #region 事件回调
        private async void onChangeSkin(int skinIndex)
        {
            if (skinIndex == getSkinCount())
            {
                scene.editor.hasUnsavedChanges = true;
                //新建皮肤
                _curCardSkin = game.card.createSkin(_cardPoolId, _cardDefine.id);
                updateSkinNames();
            }
            await setCurSkinIndex(skinIndex);
        }
        private async void onChangeSkinName(string skinName)
        {
            scene.editor.hasUnsavedChanges = true;
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
            scene.editor.hasUnsavedChanges = true;
            var skinGroup = getSkins().GroupBy(c => c.skinId).ToList();
            if (skinGroup.Count < 2)
            {
                Debug.LogWarning("你不能删除卡牌的最后一个皮肤");
                return;
            }
            game.card.removeSkin(_cardPoolId, _cardDefine.id, _curCardSkin.skinId);
            //scene.editor.removeSkins(c => c.skinId == _curCardSkin.skinId);
            updateSkinNames();
            await setCurSkinIndex(0);
        }
        private async void onChangeLanguage(int languageIndex)
        {
            string language = game.langManager.getLanguages()[languageIndex];
            var skinGroup = getSkins().GroupBy(s => s.skinId).First(g => g.Key == _curCardSkin.skinId);
            var targetSkin = skinGroup.FirstOrDefault(s => s.language == language);
            if (targetSkin == null)
            {
                scene.editor.hasUnsavedChanges = true;
                //不存在对应语言的皮肤，新建一个
                targetSkin = game.card.createSkinForDiffLanguage(_cardPoolId, _curCardSkin, language);
            }
            _curCardSkin = targetSkin;
            _ui.setLanguage(languageIndex);
            _ui.setName(_curCardSkin.name);
            _ui.setSubName(_curCardSkin.subname);
            _ui.setDesc(_curCardSkin.desc);
            string path = _curCardSkin.imagePath;
            Sprite image = await game.card.loadImage(_curCardSkin.imagePath, game.card.getCardPoolOfCard(_cardDefine) is CardPoolInfo cardPoolInfo ? cardPoolInfo.cardPoolId : 0);
            _ui.setImage(image);
        }
        /// <summary>
        /// 当用户更改类型
        /// </summary>
        /// <param name="typeIndex"></param>
        private void onChangeType(int typeIndex)
        {
            scene.editor.hasUnsavedChanges = true;
            _cardDefine.type = _types[typeIndex];
            _ui.setType(typeIndex, _types[typeIndex]);
        }
        private void onIsTokenToggleValueChanged(bool value)
        {
            scene.editor.hasUnsavedChanges = true;
            _cardDefine.setIsToken(value);
        }
        private void onTagDropdownValueChanged(bool[] value)
        {
            scene.editor.hasUnsavedChanges = true;
            string[] tags = _cardDefine.getTags();
            string race = tags.Length > 0 ? tags[0] : null;
            List<string> tagList = new List<string>();
            tagList.Add(race);
            for (int i = 0; i < _tags.Length; i++)
            {
                if (value[i] && !tagList.Contains(_tags[i]))
                {
                    tagList.Add(_tags[i]);
                }
                else
                {
                    tagList.RemoveAll(s => s == _tags[i]);
                }
            }
            _cardDefine.setTags(tagList.ToArray());
        }
        /// <summary>
        /// 当用户更改卡牌的关键词
        /// </summary>
        /// <param name="value"></param>
        private void onChangeKeywords(bool[] value)
        {
            scene.editor.hasUnsavedChanges = true;
            List<string> keywordList = new List<string>();
            for (int i = 0; i < keywords.Length; i++)
            {
                if (value[i])
                {
                    keywordList.Add(keywords[i]);
                }
            }
            _cardDefine.setKeywords(keywordList.ToArray());
        }
        private void onChangeCharacter(int characterIndex)
        {
            scene.editor.hasUnsavedChanges = true;
            if (characterIndex < 1)
            {
                _cardDefine.setCharacterID(0);
            }
            else
            {
                _cardDefine.setCharacterID(_selectableCharacters[characterIndex - 1].id);
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
                scene.editor.hasUnsavedChanges = true;
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
            scene.editor.hasUnsavedChanges = true;
            _curCardSkin.name = name;
            _ui.setName(name);
        }
        /// <summary>
        /// 当用户更改卡副名
        /// </summary>
        /// <param name="subName"></param>
        void onChangeSubName(string subName)
        {
            scene.editor.hasUnsavedChanges = true;
            _curCardSkin.subname = subName;
            _ui.setSubName(subName);
        }
        /// <summary>
        /// 当需要用户选择图片，根据平台不同调用不同的插件方法，返回选择的贴图。新建的贴图需要缓存在内存中。
        /// </summary>
        async Task<Sprite> onNeedSelectImage()
        {
            Sprite sprite = await game.card.selectExternalImage();
            if (sprite != null)
            {
                scene.editor.hasUnsavedChanges = true;
                _curCardSkin.imagePath = game.card.getSpritePath(sprite);
            }
            return sprite;
        }
        /// <summary>
        /// 当用户更改卡片描述
        /// </summary>
        /// <param name="desc"></param>
        void onChangeDesc(string desc)
        {
            scene.editor.hasUnsavedChanges = true;
            _curCardSkin.desc = desc;
            _ui.setDesc(desc);
        }
        /// <summary>
        /// 当用户更改攻击力
        /// </summary>
        /// <param name="attack"></param>
        private void onChangeAttack(string attack)
        {
            if (int.TryParse(attack, out int value))
            {
                scene.editor.hasUnsavedChanges = true;
                _cardDefine.setProp(DEFINE.ATTACK, value);
                _ui.setAttack(attack);
            }
        }
        /// <summary>
        /// 当用户更改生命值
        /// </summary>
        /// <param name="life"></param>
        private void onChangeLife(string life)
        {
            if (int.TryParse(life, out int value))
            {
                scene.editor.hasUnsavedChanges = true;
                _cardDefine.setProp(DEFINE.LIFE, value);
                _ui.setLife(life);
            }
        }
        /// <summary>
        /// 当用户更改标签
        /// </summary>
        private void onChangeRace(int index)
        {
            scene.editor.hasUnsavedChanges = true;
            string[] value = _cardDefine.getTags();
            if (value == null || value.Length < 1)
            {
                value = new string[1];
            }
            value[0] = index > 0 ? _races[index] : null;
            _cardDefine.setTags(value);
            _ui.setRace(index);
        }

        private void onChangeTag(string text)
        {
            _cardDefine.setTags(text.Split(',', ' ', '，'));
        }
        /// <summary>
        /// 当点击编辑效果按钮，打开编辑效果界面。
        /// </summary>

        public EffectEditorPanel _effectEditorPanel;//画布组件的引用
        private void onClickEditEffectButton(int index)
        {
            _effectEditorPanel.LoadBackGroundInfo();
            GeneratedEffect effect = _cardDefine.getEffects().OfType<GeneratedEffect>().ElementAt(index);
            scene.effectEditor.display(_cardPoolId, _cardDefine, effect);
        }
        private void onClickRemoveEffectButton(int index)
        {
            scene.editor.hasUnsavedChanges = true;
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
        private void onClickAddEffectMenu(int index)
        {
            scene.editor.hasUnsavedChanges = true;
            GeneratedEffect[] effects = _cardDefine.getEffects().OfType<GeneratedEffect>().Append(game.card.createEffectByIndex(index)).ToArray();
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
            scene.editor.hasUnsavedChanges = true;
            //删除当前卡牌
            game.card.removeCard(_cardDefine);
            //返回上个界面
            gameObject.SetActive(false);
            scene.editor.display(false, false, -1, true);
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
            Sprite image = await game.card.loadImage(_curCardSkin.imagePath, game.card.getCardPoolOfCard(_cardDefine) is CardPoolInfo cardPoolInfo ? cardPoolInfo.cardPoolId : 0);
            _ui.setImage(image);
        }
        private int getSkinCount()
        {
            return getSkins().GroupBy(s => s.skinId).Count();
        }
        private void updateEffectList(GeneratedEffect[] effects)
        {
            _ui.setEffects(game.card.getEffectNames(effects));
        }
        private IEnumerable<CardSkinData> getSkins()
        {
            return game.card.getAllSkins(_cardDefine.id);
        }
        [Obsolete]
        public IAppManager app { get; }
        [Obsolete]
        public MainCtrl main { get; }
        CardEditorPanel _ui;
        string[] _types;
        public static readonly string[] keywords = new string[]
        {
            KEYWORD.TAUNT,
            KEYWORD.CHARGE,
            KEYWORD.RUSH,
            KEYWORD.SHIELD,
            KEYWORD.STEALTH,
            KEYWORD.PERMANENTSTEALTH,
            KEYWORD.DRAIN,
            KEYWORD.POISONOUS,
            KEYWORD.ELUSIVE,
            KEYWORD.FREEZED,
            KEYWORD.UNIQUE,
            KEYWORD.AUTOCAST,
            KEYWORD.CANTATTACK,
            KEYWORD.WINDFURY,
            KEYWORD.IMMUNITY,
            KEYWORD.SECRET,
            KEYWORD.MISSION
        };
        public static readonly string[] keywordNames = new string[]
        {
            StringTable.GAME_KEYWORD_TAUNT,
            StringTable.GAME_KEYWORD_CHARGE,
            StringTable.GAME_KEYWORD_RUSH,
            StringTable.GAME_KEYWORD_SHIELD,
            StringTable.GAME_KEYWORD_STEALTH,
            StringTable.GAME_KEYWORD_PERMANENT_STEALTH,
            StringTable.GAME_KEYWORD_DRAIN,
            StringTable.GAME_KEYWORD_POISONOUS,
            StringTable.GAME_KEYWORD_ELUSIVE,
            StringTable.GAME_KEYWORD_FREEZE,
            StringTable.GAME_KEYWORD_UNIQUE,
            StringTable.GAME_KEYWORD_AUTOCAST,
            StringTable.GAME_KEYWORD_CANTATTACK,
            StringTable.GAME_KEYWORD_WINDFURY,
            StringTable.GAME_KEYWORD_IMMUNITY,
            StringTable.GAME_KEYWORD_SECRET,
            StringTable.GAME_KEYWORD_MISSION
        };
        string[] _races;
        string[] _tags;
        int _cardPoolId;
        CardDefine _cardDefine;
        CardSkinData _curCardSkin;
        CardDefine[] _selectableCharacters;
    }
}
