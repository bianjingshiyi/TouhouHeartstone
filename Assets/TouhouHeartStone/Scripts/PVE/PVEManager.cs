using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using BJSYGameCore;
using Controllers;
using MongoDB.Bson;
using TouhouCardEngine;
using TouhouHeartstone;
using TouhouHeartstone.Builtin;
using UnityEngine;

namespace Game.PVE
{
    public class PVEManager : Manager
    {
        #region 公有方法
        public async Task<SaveData> generateNewSaveData(PVECharacter character)
        {
            var rand = new byte[4];
            RandomNumberGenerator.Create().GetBytes(rand);
            var seed = BitConverter.ToInt32(rand, 0);

            var masterRef = character.getCharacter();
            var poolRef = main.card.getLocalCardPoolReference(masterRef.cardPoolId);
            await main.card.getOrLoadCardPool(poolRef);
            var masterDefine = main.card.getCardDefine(poolRef, masterRef.defineId);

            var deck = character.deck;
            var saveData = new SaveData()
            {
                initSeed = seed,
                deck = deck,
                currentHealth = masterDefine.getLife(),
                currentStage = 0,
                version = VERSION_NUMBER
            };
            saveData.init();
            return saveData;
        }
        public PVECharacter generateNextBoss(int seed)
        {
            var deck = new Deck();
            deck.masterCardRef = new DefineReference(0, Sanae.ID);

            deck.addCard(new DefineReference(0, LittleShrine.ID));
            deck.addCard(new DefineReference(0, BumperCrop.ID));
            deck.addCard(new DefineReference(0, GuestStarsMiracle.ID));
            deck.addCard(new DefineReference(0, OpenSeaMiracle.ID));
            deck.addCard(new DefineReference(0, FlyingSnake.ID));
            deck.addCard(new DefineReference(0, GodSummon.ID));
            deck.addCard(new DefineReference(0, ManipulatedGaintFrog.ID));
            deck.addCard(new DefineReference(0, OrochiOfAncientTimes.ID));
            deck.addCard(new DefineReference(0, MoriyaSuwako.ID));
            deck.addCard(new DefineReference(0, YasakaKanako.ID));
            return new PVECharacter()
            {
                description = "占位符",
                deck = deck
            };
        }
        public DefineReference[] generateNextSelectingCards(int seed)
        {
            List<DefineReference> list = new List<DefineReference>()
            {
                new DefineReference(0, LittleShrine.ID),
                new DefineReference(0, BumperCrop.ID),
                new DefineReference(0, GuestStarsMiracle.ID),
                new DefineReference(0, OpenSeaMiracle.ID),
                new DefineReference(0, FlyingSnake.ID),
                new DefineReference(0, GodSummon.ID),
                new DefineReference(0, ManipulatedGaintFrog.ID),
                new DefineReference(0, OrochiOfAncientTimes.ID),
                new DefineReference(0, MoriyaSuwako.ID),
                new DefineReference(0, YasakaKanako.ID)
            };
            return list.randomTake(seed, 3).ToArray();
        }
        public async Task saveData(SaveData data)
        {
            var serializable = new SerializableSaveData(data);
            var json = serializable.ToJson();
            var path = getSavePath();
            await main.fileManager.savePersistentFile(path, new string[]
            {
                json
            });
        }
        public void deleteData()
        {
            if (!hasData())
                return;
            var path = getSavePath();
            main.fileManager.deletePersistentFile(path);
        }
        public bool hasData()
        {
            var path = getSavePath();
            return main.fileManager.isPersistentFileExist(path);
        }
        public async Task<SaveData> loadData()
        {
            if (!hasData())
                return null;
            var path = getSavePath();
            var json = await main.fileManager.readPersistentTextFile(path, lineCount: 1);
            var seri = JsonHelper.fromJson<SerializableSaveData>(json[0]);
            return seri.ToSaveData();
        }
        #endregion
        #region 私有方法
        private string getSavePath()
        {
            return _savePath;
        }
        #endregion
        #region 属性字段
        public MainManager main
        {
            get
            {
                if (!_main)
                {
                    _main = TransformHelper.findComponentInScene<MainManager>(gameObject.scene);
                }
                return _main;
            }
        }
        private MainManager _main;
        [SerializeField]
        private string _savePath = "pve.dds";
        private const int VERSION_NUMBER = 0;
        #endregion
    }
}
