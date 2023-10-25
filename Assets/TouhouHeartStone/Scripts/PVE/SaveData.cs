using System;
using TouhouCardEngine;

namespace Game.PVE
{
    public class SaveData
    {
        public int version;
        public int seed;
        public int currentStage;
        public int currentHealth;
        public DefineReference character;
        public DefineReference[] deck;
    }
    [Serializable]
    public class SerializableSaveData
    {
        public SerializableSaveData(SaveData data)
        {
            version = data.version;
            seed = data.seed;
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
                seed = seed,
                currentStage = currentStage,
                currentHealth = currentHealth,
                character = character,
                deck = deck
            };
        }
        public int version;
        public int seed;
        public int currentStage;
        public int currentHealth;
        public DefineReference character;
        public DefineReference[] deck;
    }
}
