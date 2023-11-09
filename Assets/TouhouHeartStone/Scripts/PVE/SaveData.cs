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
        /// 整个PVE流程的种子
        /// </summary>
        public int runSeed;
        /// <summary>
        /// 当前层的种子
        /// </summary>
        public int stageSeed;
        public int currentStage;
        public PVEStageState stageState;
        public Deck deck;
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
        public DefineReference getCharacter()
        {
            return deck.masterCardRef;
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
            stageState = (int)data.stageState;
            deck = data.deck.toDefRefArray(true);
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
                stageState = (PVEStageState)stageState,
                deck = new Deck(deck)
            };
        }
        public int version;
        public int initSeed;
        public int runSeed;
        public int stageSeed;
        public int currentStage;
        public int stageState;
        public DefineReference[] deck;
    }

    public static class RandomHelper
    {
        public static int getNextSeed(this int seed)
        {
            var rng = new Random(seed);
            return rng.Next();
        }
        public static IEnumerable<T> randomTake<T>(this IEnumerable<T> enumrable, int seed, int count)
        {
            List<T> list = enumrable.ToList();
            if (list.Count <= count)
                return list;
            T[] results = new T[count];
            var rng = new Random(seed);
            for (int i = 0; i < results.Length; i++)
            {
                int index = rng.Next(0, list.Count - 1);
                results[i] = list[index];
                list.RemoveAt(index);
            }
            return results;
        }
    }
    public enum PVEStageState
    {
        Main,
        SelectCard
    }
}
