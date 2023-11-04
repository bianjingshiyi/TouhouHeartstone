using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game;
using Game.PVE;
using MukioI18n;
using TouhouCardEngine;
using TouhouHeartstone;
using TouhouHeartstone.Builtin;
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
            loadCharacterList();
            if (game.pve.hasData())
            {
                _saveData = await game.pve.loadData();
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
            gameManager.onGameStart += onGameStartCallback;
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
            var standardRef = game.card.getLocalCardPoolReference(1);
            var (info, standardPool) = await game.card.getOrLoadCardPool(standardRef);
            var cardDefine = standardPool.cardDefineList.FirstOrDefault();
            await _cardEditor.display(cardDefine, game.card);
            _ui.openCardEditorPage();
            return;
        }
        private async void onSelectCardChoiceClickedCallback(int index)
        {
            var selectedDefRef = _nextSelectingCards[index];
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
        private async void onGameStartCallback()
        {
            if (!gameManager.isPVE)
                return;
            var game = gameManager.game;
            foreach (var player in game.getSortedPlayers())
            {
                var master = player.getMaster();
                if (player != gameManager.localPlayer)
                {
                    await master.addModifier(game, new LifeModifier(10 * (_saveData.currentStage + 1), true));
                }
                else
                {
                    await master.setCurrentLife(game, _saveData.currentHealth);
                }
            }
        }
        private async void onGameFinishedCallback(Player[] winners)
        {
            if (!gameManager.isPVE)
                return;
            var localPlayer = gameManager.localPlayer;
            var pve = this.game.pve;
            var game = gameManager.game;
            if (winners.Contains(localPlayer))
            {
                // 获胜
                _saveData.stageState = PVEStageState.SelectCard;
                _saveData.currentHealth = localPlayer.getMaster().getCurrentLife(game);
                await pve.saveData(_saveData);
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
                pve.deleteData();
                gameResult = PVEGameResult.Defeat;
            }
        }
        private void onGameQuitCallback()
        {
            if (!gameManager.isPVE)
                return;
            switch (gameResult)
            {
                case PVEGameResult.Win:
                case PVEGameResult.Draw:
                    scene.menu.hide();
                    display();
                    break;
                case PVEGameResult.Defeat:
                    reset();
                    break;
            }
            gameResult = PVEGameResult.None;
        }
        #endregion
        #region 实现方法
        private void loadCharacterList()
        {
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
            _characterList.Clear();
            _characterList.Add(new PVECharacter()
            {
                deck = deck,
                description = PVE_PATCHOULI_DESC
            });
        }
        #region 游戏内容
        private async Task createSaveFile()
        {
            _saveData = await game.pve.generateNewSaveData(_selectedCharacter);
            _selectedCharacter = null;
            await game.pve.saveData(_saveData);
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
                    deck = _nextBoss.deck;
                }
                player.setDeck(deck);
            }
            var cardPools = new CardPoolInfoPack[]
            {
                new CardPoolInfoPack(0, string.Empty, 0)
            };
            var seed = _saveData.stageSeed;
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
            game.pve.deleteData();
            quit();
        }
        private async Task nextStage()
        {
            _saveData.advanceStage();
            _saveData.stageState = PVEStageState.Main;
            await game.pve.saveData(_saveData);
            await displayInRunPanel();
        }
        private PVECharacter getNextEnemyCharacter()
        {
            return _nextBoss;
        }
        private DefineReference[] getNextSelectingCards()
        {
            return _nextSelectingCards;
        }
        private void generateNextBoss()
        {
            _nextBoss = game.pve.generateNextBoss(_saveData.stageSeed);
        }
        private void generateSelectingCards()
        {
            _nextSelectingCards = game.pve.generateNextSelectingCards(_saveData.stageSeed);
        }
        private void reset()
        {
            _saveData = null;
            gameResult = PVEGameResult.None;
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
                generateSelectingCards();
                var defRefs = getNextSelectingCards();
                var cardManager = game.card;
                var cardDefines = defRefs.Select(d => cardManager.getLocalCardDefine(d)).ToArray();
                _ui.updateSelectCardPanel(cardDefines, cardManager);
                _ui.switchToSelectCardPanel();
            }
            else
            {
                generateNextBoss();
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
        [TranslateMsg("PVE模式人物描述-帕秋莉")]
        private const string PVE_PATCHOULI_DESC = "操纵元素之力的魔法使，使用大量的法术向敌人倾泻火力或进行防御。\n通过元素融合还可将法术进行升级，产生更加强大的力量。";
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
        private PVECharacter _nextBoss;
        private DefineReference[] _nextSelectingCards;
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
