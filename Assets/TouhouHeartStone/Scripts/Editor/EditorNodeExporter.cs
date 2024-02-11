using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using MukioI18n;
using TouhouCardEngine;
using TouhouHeartstone;
using UnityEditor;
using UnityEngine;

namespace Game
{
    public class EditorNodeExporter : MonoBehaviour
    {
        [MenuItem("Custom/Editor/更新节点文档")]
        public static void ExportEditorNode()
        {
            export();
        }

        [MenuItem("Custom/Editor/检查文档格式")]
        public static void CheckNodeDocument()
        {
            EditorManager.LoadNodeDocument();
        }

        static void export()
        {
            var defines = ActionDefine.loadDefinesFromAssemblies(new Assembly[]
            {
                typeof(CardEngine).Assembly,
                typeof(HearthStoneRule).Assembly,
                typeof(EffectEditorLibrary).Assembly
            });

            List<ActionComment> oldDict = null;
            try
            {
                oldDict = EditorManager.LoadNodeDocument();
            }
            catch (FileNotFoundException)
            {
                // 
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }

            List<ActionComment> list = new List<ActionComment>();
            foreach (var define in defines)
            {
                ActionComment comment = null;
                if (oldDict != null)
                {
                    if (oldDict.FirstOrDefault(d => define.defineId == d.defineId) is ActionComment defineNameComment)
                    {
                        comment = defineNameComment;
                    }
                    else if (oldDict.FirstOrDefault(d => define.obsoleteNames.Contains(d.editorName)) is ActionComment oldNameComment)
                    {
                        comment = oldNameComment;
                    }
                }

                if (comment != null)
                    comment.Update(define);
                else
                    comment = new ActionComment(define);
                list.Add(comment);
            }
            var pot = new MukioPotGenerator("ZMCS", "Translation Team");
            addTranslatesFromComments(pot, list);
            pot.WriteOut(Path.Combine(FileLoader.FolderPath, "i18n", "docs", "node.pot"));

            var serializer = new XmlSerializer(list.GetType());
            var fileName = Path.Combine(CustomBuilder.Editor.StepExternalDataCopy.ExternalFolderPath, EditorManager.nodeDocFileName);
            using (var str = new XmlTextWriter(fileName, System.Text.Encoding.UTF8))
            {
                str.Formatting = Formatting.Indented;
                serializer.Serialize(str, list);
            }

            Debug.Log("节点文档已更新");
        }

        private static void addTranslatesFromComments(MukioPotGenerator pot, IEnumerable<ActionComment> comments)
        {
            foreach (var comment in comments)
            {
                addTranslateFromComment(pot, comment);
            }
        }
        private static void addTranslateFromComment(MukioPotGenerator pot, ActionComment comment)
        {
            var reference = $"节点/{comment.category}/{comment.editorName}";
            var summaryCmt = $"节点\"{comment.editorName}\"的说明";
            var exampleCmt = $"节点\"{comment.editorName}\"的示例";
            if (!string.IsNullOrEmpty(comment.summaryString))
                pot.AddString(new PotTranslate(comment.summaryString, reference, summaryCmt, StringTable.CONTEXT_NODE_DOC_SUMMARY));
            if (!string.IsNullOrEmpty(comment.exampleString))
                pot.AddString(new PotTranslate(comment.exampleString, reference, exampleCmt, StringTable.CONTEXT_NODE_DOC_EXAMPLE));
            foreach (var input in comment.inputs)
            {
                if (string.IsNullOrEmpty(input.summaryString))
                    continue;
                var cmt = $"节点\"{comment.editorName}\"的输入端口\"{input.displayName}\"的说明";
                pot.AddString(new PotTranslate(input.summaryString, reference, cmt, StringTable.CONTEXT_NODE_DOC_PORT_SUMMARY));
            }
            foreach (var output in comment.outputs)
            {
                if (string.IsNullOrEmpty(output.summaryString))
                    continue;
                var cmt = $"节点\"{comment.editorName}\"的输出端口\"{output.displayName}\"的说明";
                pot.AddString(new PotTranslate(output.summaryString, reference, cmt, StringTable.CONTEXT_NODE_DOC_PORT_SUMMARY));
            }
        }
    }
}

