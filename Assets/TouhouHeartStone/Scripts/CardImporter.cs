using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TouhouHeartstone;
using TouhouCardEngine;
using UI;
using System.Reflection;
using ExcelLibrary.SpreadSheet;
namespace Game
{
    static class CardImporter
    {
        public static CardDefine[] GetCardDefines(Assembly[] assemblies, Dictionary<Workbook, string> workbooks, out CardSkinData[] skins)
        {
            List<CardDefine> cardList = new List<CardDefine>(CardHelper.getCardDefines(assemblies));
            List<CardSkinData> skinList = new List<CardSkinData>();
            foreach (var pBook in workbooks)
            {
                Worksheet worksheet = pBook.Key.Worksheets[0];
                int idIndex = findColIndex(worksheet, "ID");
                foreach (var pRow in worksheet.Cells.Rows)
                {
                    if (pRow.Key != 0)
                    {
                        //Debug.Log(pRow.Value.GetCell(idIndex).Value.GetType().FullName);
                        int id = numberToInt(pRow.Value.GetCell(idIndex).Value);
                        CardDefine card = cardList.FirstOrDefault(c => c.id == id);
                        card = readCardDefine(card, pBook.Value, worksheet, pRow.Key, out var skin);
                        skinList.Add(skin);
                    }
                }
            }
            skins = skinList.ToArray();
            return cardList.ToArray();
        }
        static CardDefine readCardDefine(CardDefine card, string dir, Worksheet sheet, int row, out CardSkinData skin)
        {
            if (card == null)
                card = new GeneratedCardDefine();
            int idIndex = findColIndex(sheet, "ID");
            card.id = numberToInt(sheet.Cells[row, idIndex].Value);
            int typeIndex = findColIndex(sheet, "Type");
            if (sheet.Cells[row, typeIndex].StringValue == "Servant")
                card.type = CardDefineType.servant;
            else if (sheet.Cells[row, typeIndex].StringValue == "Spell")
                card.type = CardDefineType.spell;
            else if (sheet.Cells[row, typeIndex].StringValue == "Master")
                card.type = CardDefineType.master;
            else if (sheet.Cells[row, typeIndex].StringValue == "Skill")
                card.type = CardDefineType.skill;
            int costIndex = findColIndex(sheet, "Cost");
            card.setProp(nameof(ServantCardDefine.cost), numberToInt(sheet.Cells[row, costIndex].Value));
            int attackIndex = findColIndex(sheet, "Attack");
            card.setProp(nameof(ServantCardDefine.attack), numberToInt(sheet.Cells[row, attackIndex].Value));
            int lifeIndex = findColIndex(sheet, "Life");
            card.setProp(nameof(ServantCardDefine.life), numberToInt(sheet.Cells[row, lifeIndex].Value));
            int tagsIndex = findColIndex(sheet, "Tags");
            card.setProp(nameof(ServantCardDefine.tags), sheet.Cells[row, tagsIndex].StringValue.Split(','));
            int keywordsIndex = findColIndex(sheet, "Keywords");
            card.setProp(nameof(ServantCardDefine.keywords), sheet.Cells[row, keywordsIndex].StringValue.Split(','));
            int isTokenIndex = findColIndex(sheet, "IsToken");
            card.setProp(nameof(ServantCardDefine.isToken), sheet.Cells[row, isTokenIndex].Value);

            skin = new CardSkinData()
            {
                id = card.id
            };
            int imageIndex = findColIndex(sheet, "Image");
            string imagePath = sheet.Cells[row, imageIndex].StringValue;
            if (File.Exists(dir + "/" + imagePath))
            {
                using (FileStream stream = new FileStream(dir + "/" + imagePath, FileMode.Open))
                {
                    Texture2D texture = new Texture2D(512, 512);
                    byte[] bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, (int)stream.Length);
                    texture.LoadImage(bytes);
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 512, 512), new Vector2(.5f, .5f), 100);
                    skin.image = sprite;
                }
            }
            else if (File.Exists(Application.streamingAssetsPath + "/" + imagePath))
            {
                using (FileStream stream = new FileStream(Application.streamingAssetsPath + "/" + imagePath, FileMode.Open))
                {
                    Texture2D texture = new Texture2D(512, 512);
                    byte[] bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, (int)stream.Length);
                    texture.LoadImage(bytes);
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 512, 512), new Vector2(.5f, .5f), 100);
                    skin.image = sprite;
                }
            }
            else if (File.Exists(Application.streamingAssetsPath + "/" + "Textures/砰砰博士.png"))
            {
                using (FileStream stream = new FileStream(Application.streamingAssetsPath + "/" + "Textures/砰砰博士.png", FileMode.Open))
                {
                    Texture2D texture = new Texture2D(512, 512);
                    byte[] bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, (int)stream.Length);
                    texture.LoadImage(bytes);
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 512, 512), new Vector2(.5f, .5f), 100);
                    skin.image = sprite;
                }
            }

            int nameIndex = findColIndex(sheet, "Name");
            skin.name = sheet.Cells[row, nameIndex].StringValue;
            int descIndex = findColIndex(sheet, "Desc");
            skin.desc = sheet.Cells[row, descIndex].StringValue;
            return card;
        }
        static int numberToInt(object value)
        {
            if (value is double d)
                return (int)d;
            else if (value is int i)
                return i;
            else if (value is float f)
                return (int)f;
            else
                return 0;
        }
        static int findColIndex(Worksheet worksheet, string name)
        {
            foreach (var p in worksheet.Cells.Rows[0])
            {
                if (p.Value.StringValue == name)
                    return p.Key;
            }
            return -1;
        }
    }
}
