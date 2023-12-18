using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MukioI18n;
using TouhouHeartstone;

namespace Game
{
    partial class CardManager
    {
        #region 公有方法

        #region 添加
        public void addKeywordInfo(string key, string name, string desc)
        {
            addKeywordInfo(key, name, desc, typeof(bool));
        }
        public void addKeywordInfo(string key, string name, string desc, Type type)
        {
            keywordInfos.Add(key, new KeywordInfo()
            {
                name = name,
                desc = desc,
                key = key,
                type = type
            });
        }
        #endregion

        #region 获取
        public bool tryGetKeywordInfo(string keyword, out KeywordInfo info)
        {
            return keywordInfos.TryGetValue(keyword, out info);
        }
        public KeywordInfo getKeywordInfo(string keyword)
        {
            if (tryGetKeywordInfo(keyword, out var keywordInfo))
            {
                return keywordInfo;
            }
            return null;
        }
        public void getKeywordTexts(string[] keywords, out string[] names, out string[] descs)
        {
            int count = keywords?.Length ?? 0;
            names = new string[count];
            descs = new string[count];
            for (int i = 0; i < count; i++)
            {
                var keyword = keywords[i];
                if (keyword == null)
                    continue;
                names[i] = getKeywordName(keyword);
                descs[i] = getKeywordDesc(keyword);
            }
        }
        public string getKeywordName(string keyword)
        {
            if (tryGetKeywordInfo(keyword, out var info))
            {
                return game.lang._(info.name);
            }
            return game.lang._(KEYWORD_NAME_UNKNOWN);
        }
        public string getKeywordDesc(string keyword)
        {
            if (tryGetKeywordInfo(keyword, out var info))
            {
                return game.lang._(info.desc);
            }
            return game.lang._(KEYWORD_DESC_UNKNOWN);
        }
        public string[] getKeywordsInDescription(string desc)
        {
            List<string> keywords = new List<string>();
            foreach (var keyword in keywordInfos.Keys)
            {
                var name = getKeywordName(keyword);
                var pattern = game.lang._(KEYWORD_MATCH_PATTERN, name);
                if (Regex.IsMatch(desc, pattern))
                {
                    keywords.Add(keyword);
                }
            }
            return keywords.ToArray();
        }
        #endregion

        #endregion

        #region 私有方法

        private void onAwake_Keywords()
        {
            loadBuiltinKeywords();
        }

        private void loadBuiltinKeywords()
        {
            // 属性关键词
            addKeywordInfo(KEYWORD.TAUNT, StringTable.GAME_KEYWORD_TAUNT, KEYWORD_DESC_TAUNT);
            addKeywordInfo(KEYWORD.CHARGE, StringTable.GAME_KEYWORD_CHARGE, KEYWORD_DESC_CHARGE);
            addKeywordInfo(KEYWORD.RUSH, StringTable.GAME_KEYWORD_RUSH, KEYWORD_DESC_RUSH);
            addKeywordInfo(KEYWORD.SHIELD, StringTable.GAME_KEYWORD_SHIELD, KEYWORD_DESC_SHIELD);
            addKeywordInfo(KEYWORD.STEALTH, StringTable.GAME_KEYWORD_STEALTH, KEYWORD_DESC_STEALTH);
            addKeywordInfo(KEYWORD.PERMANENTSTEALTH, StringTable.GAME_KEYWORD_PERMANENT_STEALTH, KEYWORD_DESC_PERMANENTSTEALTH);
            addKeywordInfo(KEYWORD.DRAIN, StringTable.GAME_KEYWORD_DRAIN, KEYWORD_DESC_DRAIN);
            addKeywordInfo(KEYWORD.POISONOUS, StringTable.GAME_KEYWORD_POISONOUS, KEYWORD_DESC_POISONOUS);
            addKeywordInfo(KEYWORD.ELUSIVE, StringTable.GAME_KEYWORD_ELUSIVE, KEYWORD_DESC_ELUSIVE);
            addKeywordInfo(KEYWORD.UNIQUE, StringTable.GAME_KEYWORD_UNIQUE, KEYWORD_DESC_UNIQUE);
            addKeywordInfo(KEYWORD.AUTOCAST, StringTable.GAME_KEYWORD_AUTOCAST, KEYWORD_DESC_AUTOCAST);
            addKeywordInfo(KEYWORD.CANTATTACK, StringTable.GAME_KEYWORD_CANTATTACK, KEYWORD_DESC_CANTATTACK);
            addKeywordInfo(KEYWORD.WINDFURY, StringTable.GAME_KEYWORD_WINDFURY, KEYWORD_DESC_WINDFURY);
            addKeywordInfo(KEYWORD.IMMUNITY, StringTable.GAME_KEYWORD_IMMUNITY, KEYWORD_DESC_IMMUNITY);
            addKeywordInfo(KEYWORD.MISSION, StringTable.GAME_KEYWORD_MISSION, KEYWORD_DESC_MISSION);
            addKeywordInfo(KEYWORD.PERMANENT_FROZEN, StringTable.GAME_KEYWORD_PERMANENT_FROZEN, KEYWORD_DESC_PERMANENT_FROZEN);
            addKeywordInfo(KEYWORD.UNPLAYABLE, KEYWORD_NAME_UNPLAYABLE, KEYWORD_DESC_UNPLAYABLE);
            addKeywordInfo(DEFINE.SPELL_DAMAGE, StringTable.GAME_CARD_PROP_NAME_SPELL_DAMAGE, KEYWORD_DESC_SPELL_DAMAGE, typeof(int));
            addKeywordInfo(DEFINE.DAMAGE_REDUCE, StringTable.GAME_CARD_PROP_NAME_DAMAGE_REDUCE, KEYWORD_DESC_DAMAGE_REDUCE, typeof(int));

            // 效果关键词
            addKeywordInfo(KEYWORD.BATTLECRY, StringTable.GAME_KEYWORD_BATTLECRY, KEYWORD_DESC_BATTLECRY);
            addKeywordInfo(KEYWORD.DEATHRATTLE, StringTable.GAME_KEYWORD_DEATHRATTLE, KEYWORD_DESC_DEATHRATTLE);
            addKeywordInfo(KEYWORD.TRAP, StringTable.GAME_KEYWORD_SECRET, KEYWORD_DESC_TRAP);
            addKeywordInfo(KEYWORD.CHARGED, KEYWORD_NAME_CHARGED, KEYWORD_DESC_CAN_BE_CHARGED);

            // 动作关键词
            addKeywordInfo(KEYWORD.SHACKLE, KEYWORD_NAME_SHACKLE, KEYWORD_DESC_FROZEN);
            addKeywordInfo(KEYWORD.SEAL, KEYWORD_NAME_SEAL, KEYWORD_DESC_SEAL);
            addKeywordInfo(KEYWORD.FAITHDUEL, KEYWORD_NAME_FAITHDUEL, KEYWORD_DESC_FAITHDUEL);
        }

        #endregion

        #region 属性字段

        #region 关键词描述

        [TranslateMsg("用于捕获描述内关键字的正则表达式，{0}为关键词名称。")]
        public const string KEYWORD_MATCH_PATTERN = @"<b>{0}((：.*)?|（.*）?|\+[0-9]+)<\/b>";

        [TranslateMsg("关键词名称-未知")]
        public const string KEYWORD_NAME_UNKNOWN = "未知关键词";
        [TranslateMsg("关键词描述-未知")]
        public const string KEYWORD_DESC_UNKNOWN = "不知道这是干什么的……";

        [TranslateMsg("关键词名称-蓄力")]
        public const string KEYWORD_NAME_CHARGED = "蓄力";
        [TranslateMsg("关键词名称-禁锢")]
        public const string KEYWORD_NAME_SHACKLE = "禁锢";
        [TranslateMsg("关键词名称-封印")]
        public const string KEYWORD_NAME_SEAL = "封印";
        [TranslateMsg("关键词名称-信仰对决")]
        public const string KEYWORD_NAME_FAITHDUEL = "信仰对决";
        [TranslateMsg("关键词名称-无法使用")]
        public const string KEYWORD_NAME_UNPLAYABLE = "无法使用";

        [TranslateMsg("关键词描述-守卫")]
        public const string KEYWORD_DESC_TAUNT = "敌人必须优先攻击具有<b>守卫</b>的角色。";
        [TranslateMsg("关键词描述-先攻")]
        public const string KEYWORD_DESC_CHARGE = "可以立即发起攻击。";
        [TranslateMsg("关键词描述-速攻")]
        public const string KEYWORD_DESC_RUSH = "可以立即对仆从发起攻击。";
        [TranslateMsg("关键词描述-护盾")]
        public const string KEYWORD_DESC_SHIELD = "阻挡一次受到的伤害。";
        [TranslateMsg("关键词描述-隐匿")]
        public const string KEYWORD_DESC_STEALTH = "直到发起一次攻击，无法成为敌人的目标。";
        [TranslateMsg("关键词描述-永久隐匿")]
        public const string KEYWORD_DESC_PERMANENTSTEALTH = "无法成为敌人的目标。";
        [TranslateMsg("关键词描述-吸血")]
        public const string KEYWORD_DESC_DRAIN = "造成伤害后，为你的英雄恢复等量生命值。";
        [TranslateMsg("关键词描述-致命")]
        public const string KEYWORD_DESC_POISONOUS = "对仆从造成伤害后，直接将其消灭。";
        [TranslateMsg("关键词描述-魔免")]
        public const string KEYWORD_DESC_ELUSIVE = "无法成为法术或英雄技能的目标。";
        [TranslateMsg("关键词描述-禁锢")]
        public const string KEYWORD_DESC_FROZEN = "被<b>禁锢</b>的角色失去下一次攻击机会。";
        [TranslateMsg("关键词描述-唯一")]
        public const string KEYWORD_DESC_UNIQUE = "只能在套牌中携带一张<b>唯一</b>卡牌。";
        [TranslateMsg("关键词描述-自动施放")]
        public const string KEYWORD_DESC_AUTOCAST = "抽到该牌后立即施放，并抽一张牌。";
        [TranslateMsg("关键词描述-无法攻击")]
        public const string KEYWORD_DESC_CANTATTACK = "无法发起攻击。";
        [TranslateMsg("关键词描述-双击")]
        public const string KEYWORD_DESC_WINDFURY = "一回合内可以发起两次攻击。";
        [TranslateMsg("关键词描述-无敌")]
        public const string KEYWORD_DESC_IMMUNITY = "无法受到伤害或成为敌人的目标。";
        [TranslateMsg("关键词描述-陷阱")]
        public const string KEYWORD_DESC_TRAP = "在敌方回合内因某些条件触发后，产生某些效果。";
        [TranslateMsg("关键词描述-委托")]
        public const string KEYWORD_DESC_MISSION = "必定出现在初始手牌中。完成某些条件后，产生某些效果。";
        [TranslateMsg("关键词描述-宣言")]
        public const string KEYWORD_DESC_BATTLECRY = "使用该牌后，产生某些效果。";
        [TranslateMsg("关键词描述-遗言")]
        public const string KEYWORD_DESC_DEATHRATTLE = "在仆从死亡或道具被摧毁后，产生某些效果。";
        [TranslateMsg("关键词描述-永久禁锢")]
        public const string KEYWORD_DESC_PERMANENT_FROZEN = "失去所有攻击机会。";
        [TranslateMsg("关键词描述-无法使用")]
        public const string KEYWORD_DESC_UNPLAYABLE = "无法从手牌中被主动使用。";
        [TranslateMsg("关键词描述-蓄力")]
        public const string KEYWORD_DESC_CAN_BE_CHARGED = "你的回合结束后，为手牌中所有灵力值消耗小于或等于你灵力珠数量的<b>蓄力</b>牌进行蓄力。";
        [TranslateMsg("关键词描述-法术伤害")]
        public const string KEYWORD_DESC_SPELL_DAMAGE = "你的法术额外造成伤害。";
        [TranslateMsg("关键词描述-减伤")]
        public const string KEYWORD_DESC_DAMAGE_REDUCE = "受到的伤害减少。";
        [TranslateMsg("关键词描述-封印")]
        public const string KEYWORD_DESC_SEAL = "被<b>封印</b>的角色失去所有强化效果。";
        [TranslateMsg("关键词描述-信仰对决")]
        public const string KEYWORD_DESC_FAITHDUEL = "揭示双方牌库里的一张牌，灵力值消耗更高的一方赢得<b>信仰对决</b>。";
        #endregion

        private Dictionary<string, KeywordInfo> keywordInfos = new Dictionary<string, KeywordInfo>();
        #endregion
    }
    public class KeywordInfo
    {
        public string name;
        public string desc;
        public string key;
        public Type type;
    }
}
