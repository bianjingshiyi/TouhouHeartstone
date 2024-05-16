using System;
using System.Collections.Generic;
using System.Linq;
using Controllers;
using Game.Animations;
using MessagePack;
using MongoDB.Bson.Serialization.Attributes;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;

namespace Game
{
    public class CardPool
    {
        public string originLanguage;
        public List<CardDefine> cardDefineList = new List<CardDefine>();
        public List<CardSkinData> cardSkinList = new List<CardSkinData>();
        public List<GeneratedBuffDefine> buffDefineList = new List<GeneratedBuffDefine>();
        public List<BuffSkin> buffSkinList = new List<BuffSkin>();
        public List<GeneratedActionDefine> actionDefineList = new List<GeneratedActionDefine>();
        public List<GeneratedAnimationDefine> animationDefineList = new List<GeneratedAnimationDefine>();
        public List<EventDefine> eventDefineList = new List<EventDefine>();
        public List<GeneratedRuleLogic> ruleLogicList = new List<GeneratedRuleLogic>();
        public List<LocalizedString> localizedStringList = new List<LocalizedString>();
        public List<LocalizedResource> localizedResourceList = new List<LocalizedResource>();

        public LocalizedString getLocalizedString(string key)
        {
            return localizedStringList.FirstOrDefault(res => res.value == key);
        }
        public LocalizedResource getLocalizedResource(string key)
        {
            return localizedResourceList.FirstOrDefault(res => res.value == key);
        }
    }
}
