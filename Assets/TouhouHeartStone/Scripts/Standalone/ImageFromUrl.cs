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

            UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url);
            uwr.SendWebRequest().completed += op =>
            {
                var uop = op as UnityWebRequestAsyncOperation;
                if (uop.webRequest.isNetworkError || uop.webRequest.isHttpError || uwr.downloadHandler.data == null)
                {
                    img.sprite = null;
                    return;
                }

                Texture2D texture = new Texture2D(512, 512, TextureFormat.RGB24, true);
                texture.LoadImage(uwr.downloadHandler.data);

                img.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                uwr.Dispose();
            };
        }
    }
}