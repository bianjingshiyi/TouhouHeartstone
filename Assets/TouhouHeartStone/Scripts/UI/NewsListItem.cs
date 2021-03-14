using System;
using UnityEngine.UI;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// 新闻列表Item
    /// </summary>
    public class NewsListItem : MonoBehaviour
    {
        [SerializeField]
        Text _title;
        [SerializeField]
        Text _description;
        [SerializeField]
        Text _date;
        [SerializeField]
        GameObject _imageObj;
        [SerializeField]
        Image _image;

        Button btn => GetComponent<Button>();

        public int ID { get; private set; }

        public void SetContent(int id, Sprite img, string title, string content, string date)
        {
            _title.text = title;
            _description.text = content;
            _date.text = date;
            ID = id;
            if (img != null)
            {
                _image.sprite = img;
                _imageObj.SetActive(true);
            } 
            else
            {
                _imageObj.SetActive(false);
            }
        }

        /// <summary>
        /// 点击事件
        /// </summary>
        public event Action<int> OnClick;

        private void Awake()
        {
            btn.onClick.AddListener(() => {
                OnClick?.Invoke(ID);
            
            });
        }
    }
}
