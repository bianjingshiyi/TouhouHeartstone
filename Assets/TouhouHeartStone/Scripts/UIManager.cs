using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Game.BattleTable;
using TouhouCardEngine;
using UI;
using UnityEngine;

namespace Game
{
    public class UIManager : BJSYGameCore.UI.UIManager
    {
        public string getTranslateNameForLanguage(string languageName)
        {
            if (_languageTranslateNameMapList.Find(m => m.languageName == languageName) is LanguageTranslateNamePair pair)
            {
                return pair.translateName;
            }
            else
                return languageName;
        }
        public IEnumerator cardshot(CardDefine cardDefine, CardSkinData cardSkin)
        {
            //激活摄像机与灯光
            _cardshotRoot.gameObject.SetActive(true);
            //设置卡牌
            _shotcard.setCardDefine(cardDefine, cardSkin, _game.card);
            _shotcard.setImageSprite(_game.card.loadCardImage(_game.card.getLocalCardPoolReference(cardDefine.cardPoolId), cardDefine.id).Result);
            //创建一个用于渲染卡图的RenderTexture
            RenderTexture renderTexture = new RenderTexture(1024, 1024, 24);
            renderTexture.antiAliasing = 2;
            _cardshotCamera.targetTexture = renderTexture;
            //等待渲染
            yield return new WaitForEndOfFrame();
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
            _ = _game.card.exportAnyFile(bytes, defaultFileName, "png");
        }
        #region 属性字段
        public UIDialog dialog => _dialog;
        public float canvasScale => _canvas.transform.localScale.x;
        public Canvas canvas => _canvas;
        [SerializeField]
        MainManager _game;
        [SerializeField]
        Canvas _canvas;
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
