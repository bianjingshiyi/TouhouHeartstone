using System;
using TouhouCardEngine;

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
        /// 整个PVE流程的种子
        /// </summary>
        public int runSeed;
        /// <summary>
        /// 当前层的种子
        /// </summary>
        public int stageSeed;
        public int currentStage;
        public int currentHealth;
        public DefineReference character;
        public DefineReference[] deck;
        public void init()
        {
            var rng = new Random(initSeed);
            runSeed = rng.Next();
            generateStageSeed();
        }
        public void advanceStage()
        {
            currentStage++;
            generateStageSeed();
        }
        private void generateStageSeed()
        {
            var rng = new Random(runSeed);
            stageSeed = rng.Next();
            runSeed = rng.Next();
        }
    }
    [Serializable]
    public class SerializableSaveData
    {
        public SerializableSaveData(SaveData data)
        {
            version = data.version;
            initSeed = data.initSeed;
            runSeed = data.runSeed;
            stageSeed = data.stageSeed;
            currentStage = data.currentStage;
            currentHealth = data.currentHealth;
            character = data.character;
            deck = data.deck;
        }
        public SaveData ToSaveData()
        {
            return new SaveData()
            {
                version = version,
                initSeed = initSeed,
                runSeed = runSeed,
                stageSeed = stageSeed,
                currentStage = currentStage,
                currentHealth = currentHealth,
                character = character,
                deck = deck
            };
        }
        public int version;
        public int initSeed;
        public int runSeed;
        public int stageSeed;
        public int currentStage;
        public int currentHealth;
        public DefineReference character;
        public DefineReference[] deck;
    }

    public static class RandomHelper
    {
        public static int getNextSeed(this int seed)
        {
            var rng = new Random(seed);
            return rng.Next();
        }
    }
}
