using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BJSYGameCore;
using MukioI18n;
using NitoriNetwork.Common;
using TouhouCardEngine;
using UI;
using UnityEngine;
using NitoriNetwork.Common;

namespace Game
{
    public partial class NetworkingManager : Manager
    {
        #region 公有方法
        public string translateNetErrorMessage(Exception exception)
        {
            if (exception is KratosNetworkException kratosException)
            {
                var msgs = kratosException.Messages.Select(m => m.text).Distinct().Select(t => translateNetErrorMessage(t));
                var fullMsg = string.Empty;
                foreach (var msg in msgs)
                {
                    if (string.IsNullOrEmpty(fullMsg))
                    {
                        fullMsg = msg;
                    }
                    else
                    {
                        fullMsg = main.lang._(ERROR_TRANSLATION_SEPERATOR_TEMPLATE, fullMsg, msg);
                    }
                }
                return fullMsg;
            }
            else
            {
                return translateNetErrorMessage(exception.Message);
            }
        }
        public string translateNetErrorMessage(string message)
        {
            if (_netErrorMsgMap.TryGetValue(message, out var translateKey))
            {
                return main.lang._(translateKey);
            }
            if (tryTranslateRegexError(message, out var regexTranslation))
            {
                return regexTranslation;
            }
            return message;
        }
        #endregion 公有方法

        #region 私有方法
        public bool tryTranslateRegexError(string message, out string translated)
        {
            foreach (var pair in _regexErrorMsgMap)
            {
                var match = Regex.Match(message, pair.Key);
                if (match.Success)
                {
                    translated = main.lang._(pair.Value, match.Groups.Skip(1).Select(g => g.Value).ToArray());
                    return true;
                }
            }
            translated = null;
            return false;
        }
        #endregion

        #region 属性字段
        public const string ERROR_MSG_EMAIL_OR_PASSWORD_WRONG = "The provided credentials are invalid, check for spelling mistakes in your password or username, email address, or phone number.";
        public const string ERROR_MSG_PASSWORD_NULL = "Property password is missing.";
        public const string ERROR_MSG_ACCOUNT_REGISTERED = "An account with the same identifier (email, phone, username, ...) exists already.";
        public const string ERROR_MSG_CANNOT_UNLINK = "can not unlink OpenID Connect connection because it is the last remaining first factor credential";

        public const string ERROR_MSG_REGEX_INVALID_EMAIL = "\"(.+?)\" is not valid \"email\"";
        public const string ERROR_MSG_REGEX_INVALID_VALUE = "\"(.+?)\" is not valid \"(.+?)\"";
        public const string ERROR_MSG_REGEX_LENGTH_TOO_SHORT = "length must be >= ([0-9]+), but got ([0-9]+)";
        public const string ERROR_MSG_REGEX_PASSWORD_LENGTH = "The password must be at least ([0-9]+) characters long, but got ([0-9]+)\\.";

        [TranslateMsg("网络错误")]
        public const string ERROR_TRANSLATION_ACCOUNT_REGISTERED = "该帐号已被注册";
        [TranslateMsg("网络错误")]
        public const string ERROR_TRANSLATION_CANNOT_UNLINK = "无法解绑帐号，必须先为该账号设置密码";

        [TranslateMsg("网络错误，{0}为输入的内容")]
        public const string ERROR_TRANSLATION_INVALID_EMAIL = "\"{0}\"不是有效的邮箱";
        [TranslateMsg("网络错误，{0}为输入的内容，{1}为值类型")]
        public const string ERROR_TRANSLATION_INVALID_VALUE = "\"{0}\"不是有效的\"{1}\"";
        [TranslateMsg("网络错误，{0}为最短长度，{1}为目前长度")]
        public const string ERROR_TRANSLATION_LENGTH_TOO_SHORT = "长度至少为{0}（目前为{1}）";
        [TranslateMsg("网络错误，{0}为最短密码长度，{1}为目前长度")]
        public const string ERROR_TRANSLATION_PASSWORD_LENGTH = "密码长度至少为{0}（目前为{1}）";

        [TranslateMsg("网络错误分割符模板，{0}和{1}都为错误信息")]
        public const string ERROR_TRANSLATION_SEPERATOR_TEMPLATE = "{0}；{1}";

        public static Dictionary<string, string> _netErrorMsgMap = new Dictionary<string, string>()
        {
            { ERROR_MSG_EMAIL_OR_PASSWORD_WRONG, StringTable.ACCOUNT_EMAIL_OR_PASSWORD_WRONG },
            { ERROR_MSG_PASSWORD_NULL, StringTable.ACCOUNT_PASSWORD_NULL },
            { ERROR_MSG_ACCOUNT_REGISTERED, ERROR_TRANSLATION_ACCOUNT_REGISTERED },
            { ERROR_MSG_CANNOT_UNLINK, ERROR_TRANSLATION_CANNOT_UNLINK }
        };
        public static Dictionary<string, string> _regexErrorMsgMap = new Dictionary<string, string>()
        {
            { ERROR_MSG_REGEX_INVALID_EMAIL, ERROR_TRANSLATION_INVALID_EMAIL },
            { ERROR_MSG_REGEX_INVALID_VALUE, ERROR_TRANSLATION_INVALID_VALUE },
            { ERROR_MSG_REGEX_LENGTH_TOO_SHORT, ERROR_TRANSLATION_LENGTH_TOO_SHORT },
            { ERROR_MSG_REGEX_PASSWORD_LENGTH, ERROR_TRANSLATION_PASSWORD_LENGTH },
        };
        #endregion 属性字段

    }
}