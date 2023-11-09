using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using BJSYGameCore;
using Controllers;
using MongoDB.Bson;
using MukioI18n;
using TouhouCardEngine;
using TouhouHeartstone;
using TouhouHeartstone.Builtin;
using UnityEngine;

namespace Game.PVE
{
    public class PVEManager : Manager
    {
        #region 公有方法

        #region 数据文件夹
        public void deleteData(string dataName)
        {
            if (!hasData(dataName))
                return;
            if (_pvePoolDict.TryGetValue(dataName, out var info))
            {
                main.card.removeCardPool(info);
                _pvePoolDict.Remove(dataName);
            }
            var dir = getSaveDataDirectory(dataName);
            Directory.Delete(dir, true);
        }
        public bool hasData(string dataName)
        {
            var dir = getSaveDataDirectory(dataName);
            return Directory.Exists(dir);
        }
        #endregion

        #region PVE数据
        public async Task<SaveData> generateNewSaveData(PVECharacter character)
        {
            var rand = new byte[4];
            RandomNumberGenerator.Create().GetBytes(rand);
            var seed = BitConverter.ToInt32(rand, 0);

            var masterRef = character.getCharacter();
            var poolRef = main.card.getLocalCardPoolReference(masterRef.cardPoolId);
            await main.card.getOrLoadCardPool(poolRef);

            var deck = character.deck;
            var saveData = new SaveData()
            {
                initSeed = seed,
                deck = deck,
                plugins = new List<DefineReference>(),
                currentStage = 0,
                version = VERSION_NUMBER
            };
            saveData.init();
            return saveData;
        }
        public async Task saveData(string dataName, SaveData data)
        {
            var serializable = new SerializableSaveData(data);
            var json = serializable.ToJson();
            var path = getPersistentSavePath(dataName);
            await main.fileManager.savePersistentFile(path, new string[]
            {
                json
            });
        }
        public async Task<SaveData> loadData(string dataName)
        {
            if (!hasData(dataName))
                return null;
            var path = getPersistentSavePath(dataName);
            var json = await main.fileManager.readPersistentTextFile(path, lineCount: 1);
            var seri = JsonHelper.fromJson<SerializableSaveData>(json[0]);
            return seri.ToSaveData();
        }
        #endregion

        #region PVE玩家生成卡池
        public void createGeneratedCardPool(string dataName, out CardPoolInfo info, out CardPool pool)
        {
            main.card.createCardPool(out info, out pool);
            info.cardPoolName = main.lang._(DEFAULT_GEN_POOL_NAME);
            _pvePoolDict.Add(dataName, info);
        }
        public async Task<bool> saveGeneratedCardPool(string dataName, CardPoolInfo info, CardPool pool)
        {
            var cardManager = main.card;
            var path = getCardPoolFilePath(dataName);
            return await cardManager.saveCardPool(info, pool, path);
        }
        public async Task<(CardPoolInfo info, CardPool pool)> loadGeneratedCardPool(string dataName)
        {
            var path = getCardPoolFilePath(dataName);
            var (info, pool) = await main.card.loadCardPool(path);
            _pvePoolDict.Add(dataName, info);
            return (info, pool);
        }
        public async Task<(CardPoolInfo info, CardPool pool)> getGeneratedCardPool(string dataName)
        {
            if (_pvePoolDict.TryGetValue(dataName, out var info))
            {
                return await main.card.getOrLoadCardPool(info);
            }
            return (null, null);
        }
        #endregion

        #region PVE逻辑
        public async Task<PVECharacter[]> getStartingCharacterList()
        {
            List<PVECharacter> characterList = new List<PVECharacter>();
            var deck = new Deck();
            deck.masterCardRef = new DefineReference(0, Patchouli.ID);

            deck.addCard(new DefineReference(0, ElementSprite.ID));
            deck.addCard(new DefineReference(0, MultiCast.ID));
            deck.addCard(new DefineReference(0, TheGreatLibrary.ID));
            deck.addCard(new DefineReference(0, LibraryProtector.ID));
            deck.addCard(new DefineReference(0, BestMagic.ID));
            deck.addCard(new DefineReference(0, AgniShine.ID));
            deck.addCard(new DefineReference(0, MetalFatigue.ID));
            deck.addCard(new DefineReference(0, SylphyHorn.ID));
            deck.addCard(new DefineReference(0, PhilosopherStone.ID));
            deck.addCard(new DefineReference(0, MegaReaver.ID));

            // 将PVE卡池中，第一个拥有插槽的卡牌加入到套牌中。
            var pvePoolRef = main.card.getLocalCardPoolReference(CardManager.PVE_POOL_ID);
            var (_, pvePool) = await main.card.getOrLoadCardPool(pvePoolRef);
            if (pvePool != null)
            {
                var define = pvePool.cardDefineList.FirstOrDefault(d => cardDefineHasSlots(d));
                if (define != null)
                {
                    deck.addCard(new DefineReference(define.cardPoolId, define.id));
                }
            }
            characterList.Add(new PVECharacter()
            {
                deck = deck,
                description = main.lang._(PVE_PATCHOULI_DESC)
            });
            return characterList.ToArray();
        }
        public async Task<StageData> generateStageData(int stageSeed)
        {
            var rng = new RNG(stageSeed);
            return new StageData()
            {
                boss = generateNextBoss(rng.next()),
                cardRewards = generateCardRewards(rng.next()),
                pluginRewards = await generatePluginRewards(rng.next()),
                fightSeed = rng.next(),
            };
        }
        #endregion

        public static bool cardDefineIsGem(CardDefine define)
        {
            return define.getProp<string>(CARD_PROP_GEM) != null;
        }
        public static CardGem getCardDefineGem(CardDefine define)
        {
            var tagString = define.getProp<string>(CARD_PROP_GEM);
            if (tagString == null)
                return null;
            var gem = new CardGem(define.getDefineRef());
            var tags = tagString.Split(',');
            gem.setTags(tags);
            return gem;
        }
        public static bool cardDefineHasSlots(CardDefine define)
        {
            return define.getProp<string[]>(CARD_PROP_SLOTS) != null;
        }
        public static CardGemSlotGroup[] getCardDefineSlots(CardDefine define)
        {
            var slotProps = define.getProp<string[]>(CARD_PROP_SLOTS);
            if (slotProps == null)
                return null;
            var slotGroups = slotProps.GroupBy(tags => tags);
            var groupList = new List<CardGemSlotGroup>();
            foreach (var group in slotGroups)
            {
                int count = group.Count();
                var gemGroup = new CardGemSlotGroup(count);
                var tags = group.Key.Split(',');
                gemGroup.setTags(tags);
                groupList.Add(gemGroup);
            }
            return groupList.ToArray();
        }
        #endregion
        #region 私有方法
        private string getSaveDataDirectory(string dataName)
        {
            return Path.Combine(Application.persistentDataPath, _directory, dataName);
        }
        private string getPersistentSavePath(string dataName)
        {
            return Path.Combine(_directory, dataName, $"{_saveFileName}.{SAVE_FILE_EXTENSION}");
        }
        private string getCardPoolFilePath(string dataName)
        {
            return Path.Combine(Application.persistentDataPath, _directory, dataName, $"{_cardPoolFileName}.{main.card.cardPoolFilePostfix}");
        }
        private PVECharacter generateNextBoss(int seed)
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
        private async Task<DefineReference[]> generatePluginRewards(int seed)
        {
            var poolRef = main.card.getLocalCardPoolReference(CardManager.PVE_POOL_ID);
            var (info, pvePool) = await main.card.getOrLoadCardPool(poolRef);
            if (pvePool != null)
            {
                var rng = new RNG(seed);
                return pvePool.cardDefineList.Where(d => cardDefineIsGem(d)).randomTake(rng, 2).Select(d => new DefineReference(d.cardPoolId, d.id)).ToArray();
            }
            return Array.Empty<DefineReference>();
        }
        private DefineReference[][] generateCardRewards(int seed)
        {
            List<DefineReference> list = new List<DefineReference>()
            {
                new DefineReference(0, ElementSprite.ID),
                new DefineReference(0, MultiCast.ID),
                new DefineReference(0, TheGreatLibrary.ID),
                new DefineReference(0, LibraryProtector.ID),
                new DefineReference(0, BestMagic.ID),
                new DefineReference(0, PhilosopherStone.ID),
                new DefineReference(0, BiteTentacle.ID),
                new DefineReference(0, ArcaneKnowledge.ID),
                new DefineReference(0, AgniShine.ID),
                new DefineReference(0, PrincessUndine.ID),
                new DefineReference(0, SylphyHorn.ID),
                new DefineReference(0, MetalFatigue.ID),
                new DefineReference(0, TrilithonShake.ID),
                new DefineReference(0, Koakuma.ID),
                new DefineReference(0, MegaReaver.ID),
                new DefineReference(0, RoyalFlare.ID),
                new DefineReference(0, SilentSelene.ID),
            };
            var rng = new RNG(seed);
            return new DefineReference[][]
            {
                list.randomTake(rng, 3).ToArray()
            };
        }
        #endregion
        #region 属性字段
        [TranslateMsg("PVE模式人物描述-帕秋莉")]
        private const string PVE_PATCHOULI_DESC = "操纵元素之力的魔法使，使用大量的法术向敌人倾泻火力或进行防御。\n通过元素融合还可将法术进行升级，产生更加强大的力量。";
        [TranslateMsg("PVE生成卡池的默认名称")]
        public const string DEFAULT_GEN_POOL_NAME = "PVE生成卡池";
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
        private string _directory = "pve";
        [SerializeField]
        private string _saveFileName = "main";
        [SerializeField]
        private string _cardPoolFileName = "cardpool";
        private Dictionary<string, CardPoolInfo> _pvePoolDict = new Dictionary<string, CardPoolInfo>();
        private const int VERSION_NUMBER = 0;

        public const string SAVE_FILE_EXTENSION = "dds";
        public const string DEFAULT_PVE_DATA_NAME = "default";
        public const string CARD_PROP_SLOTS = "插槽";
        public const string CARD_PROP_GEM = "插件";
        #endregion
    }
}
