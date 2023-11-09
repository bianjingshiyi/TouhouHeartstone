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
        public int currentStage;
        public List<DefineReference> plugins;
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
        public DefineReference getCharacter()
        {
            return deck.masterCardRef;
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
            deck = data.deck.toDefRefArray(true);
            plugins = data.plugins?.ToArray() ?? Array.Empty<DefineReference>();
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
                plugins = plugins.ToList()
            };
        }
        public int version;
        public int initSeed;
        public SerializableRNG runRNG;
        public int stageSeed;
        public int currentStage;
        public int stageState;
        public DefineReference[] deck;
        public DefineReference[] plugins;
    }
    public enum PVEStageState
    {
        Main,
        SelectCard
    }
}
