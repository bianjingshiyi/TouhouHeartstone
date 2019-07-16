using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace TouhouHeartstone.Backend
{
    public static class CardFileHelper
    {
        public static GeneratedCardDefine readFromFile(string path)
        {
            FileInfo file = new FileInfo(path);
            if (file.Exists)
            {
                using (FileStream stream = file.Open(FileMode.Open))
                {
                    return read(new StreamReader(stream));
                }
            }
            else
                return null;
        }
        public static GeneratedCardDefine read(TextReader reader)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(reader);

            GeneratedCardDefine card = new GeneratedCardDefine();
            //ID与类型
            if (doc["Card"].HasAttribute("id"))
                card.setProp("id", Convert.ToInt32(doc["Card"].GetAttribute("id")));
            if (doc["Card"].HasAttribute("type"))
                card.setProp("type", Convert.ToInt32(doc["Card"].GetAttribute("type")));
            //条件
            XmlElement conditionElement = doc["Card"]["Condition"];
            if (conditionElement != null)
                card.setProp("condition", conditionElement.InnerText);
            //效果
            List<Effect> effectList = new List<Effect>();
            for (int i = 0; i < doc["Card"].ChildNodes.Count; i++)
            {
                XmlElement child = doc["Card"].ChildNodes[i] as XmlElement;
                if (child != null && child.Name == "Effect")
                {
                    if (child.HasAttribute("pile") && child.HasAttribute("trigger"))
                    {
                        XmlElement actionElement = child["Action"];
                        if (actionElement != null)
                        {
                            XmlElement checkElement = child["Check"];
                            if (checkElement != null)
                                effectList.Add(new GeneratedEffect(child.GetAttribute("pile"), child.GetAttribute("trigger"), checkElement.InnerText, actionElement.InnerText));
                            else
                                effectList.Add(new GeneratedEffect(child.GetAttribute("pile"), child.GetAttribute("trigger"), "return true;", actionElement.InnerText));
                        }
                        else
                            effectList.Add(new GeneratedEffect(child.GetAttribute("pile"), child.GetAttribute("trigger"), null, child.InnerText));
                    }
                }
            }
            if (effectList.Count > 0)
                card.setProp("effects", effectList.ToArray());
            //属性
            if (card.type == CardDefineType.servant)
            {
                if (doc["Card"]["cost"] != null)
                    card.setProp("cost", Convert.ToInt32(doc["Card"]["cost"].InnerText));
                if (doc["Card"]["attack"] != null)
                    card.setProp("attack", Convert.ToInt32(doc["Card"]["attack"].InnerText));
                if (doc["Card"]["life"] != null)
                    card.setProp("life", Convert.ToInt32(doc["Card"]["life"].InnerText));
            }
            else if (card.type == CardDefineType.spell)
            {
                if (doc["Card"]["cost"] != null)
                    card.setProp("cost", Convert.ToInt32(doc["Card"]["cost"].InnerText));
            }
            return card;
        }
        public static void writeToFile(string path, GeneratedCardDefine card)
        {
            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                write(card, new StreamWriter(stream));
            }
        }
        public static void write(GeneratedCardDefine card, TextWriter writer)
        {
            XmlDocument doc = new XmlDocument();

            doc.CreateXmlDeclaration("1.0", "UTF-8", string.Empty);
            XmlElement cardEle = doc.CreateElement("Card");
            //ID与类型
            cardEle.SetAttribute("id", card.id.ToString());
            cardEle.SetAttribute("type", ((int)card.type).ToString());
            //属性
            if (card.type == CardDefineType.servant)
            {
                XmlElement propEle = doc.CreateElement("cost");
                propEle.InnerText = card.getProp<int>("cost").ToString();
                cardEle.AppendChild(propEle);
                propEle = doc.CreateElement("attack");
                propEle.InnerText = card.getProp<int>("attack").ToString();
                cardEle.AppendChild(propEle);
                propEle = doc.CreateElement("life");
                propEle.InnerText = card.getProp<int>("life").ToString();
                cardEle.AppendChild(propEle);
            }
            else if (card.type == CardDefineType.spell)
            {
                XmlElement propEle = doc.CreateElement("cost");
                propEle.InnerText = card.getProp<int>("cost").ToString();
                cardEle.AppendChild(propEle);
            }
            //使用条件
            XmlElement conditionElement = doc.CreateElement("Condition");
            conditionElement.InnerText = card.getProp<string>("condition");
            cardEle.AppendChild(conditionElement);
            //效果
            GeneratedEffect[] effects = card.getProp<Effect[]>("effects") as GeneratedEffect[];
            if (effects != null)
            {
                for (int i = 0; i < effects.Length; i++)
                {
                    XmlElement effectEle = doc.CreateElement("Effect");
                    effectEle.SetAttribute("pile", effects[i].pile);
                    effectEle.SetAttribute("trigger", effects[i].trigger);
                    if (!string.IsNullOrEmpty(effects[i].actionScript))
                    {
                        if (!string.IsNullOrEmpty(effects[i].filterScript))
                        {
                            XmlElement checkElement = doc.CreateElement("Check");
                            checkElement.InnerText = effects[i].filterScript;
                            effectEle.AppendChild(checkElement);
                        }
                        XmlElement actionElement = doc.CreateElement("Action");
                        actionElement.InnerText = effects[i].actionScript;
                        effectEle.AppendChild(actionElement);
                    }
                    cardEle.AppendChild(effectEle);
                }
            }
            doc.AppendChild(cardEle);

            doc.Save(writer);
        }
    }
}