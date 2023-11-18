using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace IGensoukyo
{
    [RequireComponent(typeof(Image))]
    class ImageFromUrl : MonoBehaviour
    {
        Image img => GetComponent<Image>();

        [SerializeField]
        String _imageUrl = "";

        public string ImageUrl
        {
            get => _imageUrl;
            set { _imageUrl = value; loadImage(); }
        }

        void OnEnable()
        {
            loadImage();
        }

        UnityWebRequestAsyncOperation requestOP;
        bool destroyed = false;
        private void OnDestroy()
        {
            destroyed = true;
            requestOP.completed -= requestHandler;
        }

        void loadImage()
        {
            if (String.IsNullOrEmpty(_imageUrl)) 
            {
                img.sprite = null;
                return;
            }
            
            var url = ImageUrl;
            if (url.StartsWith("//"))
                url = "https:" + ImageUrl;

            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            requestOP = request.SendWebRequest();
            requestOP.completed += requestHandler;
        }

        void requestHandler(AsyncOperation op)
        {
            var uop = op as UnityWebRequestAsyncOperation;
            if (destroyed)
                return;
            var req = uop.webRequest;

            if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError || req.downloadHandler.data == null)
            {
                img.sprite = null;
                return;
            }

            Texture2D texture = new Texture2D(512, 512, TextureFormat.RGB24, true);
            texture.LoadImage(req.downloadHandler.data);

            img.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            // request.Dispose();
        }
    }
}