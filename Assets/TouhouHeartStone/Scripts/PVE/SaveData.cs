using System;
using System.Collections.Generic;
using System.Linq;
using TouhouCardEngine;
using TouhouHeartstone;

namespace Game.PVE
{
    public class SaveData
    {
        public int version;
        /// <summary>
        /// 初始种子
        /// </summary>
        public int initSeed;
        /// <summary>
        /// 整个PVE流程的随机数生成器
        /// </summary>
        public RNG runRNG;
        /// <summary>
        /// 当前层的种子
        /// </summary>
        public int stageSeed;
        /// <summary>
        /// 下一个插件的ID。
        /// </summary>
        public int gemId = 1;
        public int currentStage;
        public List<CardGem> plugins;
        public List<int> usedGems = new List<int>();
        public PVEStageState stageState;
        public Deck deck;
        public void init()
        {
            runRNG = new RNG(initSeed);
            newStageSeed();
        }
        public void advanceStage()
        {
            currentStage++;
            newStageSeed();
        }
        public CardGem getGem(int gemId)
        {
            return plugins.FirstOrDefault(p => p.id == gemId);
        }
        public CardGem[] getInventoryGems()
        {
            return plugins.Where(p => !usedGems.Contains(p.id)).ToArray();
        }
        public void addUsedGem(int id)
        {
            usedGems.Add(id);
        }
        public bool removeUsedGem(int id)
        {
            return usedGems.Remove(id);
        }
        
        private void newStageSeed()
        {
            stageSeed = runRNG.next();
        }
    }
    [Serializable]
    public class SerializableSaveData
    {
        public SerializableSaveData(SaveData data)
        {
            version = data.version;
            initSeed = data.initSeed;
            runRNG = new SerializableRNG(data.runRNG);
            stageSeed = data.stageSeed;
            currentStage = data.currentStage;
            stageState = (int)data.stageState;
            gemId = data.gemId;
            deck = data.deck.toDefRefArray(true);
            plugins = data.plugins?.Select(p => new SerializableCardGem(p))?.ToArray() ?? Array.Empty<SerializableCardGem>();
            usedGems = data.usedGems.ToArray();
        }
        public SaveData ToSaveData()
        {
            return new SaveData()
            {
                version = version,
                initSeed = initSeed,
                runRNG = runRNG.toRNG(),
                stageSeed = stageSeed,
                currentStage = currentStage,
                stageState = (PVEStageState)stageState,
                deck = new Deck(deck),
                gemId = gemId,
                plugins = plugins.Select(p => p.toGem()).ToList(),
                usedGems = usedGems.ToList(),
            };
        }
        public int version;
        public int initSeed;
        public SerializableRNG runRNG;
        public int stageSeed;
        public int currentStage;
        public int stageState;
        public int gemId;
        public DefineReference[] deck;
        public SerializableCardGem[] plugins;
        public int[] usedGems;
    }
    public enum PVEStageState
    {
        Main,
        SelectCard
    }
}
