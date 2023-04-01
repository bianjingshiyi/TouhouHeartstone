using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using BJSYGameCore;
using IGensoukyo;
using MukioI18n;
using TouhouCardEngine;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class NodeDocument : MonoBehaviour
    {
        public const string FileName = "NodeDoc.xml";

        [SerializeField]
        HTMLRenderer _html;

        [SerializeField]
        Button _closeBtn;

        Dictionary<string, ActionComment> comments = null;
        LangManager lang => this.findInstance<LangManager>();

        private void Awake()
        {
            _closeBtn.onClick.AddListener(Hide);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        [TranslateMsg("节点帮助的标题，{0}是节点名称")]
        const string TitleTemplate = "<h2>{0}</h2>";
        [TranslateMsg("节点参数列表的标题")]
        const string ParameterTitle = "<h3>参数列表</h3>";
        [TranslateMsg("节点输入参数")]
        const string InputParameter = "<p>输入参数</p>";
        [TranslateMsg("节点输出参数")]
        const string OutputParameter = "<p>输出参数</p>";
        [TranslateMsg("节点常量参数")]
        const string ConstParameter = "<p>常量参数</p>";
        [TranslateMsg("参数模板。{0}: 参数名称, {1}: 参数类型, {2}: 数组, {3}: 参数, {4}: 注释")]
        const string ParameterTemplate = "<li>{0}({1}): {4}</li>";
        [TranslateMsg("节点说明模板, {0} 为节点自带说明")]
        const string SummaryTemplate = "<p>{0}</p>";
        [TranslateMsg("节点分类模板, {0} 为分类名称")]
        const string CategoryTemplate = "<p><b>分类</b>: {0}</p>";
        [TranslateMsg("节点过期模板")]
        const string ObsoleteTemplate = "<p><b>此节点已被废弃，可能在后续版本中删除。</b>{0}</p>";
        [TranslateMsg("节点示例标题")]
        const string ExampleTitle = "<h3>示例</h3>";
        [TranslateMsg("节点示例模板, {0} 为节点自带说明")]
        const string ExampleTemplate = "<p>{0}</p>";

        public void Show(string id)
        {
            if (comments == null) comments = LoadDocument();
            if (!comments.ContainsKey(id))
                return;

            var doc = comments[id];
            string str = "";
            str += lang._(TitleTemplate, lang._(doc.defineName)); // 标题
            if (!string.IsNullOrEmpty(doc.category))
                str += lang._(CategoryTemplate, lang._(doc.category)); // 分类
            if (doc.obsoleteMsg != null)
                str += lang._(ObsoleteTemplate, lang._(doc.obsoleteMsg)); // 过期节点
            str += lang._(SummaryTemplate, lang._(doc.summaryString)); // 说明

            // 参数文档
            str += lang._(ParameterTitle);
            if (doc.inputs.Length > 0)
            {
                str += lang._(InputParameter);
                str += GetParametersStr(doc.inputs);
            }
            if (doc.outputs.Length > 0)
            {
                str += lang._(OutputParameter);
                str += GetParametersStr(doc.outputs);
            }
            if (doc.consts.Length > 0)
            {
                str += lang._(ConstParameter);
                str += GetParametersStr(doc.consts);
            }

            // 示例
            if (!string.IsNullOrEmpty(doc.exampleString))
            {
                str += lang._(ExampleTitle);
                str += lang._(ExampleTemplate, lang._(doc.exampleString));
            }

            _html.Html = str;

            gameObject.SetActive(true);
        }

        string GetParameterStr(ActionParameterComment item)
        {
            var dotIndex = item.type.LastIndexOf(".");
            string type = dotIndex > 0 ? item.type.Substring(dotIndex + 1) : item.type;
            return lang._(ParameterTemplate, lang._(item.name), type, item.isArray, item.isParams, lang._(item.summaryString));
        }
        string GetParametersStr(ActionParameterComment[] items)
        {
            string str = "";
            str += "<ul>";
            foreach (var item in items)
            {
                str += GetParameterStr(item);
            }
            str += "</ul>";
            return str;
        }

        /// <summary>
        /// 载入节点文档
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, ActionComment> LoadDocument()
        {
            Dictionary<string, ActionComment> commentDict = new Dictionary<string, ActionComment>();

            XmlSerializer deserializer = new XmlSerializer(typeof(ActionComment[]));
            using (var fs = MukioI18n.FileLoader.ReadStreamingAsset(FileName))
            {
                var obj = deserializer.Deserialize(fs);
                if (obj is ActionComment[] comments)
                {
                    foreach (var comment in comments)
                    {
                        commentDict.Add(comment.defineName, comment);
                    }
                }
            }

            return commentDict;
        }
    }

    /// <summary>
    /// 动作注释类
    /// </summary>
    [SerializeField]
    public class ActionComment
    {
        /// <summary>
        /// 定义名称（Key）
        /// </summary>
        public string defineName;

        /// <summary>
        /// 过期名称（替换Key）
        /// </summary>
        [XmlArray]
        public string[] obsoleteNames;
        /// <summary>
        /// 节点过期提示。不为null则说明过期
        /// </summary>
        public string obsoleteMsg;

        /// <summary>
        /// 节点分类
        /// </summary>
        public string category;

        /// <summary>
        /// 节点简介
        /// </summary>
        [XmlIgnore]
        public string summaryString = "";

        /// <summary>
        /// 节点简介
        /// </summary>
        [XmlElement]
        public XmlCDataSection summary
        {
            get
            {
                XmlDocument doc = new XmlDocument();
                return doc.CreateCDataSection(summaryString);
            }
            set
            {
                summaryString = value.Value;
            }
        }

        /// <summary>
        /// 节点示例
        /// </summary>
        [XmlIgnore]
        public string exampleString = "";

        /// <summary>
        /// 节点示例
        /// </summary>
        [XmlElement]
        public XmlCDataSection example
        {
            get
            {
                XmlDocument doc = new XmlDocument();
                return doc.CreateCDataSection(exampleString);
            }
            set
            {
                exampleString = value.Value;
            }
        }
        public ActionParameterComment[] inputs;
        public ActionParameterComment[] outputs;
        public ActionParameterComment[] consts;

        public ActionComment() { }

        public ActionComment(ActionDefine act)
        {
            Update(act);
        }

        /// <summary>
        /// 更新 ActionDefine
        /// </summary>
        /// <param name="act"></param>
        public void Update(ActionDefine act)
        {
            defineName = act.defineName;
            obsoleteNames = act.obsoleteNames;
            obsoleteMsg = act.obsoleteMsg;

            inputs = ActionParameterComment.GetArray(inputs, act.inputDefines.ToArray());
            outputs = ActionParameterComment.GetArray(outputs, act.outputDefines.ToArray());
            consts = ActionParameterComment.GetArray(consts, act.constDefines.ToArray());

            if (act is MethodActionDefine method)
                category = method.category;
        }
    }

    /// <summary>
    /// 参数注释类
    /// </summary>
    [Serializable]
    public class ActionParameterComment
    {
        /// <summary>
        /// 数据类型
        /// </summary>
        public string type;
        /// <summary>
        /// 名称
        /// </summary>
        public string name;
        /// <summary>
        /// 参数
        /// </summary>
        public bool isParams;
        /// <summary>
        /// 是否为数组
        /// </summary>
        public bool isArray;

        [XmlIgnore]
        public string summaryString = "";

        /// <summary>
        /// 注释
        /// </summary>
        [XmlElement]
        public XmlCDataSection summary
        {
            get
            {
                XmlDocument doc = new XmlDocument();
                return doc.CreateCDataSection(summaryString);
            }
            set
            {
                summaryString = value.Value;
            }
        }

        public ActionParameterComment() { }

        public ActionParameterComment(PortDefine valueDefine)
        {
            Update(valueDefine);
        }

        /// <summary>
        /// 更新 ValueDefine
        /// </summary>
        /// <param name="val"></param>
        public void Update(PortDefine val)
        {
            type = val.type.IsArray ? val.type.GetElementType().FullName : val.type.FullName;
            name = val.getDisplayName();
            isParams = val.isParams;
            isArray = val.type.IsArray;
        }

        /// <summary>
        /// 将 ValueDefine 转换为 ActionParameterComment
        /// </summary>
        /// <param name="orig">原文档</param>
        /// <param name="defines">新的valueDefine</param>
        /// <returns></returns>
        public static ActionParameterComment[] GetArray(ActionParameterComment[] orig, PortDefine[] defines)
        {
            List<ActionParameterComment> list = new List<ActionParameterComment>();
            for (int i = 0; i < defines.Length; i++)
            {
                var input = defines[i];
                if (input.GetPortType() == PortType.Control && i == 0)
                {
                    // 跳过第一个动作端口。
                    continue;
                }
                if (orig == null)
                {
                    list.Add(new ActionParameterComment(input));
                    continue;
                }

                bool hasOld = false;
                foreach (var item in orig)
                {
                    if (item.name == defines[i].getDisplayName())
                    {
                        item.Update(input);
                        list.Add(item);
                        hasOld = true;
                        break;
                    }
                }
                if (!hasOld)
                {
                    list.Add(new ActionParameterComment(input));
                }
            }
            return list.ToArray();
        }
    }
}

