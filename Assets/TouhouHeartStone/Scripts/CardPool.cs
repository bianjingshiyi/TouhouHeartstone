using System;
using UI;
using Controllers;
using TouhouCardEngine;

namespace Game
{
    [Serializable]
    public class CardPool
    {
        public CardDefine[] cardDefines;
        public CardSkinData[] cardSkins;
        public ActionNodeGraphInfo[] actionNodeGraphInfos;
        public GeneratedBuffDefine[] buffDefines;
        public BuffSkin[] buffSkins;
    }
}
