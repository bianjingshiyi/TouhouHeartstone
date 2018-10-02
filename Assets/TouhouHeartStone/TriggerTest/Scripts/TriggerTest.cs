using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TriggerSystem;

namespace Tests
{
    public class TriggerTest : MonoBehaviour
    {
        [TriggerString]
        [SerializeField]
        string _triggerString;
        [TriggerAction("日志","日志：{0}")]
        public static void log(string text)
        {
            Debug.Log(text);
        }
        [TriggerAction("警告","警告：{0}")]
        public static void warning(string text)
        {
            Debug.LogWarning(text);
        }
        [TriggerAction("错误","错误：{0}")]
        public static void error(string text)
        {
            Debug.LogWarning(text);
        }
    }
}