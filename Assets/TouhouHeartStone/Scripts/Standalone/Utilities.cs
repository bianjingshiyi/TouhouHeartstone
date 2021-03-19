using UnityEngine;
using System.Xml;
using System.Text.RegularExpressions;

namespace IGensoukyo.Utilities
{
    public static class Utilities
    {
        public static T GetOrAddComponent<T>(this GameObject go) where T : MonoBehaviour
        {
            T comp = go.GetComponent<T>();
            if (comp == null)
                comp = go.AddComponent<T>();
            return comp;
        }

        public static string HTMLToUnityText(string original)
        {
            Regex headerReg = new Regex("<head>.*</head>");
            var result = headerReg.Replace(original, "");
            result = result.Replace("<br>", "\n");
            result = result.Replace("</br>", "\n");
            result = result.Replace("<p>", "");
            result = result.Replace("</p>", "");
            result = result.Replace("<strong>", "<b>");
            result = result.Replace("</strong>", "</b>");
            result = result.Replace("<h1>", "<b>");
            result = result.Replace("</h1>", "</b>");
            result = result.Replace("<h2>", "<b>");
            result = result.Replace("</h2>", "</b>");
            result = result.Replace("<h3>", "");
            result = result.Replace("</h3>", "");
            result = result.Replace("<h4>", "");
            result = result.Replace("</h4>", "");
            result = result.Replace("<ul>", "");
            result = result.Replace("<ol>", "");
            result = result.Replace("<li>", "· ");
            result = result.Replace("</li>", "");
            result = result.Replace("</ul>", "");
            result = result.Replace("</ol>", "");

            result = result.Replace("&lt;", "<");
            result = result.Replace("&gt;", ">");
            result = result.Replace("&amp;", "&");
            result = result.Replace("&quot;", "\"");
            result = result.Replace("&copy;", "©");
            result = result.Replace("&reg;", "®");
            return result;
        }
    
        public static string HTMLRemoveTags(string original) 
        {
            var result = original.Replace("\n", "");
            result = HTMLToUnityText(result);

            return result;
        }
    }
}
