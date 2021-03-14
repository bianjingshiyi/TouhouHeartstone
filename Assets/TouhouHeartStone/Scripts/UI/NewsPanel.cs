using UnityEngine;
using IGensoukyo.Utilities;
using IGensoukyo;
using NitoriNetwork.Common;
using System.Threading.Tasks;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace UI
{
    /// <summary>
    /// 新闻面板
    /// </summary>
    public class NewsPanel : ItemSpawner<NewsListItem>
    {
        [SerializeField]
        string newsSite = "https://thg.igsk.fun";

        [SerializeField]
        GameObject listView;

        [SerializeField]
        GameObject detailView;

        [SerializeField]
        Button closeButton;

        [SerializeField]
        Button detailBackButton;

        [SerializeField]
        TextMeshProUGUI detailTitle;

        [SerializeField]
        HTMLRenderer detail;

        WordPressRestfulClient client;

        Dictionary<int, WordPressRestfulClient.WordPressPost> cachedPosts = new Dictionary<int, WordPressRestfulClient.WordPressPost>();

        private new void Awake()
        {
            base.Awake();   
            client = new WordPressRestfulClient(newsSite);

            closeButton.onClick.AddListener(() => { gameObject.SetActive(false); });
            detailBackButton.onClick.AddListener(() => { switchMode(false); });
        }

        int[] NEWS_CATEGORY = new int[]{ 3, 4, 5, 7 };

        private void OnEnable()
        {
            switchMode(false);
            reloadNews();
        }

        private void reloadNews()
        {
            RemoveAllItem();
            cachedPosts.Clear();

            var data = client.GetPosts(NEWS_CATEGORY);
            foreach (var item in data)
            {
                cachedPosts.Add(item.id, item);

                var obj = SpawnItem();
                obj.SetContent(item.id, null, item.title.rendered, item.excerpt.rendered, item.date.Value.ToString("yyyy-MM-dd HH:mm"));
                obj.OnClick += showArticle;
            }
        }

        private void showArticle(int id)
        {
            Debug.Log("Show article ID: " + id);
            if (!cachedPosts.ContainsKey(id))
            {
                return;
            }

            var post = cachedPosts[id];
            detailTitle.text = post.title.rendered;
            detail.Html = post.content.rendered;

            switchMode(true);
        }

        private void switchMode(bool detail)
        {
            detailView.SetActive(detail);
            listView.SetActive(!detail);
        }
    }
}
