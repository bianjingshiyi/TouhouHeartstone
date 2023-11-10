using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game;
using Game.PVE;
using MukioI18n;
using TouhouCardEngine;
using TouhouHeartstone;
using UI;
using UnityEngine;
using Deck = TouhouHeartstone.Deck;
using SaveData = Game.PVE.SaveData;

namespace Controllers
{
    class PVERoomCtrl : GameObjectCtrl<MainSceneCtrl>
    {
        #region 公有方法
        public async Task display()
        {
            await loadCharacterList();
            var dataName = getPVEDataName();
            if (game.pve.hasData(dataName))
            {
                _saveData = await game.pve.loadData(dataName);
                await game.pve.loadGeneratedCardPool(dataName);
                await generateStageData();
            }
            if (_saveData == null)
            {
                await displayPreRunPanel();
            }
            else
            {
                await displayInRunPanel();
            }
            _ui.gameObject.SetActive(true);
        }
        public void openGamePage()
        {
            _ui.openInRunGamePage();
        }
        #endregion

        #region 私有方法
        #region 生命周期
        protected void Awake()
        {
            _ui.onQuitButtonClicked += onClickQuitButtonCallback;
            _ui.onPlayerSelectItemClicked += onPlayerItemSelectedCallback;
            _ui.onCancelSelectPlayer += onCancelSelectPlayerCallback;
            _ui.onGameStartClicked += onGameStartClickedCallback;
            _ui.onFightClicked += onFightClickedCallback;
            _ui.onGiveUpClicked += onGiveUpClickedCallback;
            _ui.onInRunQuitButtonClicked += onInRunQuitClickedCallback;
            _ui.onInventoryButtonClicked += onInventoryClickedCallback;
            _ui.onSelectCardChoiceClicked += onSelectCardChoiceClickedCallback;
            _ui.onConfirmSelectCardClicked += onConfirmSelectCardClickedCallback;
            _ui.onSkipSelectCardClicked += onSkipSelectCardClickedCallback;
            gameManager.onGameFinished += onGameFinishedCallback;
            gameManager.onGameQuit += onGameQuitCallback;
        }
        #endregion
        #region 事件回调
        /// <summary>
        /// 返回主界面
        /// </summary>
        private void onClickQuitButtonCallback()
        {
            quit();
        }
        /// <summary>
        /// 当用户选中了第index个人物
        /// </summary>
        /// <param name="index"></param>
        private async void onPlayerItemSelectedCallback(int index)
        {
            //选择人物
            _selectedCharacter = _characterList[index];
            await displayPlayerInfoPanel(_selectedCharacter);
        }
        private void onCancelSelectPlayerCallback()
        {
            _ui.setPlayerInfoPanelVisible(false);
        }
        private async void onGameStartClickedCallback()
        {
            await createSaveFile();
            await generateStageData();
            await displayInRunPanel();
        }
        private async void onFightClickedCallback()
        {
            await startFight();
        }
        private void onGiveUpClickedCallback()
        {
            showGiveUpDialog();
        }
        private void onInRunQuitClickedCallback()
        {
            quit();
        }
        private async void onInventoryClickedCallback()
        {
            var (_, pvePool) = await game.pve.getOrLoadPVECardPool();
            var cardDefine = pvePool.cardDefineList.FirstOrDefault(d => PVEManager.cardDefineHasSlots(d));
            await _cardEditor.display(getPVEDataName(), _saveData, cardDefine, game.card);
            _ui.openCardEditorPage();
            return;
        }
        private async void onSelectCardChoiceClickedCallback(int index)
        {
            var choices = getNextSelectingCards();
            var selectedDefRef = choices[index];
            _saveData.deck.addCard(selectedDefRef);
            await nextStage();
        }
        private void onConfirmSelectCardClickedCallback()
        {
        }
        private async void onSkipSelectCardClickedCallback()
        {
            await nextStage();
        }
        private async void onGameFinishedCallback(Player[] winners)
        {
            if (!gameManager.isPVE)
                return;
            var localPlayer = gameManager.localPlayer;
            var pve = game.pve;
            if (winners.Contains(localPlayer))
            {
                // 获胜
                _saveData.stageState = PVEStageState.SelectCard;
                _saveData.plugins.AddRange(_stageData.pluginRewards);
                var dataName = getPVEDataName();
                await pve.saveData(dataName, _saveData);
                gameResult = PVEGameResult.Win;
            }
            else if (winners.Length <= 0)
            {
                // 平局
                gameResult = PVEGameResult.Draw;
            }
            else
            {
                // 输掉
                _saveData = null;
                var dataName = getPVEDataName();
                pve.deleteData(dataName);
                gameResult = PVEGameResult.Defeat;
            }
        }
        private async void onGameQuitCallback()
        {
            if (!gameManager.isPVE)
                return;
            switch (gameResult)
            {
                case PVEGameResult.Win:
                case PVEGameResult.Draw:
                    scene.menu.hide();
                    await display();
                    break;
                case PVEGameResult.Defeat:
                    reset();
                    break;
            }
            gameResult = PVEGameResult.None;
        }
        #endregion
        #region 实现方法
        private async Task loadCharacterList()
        {
            _characterList.Clear();
            _characterList.AddRange(await game.pve.getStartingCharacterList());
        }
        #region 游戏内容
        private async Task createSaveFile()
        {
            _saveData = await game.pve.generateNewSaveData(_selectedCharacter);
            _selectedCharacter = null;
            var dataName = getPVEDataName();
            await game.pve.saveData(dataName, _saveData);

            game.pve.createGeneratedCardPool(dataName, out var info, out var cardPool);
            await game.pve.saveGeneratedCardPool(dataName, info, cardPool);
        }
        private void showGiveUpDialog()
        {
            var title = game.lang._(StringTable.HINT_TITLE);
            var content = game.lang._(PVE_CONFIRM_GIVE_UP);
            game.ui.dialog.ShowQuestion(title, content, () =>
            {
                giveUp();
            });
        }
        private async Task startFight()
        {
            await scene.game.network.logic.CreateLocalRoom();
            await scene.game.network.logic.AddAIPlayer();
            RoomData currentRoomData = scene.game.network.logic.CurrentRoom;
            currentRoomData.maxPlayerCount = 2;
            var localPlayer = scene.game.network.logic.localPlayer;

            foreach (var player in currentRoomData.playerDataList)
            {
                Deck deck;
                if (player == localPlayer)
                {
                    deck = _saveData.deck;
                }
                else
                {
                    deck = getNextEnemyCharacter().deck;
                }
                player.setDeck(deck);
            }

            var dataName = getPVEDataName();
            var standardPoolInfo = game.card.getCardPoolInfo(CardManager.STANDARD_POOL_ID);
            var pvePoolInfo = game.card.getCardPoolInfo(CardManager.PVE_POOL_ID);
            var genPoolInfo = await game.pve.getGeneratedCardPool(dataName);
            var cardPools = new CardPoolInfoPack[]
            {
                CardPoolHelper.BuiltinCardPoolInfoPack,
                standardPoolInfo.ToCardPoolInfoPack(),
                pvePoolInfo.ToCardPoolInfoPack(),
                genPoolInfo.info.ToCardPoolInfoPack()
            };
            var seed = _stageData.fightSeed;
            HearthstoneOption option = new HearthstoneOption()
            {
                randomSeed = seed,
                sortedPlayers = currentRoomData.playerDataList.Select(p => p.id).ToArray(),
                cardPools = cardPools
            };
            currentRoomData.setOption(option);

            _ui.gameObject.SetActive(false);
            await gameManager.startPVEGame(currentRoomData);
        }
        private void giveUp()
        {
            var dataName = getPVEDataName();
            game.pve.deleteData(dataName);
            quit();
        }
        private async Task nextStage()
        {
            _saveData.advanceStage();
            _saveData.stageState = PVEStageState.Main;
            var dataName = getPVEDataName();
            await game.pve.saveData(dataName, _saveData);

            await generateStageData();
            await displayInRunPanel();
        }
        private PVECharacter getNextEnemyCharacter()
        {
            return _stageData.boss;
        }
        private DefineReference[] getNextSelectingCards()
        {
            return _stageData.cardRewards[0];
        }
        private async Task generateStageData()
        {
            _stageData = await game.pve.generateStageData(_saveData.currentStage, _saveData.stageSeed);
        }
        private void reset()
        {
            _saveData = null;
            gameResult = PVEGameResult.None;
        }
        private string getPVEDataName()
        {
            return PVEManager.DEFAULT_PVE_DATA_NAME;
        }
        #endregion
        #region UI
        private async Task displayPreRunPanel()
        {
            _ui.setPreStartPanelVisible(true);
            _ui.setInRunPanelVisible(false);
            _ui.setPlayerInfoPanelVisible(false);
            await updatePlayerList();
        }
        private async Task displayPlayerInfoPanel(PVECharacter character)
        {
            var (name, image) = await getCharacterViewData(character);
            var desc = game.lang._(character.description);
            var deckCardsViewData = await getCharacterDeckCardViewData(character);
            _ui.updatePlayerInfoPanel(name, image, desc);
            _ui.updatePlayerInfoPanelDeck(deckCardsViewData);
            _ui.setPlayerInfoPanelVisible(true);
        }
        private async Task displayInRunPanel()
        {
            _ui.setPreStartPanelVisible(false);
            _ui.setInRunPanelVisible(true);
            _ui.openInRunGamePage();

            var deck = _saveData.deck;
            var yourDeckCardsViewData = await getDeckCardViewData(deck);
            _ui.updateYourDeck(yourDeckCardsViewData);

            if (_saveData.stageState == PVEStageState.SelectCard)
            {
                var defRefs = getNextSelectingCards();
                var cardManager = game.card;
                var cardDefines = defRefs.Select(d => cardManager.getLocalCardDefine(d)).ToArray();
                _ui.updateSelectCardPanel(cardDefines, cardManager);
                _ui.switchToSelectCardPanel();
            }
            else
            {
                var character = getNextEnemyCharacter();
                var (name, image) = await getCharacterViewData(character);
                var desc = game.lang._(character.description);
                var deckCardsViewData = await getCharacterDeckCardViewData(character);
                _ui.updateEnemyInfoPanel(name, image, desc);
                _ui.updateEnemyInfoPanelDeck(deckCardsViewData);
                _ui.switchToEnemyInfoPanel();
            }
        }
        /// <summary>
        /// 更新可选择的当前的卡组列表
        /// </summary>
        private async Task updatePlayerList()
        {
            CharacterViewData[] viewData = new CharacterViewData[_characterList.Count];
            for (int i = 0; i < viewData.Length; i++)
            {
                var character = _characterList[i];
                var (name, image) = await getCharacterViewData(character);
                viewData[i].name = name;
                viewData[i].image = image;
            }
            _ui.updatePlayerSelectPanel(viewData);
        }
        private Task<(string name, Sprite image, int count)[]> getCharacterDeckCardViewData(PVECharacter character)
        {
            var deck = character?.deck;
            return getDeckCardViewData(deck);
        }
        private async Task<(string name, Sprite image, int count)[]> getDeckCardViewData(Deck deck)
        {
            (string name, Sprite image, int count)[] deckCardsViewData;
            if (deck != null)
            {
                var cards = deck.cardList.OrderBy(e => game.card.getLocalCardDefine(e.cardRef) is CardDefine cardDefine ? cardDefine.getCost() : 0).ToArray();
                deckCardsViewData = new (string name, Sprite image, int count)[cards.Length];
                for (int i = 0; i < cards.Length; i++)
                {
                    CardDefine cardDefine = game.card.getLocalCardDefine(cards[i].cardRef);
                    CardSkinData skin = cardDefine != null ? game.card.getDefaultSkin(cardDefine) : null;

                    string name;
                    Sprite image;
                    if (skin != null)
                    {
                        name = skin.name;
                        image = await game.card.loadCardImage(cardDefine);
                    }
                    else
                    {
                        name = game.langManager.getString(StringTable.BUILD_UNKNOW_CARD_NAME);
                        image = game.card.getDefaultSprite();
                    }
                    deckCardsViewData[i].name = name;
                    deckCardsViewData[i].image = image;
                    deckCardsViewData[i].count = cards[i].count;
                }
            }
            else
            {
                deckCardsViewData = new (string name, Sprite image, int count)[0];
            }
            return deckCardsViewData;
        }
        private async Task<(string name, Sprite image)> getCharacterViewData(PVECharacter character)
        {
            var charRef = character.getCharacter();
            if (charRef != null)
            {
                var poolRef = game.card.getLocalCardPoolReference(charRef.cardPoolId);
                await game.card.getOrLoadCardPool(poolRef);
                var cardDefine = game.card.getCardDefine(charRef);
                var skin = game.card.getCardSkin(cardDefine);
                return (skin.name, await game.card.loadCardImage(cardDefine));
            }
            return (string.Empty, null);
        }
        private void quit()
        {
            _ui.gameObject.SetActive(false);
            scene.menu.display();
            reset();
        }
        #endregion
        #endregion

        #endregion

        #region 属性字段
        [TranslateMsg("PVE模式-放弃一局游戏的提示")]
        private const string PVE_CONFIRM_GIVE_UP = "确认要放弃本局游戏？";
        [SerializeField]
        PVERoomPanel _ui;
        [SerializeField]
        PVECardEditorCtrl _cardEditor;
        /// <summary>
        /// 用户当前选择的卡组
        /// </summary>
        PVECharacter _selectedCharacter = null;
        List<PVECharacter> _characterList = new List<PVECharacter>();
        private SaveData _saveData;
        private StageData _stageData;
        private PVEGameResult gameResult;
        private GameManager gameManager => game.game;
        #endregion
    }
    public class PVECharacter
    {
        public Deck deck;
        public string description;
        public DefineReference getCharacter()
        {
            return deck.masterCardRef;
        }
    }
    public enum PVEGameResult
    {
        None,
        Win,
        Draw,
        Defeat
    }
}
