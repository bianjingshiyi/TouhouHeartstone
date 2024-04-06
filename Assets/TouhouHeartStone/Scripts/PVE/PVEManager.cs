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
using PVE;
using TouhouCardEngine;
using TouhouCardEngine.Shared;
using TouhouHeartstone;
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
                plugins = new List<CardGem>(),
                currentStage = 1,
                version = VERSION_NUMBER
            };
            saveData.init();
            return saveData;
        }
        public async Task saveData(string dataName, SaveData data)
        {
            var serializable = new SerializableSaveData(data);
            var path = getPersistentSavePath(dataName);
            await main.fileManager.savePersistentFile(path, serializable.toBytes());
        }
        public async Task<SaveData> loadData(string dataName)
        {
            if (!hasData(dataName))
                return null;
            var path = getPersistentSavePath(dataName);
            var bytes = await main.fileManager.readPersistentBinaryFile(path);
            var seri = JsonHelper.fromBytes<SerializableSaveData>(bytes);
            return seri.ToSaveData();
        }
        #endregion

        #region PVE玩家生成卡池
        public void createGeneratedCardPool(string dataName, out CardPoolInfo info, out CardPool pool)
        {
            main.card.createCardPool(out info, out pool);
            info.dependencyRefs = new CardPoolReference[]
            {
                main.card.getLocalCardPoolInfo(CardManager.STANDARD_POOL_ID).ToReference(),
                main.card.getLocalCardPoolInfo(CardManager.PVE_POOL_ID).ToReference(),
            };
            info.texturesDir = _generatedTexturesPath;
            info.cardPoolName = main.lang._(DEFAULT_GEN_POOL_NAME);
            _pvePoolDict.Add(dataName, info);
        }
        public async Task<bool> saveGeneratedCardPool(string dataName)
        {
            var (info, _) = await loadGeneratedCardPool(dataName);
            return await saveGeneratedCardPool(dataName, info);
        }
        public async Task<bool> saveGeneratedCardPool(string dataName, CardPoolInfo info)
        {
            var cardManager = main.card;
            var path = getCardPoolFilePath(dataName);
            return await cardManager.saveCardPool(info, path);
        }
        public async Task<bool> saveGeneratedCardPool(string dataName, CardPoolInfo info, CardPool pool)
        {
            var cardManager = main.card;
            var path = getCardPoolFilePath(dataName);
            var dir = Path.GetDirectoryName(path);
            var texDir = Path.Combine(dir, _generatedTexturesPath);
            Directory.CreateDirectory(texDir);
            return await cardManager.saveCardPool(info, pool, path);
        }
        public async Task<(CardPoolInfo info, CardPool pool)> loadGeneratedCardPool(string dataName)
        {
            if (_pvePoolDict.TryGetValue(dataName, out var poolInfo))
            {
                return await main.card.getOrLoadCardPool(poolInfo);
            }
            else
            {
                var path = getCardPoolFilePath(dataName);
                var (info, pool) = await main.card.loadLocalCardPool(path);
                _pvePoolDict.Add(dataName, info);
                return (info, pool);
            }
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
            deck.masterCardRef = CardManager.getClassicCardRef(ClassicCardID.Patchouli);

            deck.addCard(CardManager.getClassicCardRef(ClassicCardID.ElementSprite));
            deck.addCard(CardManager.getClassicCardRef(ClassicCardID.MultiCast));
            deck.addCard(CardManager.getClassicCardRef(ClassicCardID.TheGreatLibrary));
            deck.addCard(CardManager.getClassicCardRef(ClassicCardID.LibraryProtector));
            deck.addCard(CardManager.getClassicCardRef(ClassicCardID.BestMagic));
            deck.addCard(CardManager.getClassicCardRef(ClassicCardID.AgniShine));
            deck.addCard(CardManager.getClassicCardRef(ClassicCardID.MetalFatigue));
            deck.addCard(CardManager.getClassicCardRef(ClassicCardID.SylphyHorn));
            deck.addCard(CardManager.getClassicCardRef(ClassicCardID.PhilosopherStone));
            deck.addCard(CardManager.getClassicCardRef(ClassicCardID.MegaReaver));

            // 将PVE卡池中，第一个拥有插槽的卡牌加入到套牌中。
            var (_, pvePool) = await getOrLoadPVECardPool();
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
        public async Task<StageData> generateStageData(int stage, int stageSeed)
        {
            var rng = new RNG(stageSeed);
            return new StageData()
            {
                boss = await generateNextBoss(stage, rng.next()),
                cardRewards = generateCardRewards(rng.next()),
                pluginRewards = await generatePluginRewards(rng.next()),
                fightSeed = rng.next(),
            };
        }
        #endregion
        public CardGem createGemByDefineReference(SaveData saveData, DefineReference defRef)
        {
            var poolRef = main.card.getLocalCardPoolReference(defRef.cardPoolId);
            return createGemByCardDefine(saveData, main.card.getCardDefine(poolRef, defRef.defineId));
        }
        public static CardGem createGemByCardDefine(SaveData saveData, CardDefine define)
        {
            var tagString = define.getProp<string>(CARD_PROP_GEM);
            if (tagString == null)
                return null;
            var gem = new CardGem(saveData.gemId, define.getDefineRef());
            var tags = tagString.Split(',');
            gem.setTags(tags);
            saveData.gemId++;
            return gem;
        }

        public static bool cardDefineIsGem(CardDefine define)
        {
            return define.getProp<string>(CARD_PROP_GEM) != null;
        }
        public static bool cardDefineHasSlots(CardDefine define)
        {
            return define.getProp<string[]>(CARD_PROP_SLOTS) != null;
        }
        public static GeneratedEffect getSlotEffect(CardDefine define, string slotTagStr)
        {
            var tags = slotTagStr.Split(",");
            return getSlotEffect(define, tags);
        }
        public static GeneratedEffect getSlotEffect(CardDefine define, string[] tags)
        {
            return define.getGeneratedEffects().FirstOrDefault(e => isValidEffectForSlot(define, tags, e));
        }
        public static bool isValidEffectForSlot(CardDefine define, string[] slotTags, GeneratedEffect effect)
        {
            if (slotTags.Contains(GEMTAG.SERVANT)) // 只能是仆从牌。
            {
                if (!define.isServant())
                    return false;
            }
            else if (slotTags.Contains(GEMTAG.SPELL)) // 只能是法术牌。
            {
                if (!define.isSpell())
                    return false;
            }


            if (slotTags.Contains(GEMTAG.ACTIVE)) // 只能是主动效果
            {
                if (effect is not GeneratedActiveEffect genActive)
                    return false;
                if (slotTags.Contains(GEMTAG.PLAYED)) // 只能是宣言
                {
                    if (!effect.hasTag(EFFECTTAG.ACTIVE_PLAYED))
                        return false;
                }
                if (slotTags.Contains(GEMTAG.TARGET_SERVANT)) // 只能是可以选择仆从的主动效果
                {
                    if (genActive.targetCheckers == null || !genActive.targetCheckers.All(tc => tc.targetType == DEFINE.TARGET_TYPE_SERVANT))
                        return false;
                }
                else if (slotTags.Contains(GEMTAG.TARGET)) // 只能是可以选择目标的主动效果
                {
                    if (genActive.targetCheckers == null)
                        return false;
                }
            }
            else if (slotTags.Contains(GEMTAG.PASSIVE)) // 只能是被动效果
            {
                if (effect is not GeneratedPassiveEffect)
                    return false;
                if (slotTags.Contains(GEMTAG.LASTWORD)) // 只能是遗言
                {
                    if (!effect.hasTag(EFFECTTAG.PASSIVE_LASTWORD))
                        return false;
                }
            }
            else if (slotTags.Contains(GEMTAG.AURA)) // 只能是光环效果
            {
                if (effect is not GeneratedAura)
                    return false;
            }
            return true;
        }
        public static CardGemSlotGroup[] getCardDefineSlots(SaveData saveData, CardDefine define)
        {
            var slotProps = define.getProp<string[]>(CARD_PROP_SLOTS);
            if (slotProps == null)
                return null;
            var slotGroups = slotProps.Select((tagStr, index) => (index, tagStr)).GroupBy(p => getSlotEffect(define, p.tagStr));
            // 分组。
            var effectList = define.getGeneratedEffects();
            var groupList = new List<CardGemSlotGroup>();

            // 获取这张卡上插着的宝石。
            var insertedGems = getCardDefineInsertedGems(saveData, define);
            foreach (var group in slotGroups)
            {
                int count = group.Count();

                // 创建组。
                var effect = group.Key;
                var effectIndex = Array.IndexOf(effectList, effect);
                var gemGroup = new CardGemSlotGroup(effectIndex, count);

                // 向组内添加插槽。
                for (int i = 0; i < count; i++)
                {
                    var propItem = group.ElementAt(i);
                    var slotTagStr = propItem.tagStr;
                    var slotIndex = propItem.index;

                    var slotTags = slotTagStr.Split(',');
                    var slot = new CardGemSlot(slotIndex, slotTags);
                    var gem = insertedGems[slotIndex];
                    if (gem != null)
                    {
                        slot.setGem(gem);
                    }
                    gemGroup.setSlotAt(i, slot);
                }

                // 将组加到列表中。
                groupList.Add(gemGroup);
            }
            return groupList.ToArray();
        }
        public static CardGem[] getCardDefineInsertedGems(SaveData data, CardDefine define)
        {
            var gems = getOrCreateCardDefineInsertedGemProp(define);
            if (gems == null)
                return null;
            return gems.Select(id => data.plugins.FirstOrDefault(p => p.id == id)).ToArray();
        }
        public static void setCardDefineInsertedGem(CardDefine define, int index, CardGem gem)
        {
            var gems = getOrCreateCardDefineInsertedGemProp(define);
            if (gems == null)
                return;
            gems[index] = gem?.id ?? 0;
        }
        public async Task<(CardPoolInfo info, CardPool pool)> getOrLoadPVECardPool()
        {
            var poolRef = main.card.getLocalCardPoolReference(CardManager.PVE_POOL_ID);
            return await main.card.getOrLoadCardPool(poolRef);
        }
        public string getCardPoolFileDirectory()
        {
            return Path.Combine(Application.persistentDataPath, _directory);
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
            return Path.Combine(getCardPoolFileDirectory(), dataName, $"{_cardPoolFileName}.{main.card.cardPoolFilePostfix}");
        }
        private async Task<PVECharacter> generateNextBoss(int stage, int seed)
        {
            var (info, pvePool) = await getOrLoadPVECardPool();
            var rng = new RNG(seed);
            var master = pvePool.cardDefineList.Where(d => d.getProp<int>(CARD_PROP_BOSS_STAGE) == stage).random(rng);
            if (master == null)
            {
                return new PVECharacter()
                {
                    description = "占位符",
                    deck = new Deck(CardManager.getClassicCardRef(ClassicCardID.Sanae), new DefineReference[0])
                };
            }
            var deck = new Deck(master.getDefineRef(), master.getProp<DefineReference[]>(CARD_PROP_BOSS_DECK));
            return new PVECharacter()
            {
                description = "占位符",
                deck = deck
            };
        }
        private async Task<DefineReference[]> generatePluginRewards(int seed)
        {
            var (info, pvePool) = await getOrLoadPVECardPool();
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
                CardManager.getClassicCardRef(ClassicCardID.ElementSprite),
                CardManager.getClassicCardRef(ClassicCardID.MultiCast),
                CardManager.getClassicCardRef(ClassicCardID.TheGreatLibrary),
                CardManager.getClassicCardRef(ClassicCardID.LibraryProtector),
                CardManager.getClassicCardRef(ClassicCardID.BestMagic),
                CardManager.getClassicCardRef(ClassicCardID.PhilosopherStone),
                CardManager.getClassicCardRef(ClassicCardID.BiteTentacle),
                CardManager.getClassicCardRef(ClassicCardID.ArcaneKnowledge),
                CardManager.getClassicCardRef(ClassicCardID.AgniShine),
                CardManager.getClassicCardRef(ClassicCardID.PrincessUndine),
                CardManager.getClassicCardRef(ClassicCardID.SylphyHorn),
                CardManager.getClassicCardRef(ClassicCardID.MetalFatigue),
                CardManager.getClassicCardRef(ClassicCardID.TrilithonShake),
                CardManager.getClassicCardRef(ClassicCardID.Koakuma),
                CardManager.getClassicCardRef(ClassicCardID.MegaReaver),
                CardManager.getClassicCardRef(ClassicCardID.RoyalFlare),
                CardManager.getClassicCardRef(ClassicCardID.SilentSelene),
            };
            var rng = new RNG(seed);
            return new DefineReference[][]
            {
                list.randomTake(rng, 3).ToArray()
            };
        }
        private static int[] getOrCreateCardDefineInsertedGemProp(CardDefine define)
        {
            var slotProps = define.getProp<string[]>(CARD_PROP_SLOTS);
            if (slotProps == null)
                return null;
            var gems = define.getProp<int[]>(CARD_PROP_INSERTED_GEMS);
            if (gems == null)
            {
                gems = new int[slotProps.Length];
                define.setProp(CARD_PROP_INSERTED_GEMS, gems);
            }
            else if (gems.Length != slotProps.Length)
            {
                var newGems = new int[slotProps.Length];
                gems.CopyTo(newGems, 0);
                gems = newGems;
                define.setProp(CARD_PROP_INSERTED_GEMS, gems);
            }
            return gems;
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
        public string directory => _directory;
        private MainManager _main;
        [SerializeField]
        private string _directory = "pve";
        [SerializeField]
        private string _saveFileName = "main";
        [SerializeField]
        private string _cardPoolFileName = "cardpool";
        [SerializeField]
        private string _generatedTexturesPath = "Textures";
        private Dictionary<string, CardPoolInfo> _pvePoolDict = new Dictionary<string, CardPoolInfo>();
        private const int VERSION_NUMBER = 0;

        public const string SAVE_FILE_EXTENSION = "dds";
        public const string DEFAULT_PVE_DATA_NAME = "default";
        public const string CARD_PROP_SLOTS = "插槽";
        public const string CARD_PROP_GEM = "插件";
        public const string CARD_PROP_BOSS_STAGE = "Boss层数";
        public const string CARD_PROP_BOSS_DECK = "Boss套牌";
        public const string CARD_PROP_TEMPLATE_DEFINE = "模板卡牌";
        public const string CARD_PROP_INSERTED_GEMS = "插入的插件";
        #endregion
    }
}
