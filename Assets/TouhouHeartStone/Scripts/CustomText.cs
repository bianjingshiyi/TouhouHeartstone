using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

 public class CustomText : Text
    {
        private readonly string mArkList = @"(\！|\？|\，|\。|\(|\)|\：|\“|\‘|\、|\；|\+|\-)";

        public override void SetVerticesDirty()
        {
            var settings = GetGenerationSettings(rectTransform.rect.size);
            cachedTextGenerator.Populate(text, settings);

            StringBuilder stringBuilder = new StringBuilder(text);
            IList<UILineInfo> lineList = cachedTextGenerator.lines;
            int changeIndex = -1;
            for (int i = 1; i < lineList.Count; i++)
            {
                bool isMark = Regex.IsMatch(text[lineList[i].startCharIdx].ToString(), mArkList);

                if (isMark)
                {
                    bool b = Regex.IsMatch(text[lineList[i].startCharIdx].ToString(), mArkList);
                    if (b)
                    {
                        changeIndex = lineList[i].startCharIdx - 1;
                        break;
                    }
                }
            }

            if (changeIndex >= 0)
            {
                stringBuilder.Insert(changeIndex, '\n');
                text = stringBuilder.ToString();
            }

            base.SetVerticesDirty();
        }
    }


