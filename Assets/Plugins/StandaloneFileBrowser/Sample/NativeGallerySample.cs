using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NativeGallerySample : MonoBehaviour
{
    protected void Awake()
    {
        _button.onClick.AddListener(() =>
        {
            var permission = NativeGallery.GetImageFromGallery(path =>
            {
                Debug.Log("Image path: " + path);
                if (path != null)
                {
                    // Create Texture from selected image
                    Texture2D texture = NativeGallery.LoadImageAtPath(path);
                    if (texture == null)
                    {
                        Debug.Log("Couldn't load texture from " + path);
                        return;
                    }
                    _image.texture = texture;
                }
            });
        });
    }
    [SerializeField]
    Button _button;
    [SerializeField]
    RawImage _image;
}
