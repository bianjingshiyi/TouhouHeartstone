using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
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
            NodeDocument.LoadDocument();
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
                oldDict = NodeDocument.LoadDocument();
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
                {
                    comment.Update(define);
                    list.Add(comment);
                }
                else
                {
                    list.Add(new ActionComment(define));
                }
            }

            var serializer = new XmlSerializer(list.GetType());
            var fileName = Path.Combine(CustomBuilder.Editor.StepExternalDataCopy.ExternalFolderPath, NodeDocument.FileName);
            using (var str = new XmlTextWriter(fileName, System.Text.Encoding.UTF8))
            {
                str.Formatting = Formatting.Indented;
                serializer.Serialize(str, list);
            }

            Debug.Log("节点文档已更新");
        }
    }
}

