using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game;
using MukioI18n;
using TouhouCardEngine;
using TouhouHeartstone;
using TouhouHeartstone.Builtin;
using UI;
using UnityEngine;
using Deck = TouhouHeartstone.Deck;

namespace Controllers
{
    class PVERoomCtrl : GameObjectCtrl<MainSceneCtrl>
    {
        #region 公有方法
        public async void display()
        {
            _ui.hidePlayerInfoPanel();
            loadCharacterList();
            await updatePlayerList();
            _ui.gameObject.SetActive(true);
        }
        #endregion

        #region 私有方法
        #region 生命周期
        protected void Awake()
        {
            _ui.onQuitButtonClicked += onClickQuitButton;
            _ui.onPlayerSelectItemClicked += onPlayerItemSelected;
            _ui.onCancelSelectPlayer += onCancelSelectPlayerCallback;
        }
        #endregion
        #region 事件回调
        /// <summary>
        /// 返回主界面
        /// </summary>
        private void onClickQuitButton()
        {
            _ui.gameObject.SetActive(false);
            scene.menu.display();
        }
        /// <summary>
        /// 当用户选中了第index个人物
        /// </summary>
        /// <param name="index"></param>
        private async void onPlayerItemSelected(int index)
        {
            //选择人物
            _selectedCharacter = _characterList[index];
            await displayPlayerInfoPanel(_selectedCharacter);
        }
        private void onCancelSelectPlayerCallback()
        {
            _ui.hidePlayerInfoPanel();
        }
        #endregion
        #region 实现方法
        private void loadCharacterList()
        {
            var deck = new Deck();
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
                characterRef = new DefineReference(0, Patchouli.ID),
                starterDeck = deck,
                description = PVE_PATCHOULI_DESC
            });
        }
        private async Task displayPlayerInfoPanel(PVECharacter character)
        {
            var (name, image) = await getCharacterViewData(character);
            var desc = game.lang._(character.description);
            _ui.updatePlayerInfoPanel(name, image, desc);
            await updatePlayerStarterDeck();
            _ui.displayPlayerInfoPanel();
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
        private async Task updatePlayerStarterDeck()
        {
            (string name, Sprite image, int count)[] deckCardsViewData;
            var character = _selectedCharacter;
            var deck = character?.starterDeck;
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
            _ui.updatePlayerInfoPanelDeck(deckCardsViewData);
        }
        private async Task<(string name, Sprite image)> getCharacterViewData(PVECharacter character)
        {
            var charRef = character.characterRef;
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
        #endregion

        #endregion

        #region 属性字段
        [TranslateMsg("PVE模式人物描述-帕秋莉")]
        private const string PVE_PATCHOULI_DESC = "操纵元素之力的魔法使，使用大量的法术向敌人倾泻火力或进行防御。\n通过元素融合还可将法术进行升级，产生更加强大的力量。";
        [SerializeField]
        PVERoomPanel _ui;
        /// <summary>
        /// 用户当前选择的卡组
        /// </summary>
        [SerializeField]
        PVECharacter _selectedCharacter = null;

        List<PVECharacter> _characterList = new List<PVECharacter>();
        #endregion
    }
    public class PVECharacter
    {
        public DefineReference characterRef;
        public Deck starterDeck;
        public string description;
    }
}
