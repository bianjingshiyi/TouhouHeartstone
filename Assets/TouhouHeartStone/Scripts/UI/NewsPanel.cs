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

        [SerializeField]
        GameObject loadingImg;

        WordPressRestfulClient client;

        Dictionary<int, WordPressRestfulClient.WordPressPost> cachedPosts = new Dictionary<int, WordPressRestfulClient.WordPressPost>();

        private new void Awake()
        {
            base.Awake();
            client = new WordPressRestfulClient(newsSite);

            closeButton.onClick.AddListener(() => { gameObject.SetActive(false); });
            detailBackButton.onClick.AddListener(() => { switchMode(false); });
        }

        int[] NEWS_CATEGORY = new int[] { 3, 4, 5, 7 };

        private void OnEnable()
        {
            switchMode(false);
            reloadNews();
        }

        private async void reloadNews()
        {
            loadingImg.SetActive(true);

            RemoveAllItem();
            cachedPosts.Clear();

            try
            {
                var data = await client.GetPostsAsync(NEWS_CATEGORY);
                foreach (var item in data)
                {
                    cachedPosts.Add(item.id, item);

                    var obj = SpawnItem();
                    obj.SetContent(
                        item.id,
                        null,
                        Utilities.HTMLRemoveTags(item.title.rendered),
                        Utilities.HTMLRemoveTags(item.excerpt.rendered),
                        item.date.Value.ToString("yyyy-MM-dd HH:mm")
                    );
                    obj.OnClick += showArticle;
                }
            }
            catch (NetClientException e)
            {
                Debug.LogException(e);
            }
            finally
            {
                loadingImg.SetActive(false);
            }
        }

        private void showArticle(int id)
        {
            if (!cachedPosts.ContainsKey(id))
                return;

            var post = cachedPosts[id];
            detailTitle.text = post.title.rendered;
            detail.Html = post.content.rendered;

            switchMode(true);
        }

        private void switchMode(bool detail)
        {
            detailView.SetActive(detail);
            listView.SetActive(!detail);

            if (detail)
            {
                var rt = this.detail.GetComponent<RectTransform>();
                var p = rt.anchoredPosition;
                p.y = 0;
                rt.anchoredPosition = p;
            }
        }
    }
}
