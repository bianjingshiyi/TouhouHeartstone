using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using BJSYGameCore;
using TouhouCardEngine;
using UnityEngine;

namespace Game
{
    public partial class EditorManager : Manager
    {
        public ActionComment getNodeComment(int defineId)
        {
            return LoadOrGetNodeDocument().FirstOrDefault(d => defineId == d.defineId);
        }
        /// <summary>
        /// 载入节点文档
        /// </summary>
        /// <returns></returns>
        public static List<ActionComment> LoadNodeDocument()
        {
            List<ActionComment> commentDict = new List<ActionComment>();

            XmlSerializer deserializer = new XmlSerializer(typeof(ActionComment[]));
            using (var fs = MukioI18n.FileLoader.ReadStreamingAsset(nodeDocFileName))
            {
                var obj = deserializer.Deserialize(fs);
                if (obj is ActionComment[] comments)
                {
                    commentDict.AddRange(comments);
                }
            }
            return commentDict;
        }
        /// <summary>
        /// 载入节点文档
        /// </summary>
        /// <returns></returns>
        private List<ActionComment> LoadOrGetNodeDocument()
        {
            if (comments == null)
                comments = LoadNodeDocument();
            return comments;
        }

        #region 属性字段
        public const string nodeDocFileName = "NodeDoc.xml";
        List<ActionComment> comments = null;
        #endregion
    }

    /// <summary>
    /// 动作注释类
    /// </summary>
    [SerializeField]
    public class ActionComment
    {
        /// <summary>
        /// 定义ID
        /// </summary>
        public int defineId;

        /// <summary>
        /// 过期名称（替换Key）
        /// </summary>
        [XmlArray]
        public string[] obsoleteNames;

        /// <summary>
        /// 编辑器名称
        /// </summary>
        public string editorName;

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
            defineId = act.defineId;
            editorName = act.editorName;
            obsoleteNames = act.obsoleteNames.Where(n => n != editorName).ToArray();
            obsoleteMsg = act.obsoleteMsg;

            var origInputs = consts != null ? inputs.Concat(consts).ToArray() : inputs;
            inputs = ActionParameterComment.GetArray(origInputs, act.inputDefines.ToArray());
            outputs = ActionParameterComment.GetArray(outputs, act.outputDefines.ToArray());
            consts = null;

            category = act.category;
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
        /// 显示名称
        /// </summary>
        public string displayName;
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
            var portType = val.type.IsArray ? val.type.GetElementType() : val.type;
            type = getTypeName(portType);
            name = val.name;
            displayName = val.getDisplayName();
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
                if (input.name == ActionDefine.enterPortName || input.name == ActionDefine.exitPortName)
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
                    if (item.name == defines[i].name || item.name == defines[i].getDisplayName())
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

        private static string getTypeName(Type portType)
        {
            string typeStr = null;
            if (portType.IsGenericType)
            {
                var definition = portType.GetGenericTypeDefinition();
                var genericArgs = portType.GenericTypeArguments;
                var fullName = definition.Name;
                var index = fullName.IndexOf('`');
                var trueName = fullName.Substring(0, index);
                typeStr = $"{trueName}<{string.Join(",", genericArgs.Select(a => getTypeName(a)))}>";
            }
            else
            {
                typeStr = portType.Name;
            }
            return typeStr;
        }
    }
}
