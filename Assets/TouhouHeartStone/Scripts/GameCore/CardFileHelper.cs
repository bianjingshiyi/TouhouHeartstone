using System;
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
                using (StreamReader reader = new StreamReader(file.Open(FileMode.Open)))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(reader);

                    GeneratedCardDefine _card = new GeneratedCardDefine();
                    _card.setProp("id", Convert.ToInt32(doc["Card"].GetAttribute("id")));
                    _card.setProp("type", Convert.ToInt32(doc["Card"].GetAttribute("type")));
                    _card.setProp("cost", Convert.ToInt32(doc["Card"]["cost"].InnerText));
                    _card.setProp("attack", Convert.ToInt32(doc["Card"]["attack"].InnerText));
                    _card.setProp("life", Convert.ToInt32(doc["Card"]["life"].InnerText));
                    return _card;
                }
            }
            else
                return null;
        }
        public static void writeToFile(string path, GeneratedCardDefine card)
        {
            using (StreamWriter writer = new StreamWriter(new FileStream(path, FileMode.Create)))
            {
                XmlDocument doc = new XmlDocument();

                doc.CreateXmlDeclaration("1.0", "UTF-8", string.Empty);
                XmlElement cardEle = doc.CreateElement("Card");
                cardEle.SetAttribute("id", card.id.ToString());
                cardEle.SetAttribute("type", ((int)card.type).ToString());
                if (card.type == CardType.servant)
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
                doc.AppendChild(cardEle);

                doc.Save(writer);
            }
        }
    }
}