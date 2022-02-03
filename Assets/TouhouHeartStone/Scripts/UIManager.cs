using UnityEngine;
using UI;
using System;
using System.Collections.Generic;

namespace Game
{
    public class UIManager : BJSYGameCore.UI.UIManager
    {
        public string getTranslateNameForLanguage(string languageName)
        {
            if (_languageTranslateNameMapList.Find(m => m.languageName == languageName) is LanguageTranslateNamePair pair)
            {
                return pair.translateName;
            }
            else
                return languageName;
        }
        #region 属性字段
        public UIDialog dialog => _dialog;
        [SerializeField]
        UIDialog _dialog;
        [SerializeField]
        List<LanguageTranslateNamePair> _languageTranslateNameMapList;
        #endregion
        #region 内部类
        [Serializable]
        class LanguageTranslateNamePair
        {
            public string languageName;
            public string translateName;
        }
        #endregion
    }
}
