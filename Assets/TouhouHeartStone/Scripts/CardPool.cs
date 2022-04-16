using System;
using UI;
using Controllers;
using TouhouCardEngine;
using System.Collections.Generic;
namespace Game
{
    [Serializable]
    public class CardPool
    {
        public List<CardDefine> cardDefineList = new List<CardDefine>();
        public List<CardSkinData> cardSkinList = new List<CardSkinData>();
        public List<ActionNodeGraphInfo> actionNodeGraphInfoList = new List<ActionNodeGraphInfo>();
        public List<GeneratedBuffDefine> buffDefineList = new List<GeneratedBuffDefine>();
        public List<BuffSkin> buffSkinList = new List<BuffSkin>();
        public List<SerializableActionDefine> actionDefineList = new List<SerializableActionDefine>();
    }
}
