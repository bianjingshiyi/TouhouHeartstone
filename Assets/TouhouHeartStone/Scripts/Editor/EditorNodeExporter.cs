using Game;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using TouhouCardEngine;
using TouhouHeartstone;
using UnityEngine;
using UnityEditor;
using System.Xml.Serialization;
using System.Linq;
using System.IO;
using System.Xml;

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

            Dictionary<string, ActionComment> oldDict = null;
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
            foreach (var kv in defines)
            {
                if (oldDict?.ContainsKey(kv.Key) == true)
                {
                    var item = oldDict[kv.Key];
                    item.Update(kv.Value);
                    list.Add(item);
                    continue;
                }

                bool hasOld = false;
                foreach (var oldName in kv.Value.obsoleteNames)
                {
                    if (oldDict?.ContainsKey(oldName) == true)
                    {
                        var item = oldDict[kv.Key];
                        item.Update(kv.Value);
                        list.Add(item);
                        hasOld = true;
                        break;
                    }
                }

                if (!hasOld)
                {
                    list.Add(new ActionComment(kv.Value));
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

