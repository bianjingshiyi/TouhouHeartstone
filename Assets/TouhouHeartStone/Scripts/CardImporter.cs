using System.Collections.Generic;
using System.Linq;
using System.Data;
using UnityEngine;
using TouhouHeartstone;
using TouhouCardEngine;
using UI;
using System.Reflection;
using System.Threading.Tasks;
using ExcelLibrary.SpreadSheet;
using System;
using IGensoukyo.Utilities;

namespace Game
{
//    static class CardImporter
//    {
//        public static async Task<KeyValuePair<CardDefine[], CardSkinData[]>> GetCardDefines(ResourceManager resource, Assembly[] assemblies, Dictionary<Workbook, string> workbooks, Sprite defaultSprite)
//        {
//            List<CardDefine> cardList = new List<CardDefine>(CardHelper.getCardDefines(assemblies));
//            List<CardSkinData> skinList = new List<CardSkinData>();
//            foreach (var pBook in workbooks)
//            {
//                Worksheet worksheet = pBook.Key.Worksheets[0];
//                int idIndex = findColIndex(worksheet, "ID");
//                foreach (var pRow in worksheet.Cells.Rows)
//                {
//                    if (pRow.Key != 0)
//                    {
//                        int id = numberToInt(pRow.Value.GetCell(idIndex).Value);
//                        if (id < 1)
//                            continue;
//                        CardDefine card = cardList.FirstOrDefault(c => c.id == id);
//                        CardSkinData skin;
//                        var pair = await readCardDefine(resource, card, worksheet, pRow.Key, defaultSprite);
//                        card = pair.Key;
//                        skin = pair.Value;
//                        if (card == null)
//                        {
//                            cardList.RemoveAll(c => c.id == id);
//                            Debug.Log("忽略卡片" + id);
//                        }
//                        else
//                        {
//                            cardList.Add(card);
//                            skinList.Add(skin);
//                            Debug.Log("读取Excel卡片" + card + "，皮肤：" + skin);
//                        }
//                    }
//                }
//            }
//            return new KeyValuePair<CardDefine[], CardSkinData[]>(cardList.ToArray(), skinList.ToArray());
//        }

//        public static async Task<KeyValuePair<CardDefine[], CardSkinData[]>> GetCardDefines(ResourceManager resource, Assembly[] assemblies, Dictionary<DataSet, string> tables, Sprite defaultSprite)
//        {
//            List<CardDefine> cardList = new List<CardDefine>(CardHelper.getCardDefines(assemblies));
//            List<CardSkinData> skinList = new List<CardSkinData>();
//            foreach (var pTable in tables)
//            {
//                DataTable worksheet = pTable.Key.Tables[0];
//                for (int i = 1; i < worksheet.Rows.Count; i++)
//                {
//                    var row = worksheet.Rows[i];

//                    int id = row.ReadCol<int>("ID");
//                    if (id < 1)
//                        continue;
//                    CardDefine card = cardList.FirstOrDefault(c => c.id == id);
//                    CardSkinData skin;
//                    var pair = await readCardDefine(resource, card, row, defaultSprite);
//                    card = pair.Key;
//                    skin = pair.Value;
//                    if (card == null)
//                    {
//                        cardList.RemoveAll(c => c.id == id);
//                        Debug.Log("忽略卡片" + id);
//                    }
//                    else
//                    {
//                        cardList.Add(card);
//                        skinList.Add(skin);
//                        Debug.Log("读取Excel卡片" + card + "，皮肤：" + skin);
//                    }
//                }
//            }
//            return new KeyValuePair<CardDefine[], CardSkinData[]>(cardList.ToArray(), skinList.ToArray());
//        }

//        static async Task<KeyValuePair<CardDefine, CardSkinData>> readCardDefine(ResourceManager resource, CardDefine card, Worksheet sheet, int row, Sprite defaultSprite)
//        {
//            if (card == null)
//                card = new GeneratedCardDefine();
//            int idIndex = findColIndex(sheet, "ID");
//            card.id = numberToInt(sheet.Cells[row, idIndex].Value);
//            int typeIndex = findColIndex(sheet, "Type");
//            card.type = sheet.Cells[row, typeIndex].StringValue;
//            int costIndex = findColIndex(sheet, "Cost");
//            card.setProp(nameof(ServantCardDefine.cost), numberToInt(sheet.Cells[row, costIndex].Value));
//            int attackIndex = findColIndex(sheet, "Attack");
//            card.setProp(nameof(ServantCardDefine.attack), numberToInt(sheet.Cells[row, attackIndex].Value));
//            int lifeIndex = findColIndex(sheet, "Life");
//            card.setProp(nameof(ServantCardDefine.life), numberToInt(sheet.Cells[row, lifeIndex].Value));
//            int tagsIndex = findColIndex(sheet, "Tags");
//            card.setProp(nameof(ServantCardDefine.tags), sheet.Cells[row, tagsIndex].StringValue.Split(','));
//            int keywordsIndex = findColIndex(sheet, "Keywords");
//            card.setProp(nameof(ServantCardDefine.keywords), sheet.Cells[row, keywordsIndex].StringValue.Split(','));
//            int isTokenIndex = findColIndex(sheet, "IsToken");
//            card.setProp(nameof(ServantCardDefine.isToken), sheet.Cells[row, isTokenIndex].Value);
//            int ignoreIndex = findColIndex(sheet, "Ignore");
//            CardSkinData skin;
//            if (sheet.Cells[row, ignoreIndex].Value is bool b && b)
//            {
//                return new KeyValuePair<CardDefine, CardSkinData>(null, null);
//            }

//            skin = new CardSkinData()
//            {
//                id = card.id
//            };
//            int imageIndex = findColIndex(sheet, "Image");
//            string imagePath = sheet.Cells[row, imageIndex].StringValue;
//            try
//            {
//                Texture2D texture = await resource.loadTexture(imagePath);
//                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f), 100);
//                skin.image = sprite;
//            }
//            catch (Exception e)
//            {
//                skin.image = defaultSprite;
//                Debug.LogWarning("加载贴图" + imagePath + "失败：" + e);
//            }

//            int nameIndex = findColIndex(sheet, "Name");
//            skin.name = sheet.Cells[row, nameIndex].StringValue;
//            int descIndex = findColIndex(sheet, "Desc");
//            skin.desc = sheet.Cells[row, descIndex].StringValue;
//            return new KeyValuePair<CardDefine, CardSkinData>(card, skin);
//        }
        
//        static async Task<KeyValuePair<CardDefine, CardSkinData>> readCardDefine(ResourceManager resource, CardDefine card, DataRow datarow, Sprite defaultSprite)
//        {
//            if (card == null)
//                card = new GeneratedCardDefine();

//            card.id = datarow.ReadCol<int>("ID");
//            card.type = datarow.ReadCol<string>("Type");
//            card.setProp(nameof(ServantCardDefine.cost), datarow.ReadCol<int>("Cost"));
//            card.setProp(nameof(ServantCardDefine.attack), datarow.ReadCol<int>("Attack", 0));
//            card.setProp(nameof(ServantCardDefine.life), datarow.ReadCol<int>("Life", 0));
//            card.setProp(nameof(ServantCardDefine.tags), datarow.ReadCol<string>("Tags", "").Split(','));
//            card.setProp(nameof(ServantCardDefine.keywords), datarow.ReadCol<string>("Keywords", "").Split(','));
//            card.setProp(nameof(ServantCardDefine.isToken), datarow.ReadCol<bool>("IsToken", false));
//            CardSkinData skin;
//            if (datarow.ReadCol("Ignore", false))
//            {
//                return new KeyValuePair<CardDefine, CardSkinData>(null, null);
//            }

//            skin = new CardSkinData()
//            {
//                id = card.id
//            };
//            string imagePath = datarow.ReadCol("Image", "");
//            try
//            {
//                Texture2D texture = await resource.loadTexture(imagePath);
//                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f), 100);
//                skin.image = sprite;
//            }
//            catch (Exception e)
//            {
//                skin.image = defaultSprite;
//                Debug.LogWarning("加载贴图" + imagePath + "失败：" + e);
//            }

//            skin.name = datarow.ReadCol<string>("Name");
//            skin.desc = datarow.ReadCol<string>("Desc", "");
//            return new KeyValuePair<CardDefine, CardSkinData>(card, skin);
//        }

//        static string fixPath(string path)
//        {
//#if UNITY_ANDROID
//            path.ToLower();
//#endif
//            return path;
//        }
//        static int numberToInt(object value)
//        {
//            if (value is double d)
//                return (int)d;
//            else if (value is int i)
//                return i;
//            else if (value is float f)
//                return (int)f;
//            else
//                return 0;
//        }
//        static int findColIndex(Worksheet worksheet, string name)
//        {
//            foreach (var p in worksheet.Cells.Rows[0])
//            {
//                if (p.Value.StringValue == name)
//                    return p.Key;
//            }
//            return -1;
//        }
//    }
}
