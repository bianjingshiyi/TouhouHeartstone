using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BJSYGameCore;
using Controllers;
using Game.BattleTable;
using TouhouCardEngine;
using TouhouCardEngine.Shared;
using TouhouHeartstone;
using UI;
using UnityEngine;
using Deck = TouhouHeartstone.Deck;

namespace Game
{
    public class UIManager : BJSYGameCore.UI.UIManager
    {
        #region 卡组卡牌列表
        public async Task enterDeckListItem(DeckCardList deckList, Deck deck, int index, IEnumerable<ICardPoolReference> poolSet)
        {
            var cardRef = await main.deck.getDeckCardReferenceByIndex(deck, index, poolSet);
            await enterDeckListItem(deckList, cardRef, index, poolSet);
        }
        public Task enterDeckListItem(DeckCardList deckList, DeckItem[] deckItems, int index, IEnumerable<ICardPoolReference> poolSet)
        {
            var cardRef = deckItems[index]?.cardRef;
            return enterDeckListItem(deckList, cardRef, index, poolSet);
        }
        public async Task enterDeckListItem(DeckCardList deckList, DefineReference cardRef, int index, IEnumerable<ICardPoolReference> poolSet)
        {
            var poolRef = main.card.getCardPoolReferenceInEnv(cardRef.cardPoolId, poolSet);

            var cardDefine = main.card.getCardDefine(poolRef, cardRef.defineId);
            var cardSkin = main.card.getCardSkin(poolRef, cardRef.defineId);
            var listItem = deckList.getListItem(index);

            await showDeckCardListPreview(deckList, cardRef, index, poolSet);
            scene.addCardPopup(deckList.previewCard, cardDefine, poolSet);
        }
        public void exitDeckListItem(DeckCardList deckList)
        {
            deckList.hidePreviewCard();
            scene.removeCardPopup(deckList.previewCard);
        }
        public async Task showDeckCardListPreview(DeckCardList deckView, DefineReference defRef, int index, IEnumerable<ICardPoolReference> poolSet)
        {
            var cardManager = main.card;
            var poolRef = cardManager.getCardPoolReferenceInEnv(defRef.cardPoolId, poolSet);
            var cardDef = cardManager.getCardDefine(poolRef, defRef.defineId);
            deckView.setPreviewCardVisible(true);
            deckView.setPreviewCardPosition(index);
            if (cardDef == null)
            {
                deckView.previewCard.setUnknownCard(true);
            }
            else
            {
                await deckView.previewCard.setCardDefine(cardDef, cardManager, true);
            }
        }
        #endregion

        public string getTranslateNameForLanguage(string languageName)
        {
            if (_languageTranslateNameMapList.Find(m => m.languageName == languageName) is LanguageTranslateNamePair pair)
            {
                return pair.translateName;
            }
            else
                return languageName;
        }
        public async Task<bool> cardshot(CardDefine cardDefine, CardSkinData cardSkin)
        {
            //激活摄像机与灯光
            _cardshotRoot.gameObject.SetActive(true);
            //设置卡牌
            _shotcard.setCardDefine(cardDefine, cardSkin, _game.card);
            var poolRef = _game.card.getLocalCardPoolReference(cardDefine.cardPoolId);
            var image = await _game.card.loadCardImage(poolRef, cardDefine.id);
            _shotcard.setImageSprite(image);
            //创建一个用于渲染卡图的RenderTexture
            RenderTexture renderTexture = new RenderTexture(1024, 1024, 24);
            renderTexture.antiAliasing = 2;
            _cardshotCamera.targetTexture = renderTexture;
            _cardshotCamera.enabled = false;
            // 相机渲染。
            _cardshotCamera.Render();
            // 从Render Texture读取像素并保存为图片
            int exportWidth = 800;
            Texture2D texture = new Texture2D(exportWidth, renderTexture.height, TextureFormat.RGBA32, false);
            RenderTexture.active = renderTexture;
            texture.ReadPixels(new Rect((renderTexture.width - exportWidth) / 2, 0, exportWidth, renderTexture.height), 0, 0);
            texture.Apply();
            RenderTexture.active = null; // 重置活动的Render Texture
            _cardshotCamera.targetTexture = null;
            renderTexture.Release();

            // 将Texture2D转换为字节流并保存为文件
            byte[] bytes = texture.EncodeToPNG();
            Destroy(texture);
            Destroy(renderTexture);
            _cardshotRoot.gameObject.SetActive(false);

            var defaultFileName = cardSkin.name;
            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (var chr in invalidChars)
            {
                defaultFileName = defaultFileName.Replace(chr, '_');
            }
            var path = await _game.card.exportAnyFile(bytes, defaultFileName, "png");
            return path != null;
        }
        #region 属性字段
        public UIDialog dialog => _dialog;
        MainManager main => this.findInstance<MainManager>();
        MainSceneCtrl scene => this.findInstance<MainSceneCtrl>();
        [SerializeField]
        MainManager _game;
        [SerializeField]
        Canvas _canvas;
        [SerializeField]
        Main _main;
        [SerializeField]
        UIDialog _dialog;
        [SerializeField]
        List<LanguageTranslateNamePair> _languageTranslateNameMapList;
        [SerializeField]
        GameObject _cardshotRoot;
        [SerializeField]
        TableCard _shotcard;
        [SerializeField]
        Camera _cardshotCamera;
        #endregion
        #region 内部类
        [Serializable]
        class LanguageTranslateNamePair
        {
            public string languageName;
            public string translateName;
        }
        #endregion
    }
}
