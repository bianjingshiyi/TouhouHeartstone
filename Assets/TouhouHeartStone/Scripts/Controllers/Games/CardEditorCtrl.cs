using System.Collections.Generic;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using BJSYGameCore;
using System;
using System.Linq;
using Game;

namespace Controllers
{
    partial class CardEditorCtrl : IController<MainCtrl>
    {
        public CardEditorCtrl(IAppManager app, MainCtrl main, CardEditorPanel compo)
        {
            this.app = app;
            this.main = main;
            _compo = compo;
            _types = new string[] { DEFINE.SERVANT, DEFINE.SPELL, DEFINE.ITEM };
            _compo.setTypes(_types);
            _compo.onChangeSkin += onChangeSkin;
            _compo.onChangeLanguage += onChangeLanguage;
            _compo.onChangeType += onChangeType;
            _compo.onChangeCost += onChangeCost;
            _compo.onChangeName += onChangeName;
            _compo.onChangeSubName += onChangeSubName;
            _compo.onSelectImage += onSelectImage;
            _compo.onChangeDesc += onChangeDesc;
            _compo.onChangeAttack += onChangeAttack;
            _compo.onChangeLife += onChangeLife;
            _compo.onChangeTag += onChangeTag;
            _compo.onClickEditEffectButton += onClickEditEffectButton;
            _compo.onClickAddEffectButton += onClickAddEffectButton;
        }
        /// <summary>
        /// 打开一张卡片进行编辑
        /// </summary>
        public void displayCard(CardDefine cardDefine, CardSkinData[] cardSkins)
        {
            _cardDefine = cardDefine;
            _cardSkinList.Clear();
            _cardSkinList.AddRange(cardSkins);
            _compo.setSkinNames(_cardSkinList.GroupBy(s => s.skinId)
                .Select(g => g.FirstOrDefault(s => s.language == app.langManager.getLanguage()) is CardSkinData s2 ? s2.name : g.First().name)
                .ToArray());
            setCurSkinIndex(0);
            //TODO:更换卡牌卡框
            _compo.setType(Array.IndexOf(_types, _cardDefine.type));
            _compo.setCost(_cardDefine.getProp<int>(DEFINE.COST).ToString());
            _compo.setAttack(_cardDefine.getProp<int>(DEFINE.ATTACK).ToString());
            _compo.setLife(_cardDefine.getProp<int>(DEFINE.LIFE).ToString());
            string[] tags = _cardDefine.getProp<string[]>(DEFINE.TAGS);
            if (tags != null && tags.Length > 0)
                _compo.setTag(tags[0]);
            else
                _compo.setTag(string.Empty);
            GeneratedEffect[] effects = cardDefine.getEffects().OfType<GeneratedEffect>().ToArray();
            updateEffectList(effects);
        }
        void setCurSkinIndex(int skinIndex)
        {
            var skinGroup = _cardSkinList.GroupBy(s => s.skinId).ElementAt(skinIndex);
            //获取对应语言的皮肤
            _curCardSkin = skinGroup.FirstOrDefault(s => s.language == app.langManager.getLanguage());
            //如果没有对应语言，就第一个
            if (_curCardSkin == null)
                _curCardSkin = skinGroup.First();
            _compo.setSkin(skinIndex);
            _compo.setLanguage(Array.IndexOf(app.langManager.getLanguages(), _curCardSkin.language));
            //TODO:更换卡牌卡框
            _compo.setName(_curCardSkin.name);
            _compo.setSubName(_curCardSkin.subname);
            _compo.setImage(_curCardSkin.imagePath);
            _compo.setDesc(_curCardSkin.desc);
        }
        void updateEffectList(GeneratedEffect[] effects)
        {
            string[] effectNames = new string[effects.Length];
            for (int i = 0; i < effectNames.Length; i++)
            {
                effectNames[i] = app.langManager.getString(StringTable.EDITOR_EDIT_EFFECT) + i;
            }
            _compo.setEffects(effectNames);
        }
        #region UI回调
        void onChangeSkin(int skinIndex)
        {
            setCurSkinIndex(skinIndex);
        }
        void onChangeLanguage(int languageIndex)
        {
            string language = app.langManager.getLanguages()[languageIndex];
            var skinGroup = _cardSkinList.GroupBy(s => s.skinId).First(g => g.Key == _curCardSkin.skinId);
            var targetSkin = skinGroup.FirstOrDefault(s => s.language == language);
            if (targetSkin == null)
            {
                //不存在对应语言的皮肤，新建一个
                targetSkin = new CardSkinData(_curCardSkin)
                {
                    language = language
                };
            }
            _curCardSkin = targetSkin;
            _compo.setLanguage(languageIndex);
            _compo.setName(_curCardSkin.name);
            _compo.setSubName(_curCardSkin.subname);
            _compo.setImage(_curCardSkin.imagePath);
            _compo.setDesc(_curCardSkin.desc);
        }
        /// <summary>
        /// 当用户更改类型
        /// </summary>
        /// <param name="typeIndex"></param>
        void onChangeType(int typeIndex)
        {
            _cardDefine.type = _types[typeIndex];
            _compo.setType(typeIndex);
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
                _compo.setCost(cost);
            }
        }
        /// <summary>
        /// 当用户更改卡名
        /// </summary>
        /// <param name="name"></param>
        void onChangeName(string name)
        {
            _curCardSkin.name = name;
            _compo.setName(name);
        }
        /// <summary>
        /// 当用户更改卡副名
        /// </summary>
        /// <param name="subName"></param>
        void onChangeSubName(string subName)
        {
            _curCardSkin.name = subName;
            _compo.setSubName(subName);
        }
        /// <summary>
        /// 当用户选择完图片，更换卡图。如果选择的是外置图片，那么会在保存的时候将图片复制到相应路径，反之则不会。
        /// </summary>
        /// <param name="path"></param>
        void onSelectImage(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                _curImagePath = path;
                _compo.setImage(_curImagePath);
            }
        }
        /// <summary>
        /// 当用户更改卡片描述
        /// </summary>
        /// <param name="desc"></param>
        void onChangeDesc(string desc)
        {
            _curCardSkin.desc = desc;
            _compo.setDesc(desc);
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
                _compo.setAttack(attack);
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
                _compo.setLife(life);
            }
        }
        /// <summary>
        /// 当用户更改标签
        /// </summary>
        /// <param name="tag"></param>
        void onChangeTag(string tag)
        {
            if (string.IsNullOrEmpty(tag))
            {
                _cardDefine.setProp(DEFINE.TAGS, new string[0]);
                _compo.setTag(string.Empty);
            }
            else
            {
                _cardDefine.setProp(DEFINE.TAGS, new string[] { tag });
                _compo.setTag(tag);
            }
        }
        /// <summary>
        /// 当点击编辑效果按钮，打开编辑效果界面。
        /// </summary>
        void onClickEditEffectButton(int index)
        {
            GeneratedEffect effect = _cardDefine.getEffects().OfType<GeneratedEffect>().ElementAt(index);
            main.lobbySceneCtrl.effectEditorCtrl.displayEffect(effect);
        }
        /// <summary>
        /// 当点击添加效果按钮，为卡牌定义添加一个新的空效果，并刷新效果列表。
        /// </summary>
        void onClickAddEffectButton()
        {
            GeneratedEffect[] effects = _cardDefine.getEffects().OfType<GeneratedEffect>().Append(new GeneratedEffect()).ToArray();
            _cardDefine.setEffects(effects);
            updateEffectList(effects);
        }
        #endregion
        public IAppManager app { get; }
        public MainCtrl main { get; }
        CardEditorPanel _compo;
        string[] _types;
        CardDefine _cardDefine;
        readonly List<CardSkinData> _cardSkinList = new List<CardSkinData>();
        CardSkinData _curCardSkin;
        string _curImagePath;
    }
}
