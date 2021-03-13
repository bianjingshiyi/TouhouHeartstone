using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using HtmlAgilityPack;

namespace IGensoukyo
{
    // [ExecuteInEditMode]
    public class HTMLRenderer : MonoBehaviour
    {
        [SerializeField]
        TextMeshProUGUI _item;

        [SerializeField]
        [HideInInspector]
        TextMeshProUGUI[] items = new TextMeshProUGUI[0];

        List<TextMeshProUGUI> itemList;

        [TextArea(5, 20)]
        [SerializeField]
        string _html;

        string _cachedHtml;

        public string Html
        {
            get => _html;
            set => _html = value;
        }

        void Update()
        {
            if (_cachedHtml != _html)
            {
                _cachedHtml = _html;
                renderHTML();
            }
        }

        string currentSectionText;
        int currentIndex = 0;

        private void Awake()
        {
            _item.gameObject.SetActive(false);
        }

        void hideAll()
        {
            if (items == null) return;

            foreach (var item in items)
            {
                item?.gameObject?.SetActive(false);
            }
            currentIndex = 0;
        }

        TextMeshProUGUI createItem()
        {
            for (int i = currentIndex; items != null && i < items.Length; i++)
            {
                if (items[i] != null)
                {
                    currentIndex = i + 1;
                    items[i].gameObject.SetActive(true);
                    return items[i];
                }
            }

            var item = Instantiate(_item, this.transform);
            item.gameObject.SetActive(true);
            if (itemList == null)
                itemList = new List<TextMeshProUGUI>(items);

            itemList.Add(item);
            currentIndex++;

            items = itemList.ToArray();

            return item;
        }

        bool listIsOrdered = false;
        /// <summary>
        /// 有序列表Index
        /// </summary>
        int listOrder = 1;
        /// <summary>
        /// 缩进
        /// </summary>
        int listIntent { 
            get;
            set;
        }

        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="node"></param>
        /// <returns>是否跳过子节点</returns>
        bool parseNode(HtmlNode node)
        {
            switch (node.Name.ToLower())
            {
                case "#comment":
                case "html":
                case "body":
                case "div":
                case "span":
                case "noscript":
                case "label":
                case "article":
                    break; // 忽略，不做任何处理
                case "img":
                    break; // 暂时不知道怎么处理
                case "head":
                case "script":
                case "style":
                    return true; // 忽略，并且忽略子节点
                case "ul":
                    TryCreateNodeWithContent(); // 防止套娃的时候套没了
                    listIsOrdered = false;
                    listIntent++;
                    break;
                case "ol":
                    TryCreateNodeWithContent(); // 防止套娃的时候套没了
                    listIsOrdered = true;
                    listOrder = 1;
                    listIntent++;
                    break;
                case "a":
                    var target = node.GetAttributeValue("href", "");
                    currentSectionText += $"<link=\"{target}\"><color=blue><u>";
                    break;
                case "p":
                case "h1":
                case "h2":
                case "h3":
                case "h4":
                case "h5":
                case "h6":
                case "h7":
                case "header":
                case "footer":
                    // 新建一个段落
                    TryCreateNodeWithContent(); // 防止套娃的时候套没了
                    break;
                case "li":
                    TryCreateNodeWithContent(); // 防止套娃的时候套没了
                    currentSectionText += listIsOrdered ? listOrder++ + ". " : "· ";
                    break;
                case "br":
                    currentSectionText += "\n";
                    break;
                case "b":
                case "em":
                case "strong":
                    currentSectionText += "<b>";
                    break;
                case "i":
                    currentSectionText += "<i>";
                    break;
                case "u":
                    currentSectionText += "<u>";
                    break;
                case "del":
                case "strike":
                    currentSectionText += "<s>";
                    break;
                case "#text":
                    var text = node.InnerText.Trim();
                    if (text != "")
                        currentSectionText += text;
                    break;
                default:
                    Debug.Log("Unhandle node: " + node.Name);
                    break;
            }
            return false;
        }

        TextMeshProUGUI TryCreateNodeWithContent()
        {
            if (currentSectionText == "")
                return null;

            var item = createItem();

            if (listIntent > 0)
                currentSectionText = $"<margin={listIntent * 1}em>" + currentSectionText;

            item.text = currentSectionText;
            currentSectionText = "";

            return item;
        }

        void nodeExit(HtmlNode node)
        {
            TextMeshProUGUI item = null;
            switch (node.Name.ToLower())
            {
                case "p":
                case "h1":
                case "h2":
                case "h3":
                case "h4":
                case "h5":
                case "h6":
                case "h7":
                case "header":
                case "footer":
                case "li":
                    item = TryCreateNodeWithContent();
                    break;
                case "b":
                case "em":
                case "strong":
                    currentSectionText += "</b>";
                    break;
                case "i":
                    currentSectionText += "</i>";
                    break;
                case "u":
                    currentSectionText += "</u>";
                    break;
                case "del":
                case "strike":
                    currentSectionText += "</s>";
                    break;
                case "ul":
                case "ol":
                    listIntent--;
                    break;
                case "a":
                    currentSectionText += "</u></color></link>";
                    break;
                default:
                    break;
            }

            if (item == null) return;

            TMP_StyleSheet sheet = TMP_Settings.defaultStyleSheet;

            switch (node.Name.ToLower())
            {
                case "h1":
                    item.textStyle = sheet.GetStyle("h1");
                    break;
                case "h2":
                    item.textStyle = sheet.GetStyle("h2");
                    break;
                case "h3":
                    item.textStyle = sheet.GetStyle("h3");
                    break;
                case "h4":
                case "h5":
                case "h6":
                case "h7":
                case "p":
                case "header":
                case "footer":
                case "li":
                    item.textStyle = sheet.GetStyle("normal");
                    break;
                default:
                    break;
            }
        }

        void renderHTML()
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(_cachedHtml);

            // reset
            hideAll();
            listIntent = 0;

            var currentNode = htmlDoc.DocumentNode.FirstChild;
            while (currentNode != null)
            {
                bool skipChild = parseNode(currentNode);

                // 深度优先遍历
                if (!skipChild && currentNode.FirstChild != null)
                {
                    currentNode = currentNode.FirstChild;
                }
                else
                {
                    if (currentNode.NextSibling != null)
                    {
                        nodeExit(currentNode);
                        currentNode = currentNode.NextSibling;
                    }
                    else
                    {
                        currentNode = currentNode.ParentNode;
                        while (currentNode != null)
                        {
                            nodeExit(currentNode);
                            if (currentNode.NextSibling != null)
                            {
                                currentNode = currentNode.NextSibling;
                                break;
                            }
                            else
                            {
                                currentNode = currentNode.ParentNode;
                            }
                        }
                    }
                }
            }
        }
    }

}
