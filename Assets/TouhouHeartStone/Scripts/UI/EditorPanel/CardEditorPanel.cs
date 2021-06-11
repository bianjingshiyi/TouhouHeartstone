using System;
using UnityEngine;
using UnityEngine.UI;
namespace UI
{
    public partial class CardEditorPanel
    {
        /// <summary>
        /// 设置皮肤切换选项
        /// </summary>
        /// <param name="skinNames"></param>
        public void setSkinNames(string[] skinNames)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 设置当前皮肤索引
        /// </summary>
        /// <param name="skinIndex"></param>
        public void setSkin(int skinIndex)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 设置当前语言索引
        /// </summary>
        /// <param name="languageIndex"></param>
        public void setLanguage(int languageIndex)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 设置可选的卡牌类型
        /// </summary>
        /// <param name="types"></param>
        public void setTypes(string[] types)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 设置卡牌的卡框类型索引
        /// </summary>
        /// <param name="typeIndex"></param>
        public void setType(int typeIndex)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 设置费用
        /// </summary>
        /// <param name="cost"></param>
        public void setCost(string cost)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 设置卡牌名称
        /// </summary>
        /// <param name="name"></param>
        public void setName(string name)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 设置卡牌副名
        /// </summary>
        /// <param name="subName"></param>
        public void setSubName(string subName)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 设置卡牌的图片。
        /// </summary>
        /// <param name="imageResPath"></param>
        public void setImage(string imageResPath)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 设置卡牌的描述
        /// </summary>
        /// <param name="text"></param>
        public void setDesc(string text)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 设置卡牌的攻击力
        /// </summary>
        /// <param name="attack"></param>
        public void setAttack(string attack)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 设置卡牌的生命值
        /// </summary>
        /// <param name="life"></param>
        public void setLife(string life)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 设置卡牌的标签
        /// </summary>
        /// <param name="tag"></param>
        public void setTag(string tag)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 当更换了皮肤，参数为更换的皮肤索引。
        /// </summary>
        public event Action<int> onChangeSkin;
        /// <summary>
        /// 当更换了语言，参数为更换的语言索引。
        /// </summary>
        public event Action<int> onChangeLanguage;
        /// <summary>
        /// 当更换卡牌类型，参数为更改类型索引。
        /// </summary>
        public event Action<int> onChangeType;
        /// <summary>
        /// 当卡牌费用值被更改，参数为用户输入费用。
        /// </summary>
        public event Action<string> onChangeCost;
        /// <summary>
        /// 当卡牌名称被更改，参数为更改后的名称。
        /// </summary>
        public event Action<string> onChangeName;
        /// <summary>
        /// 当卡牌副名被更改，参数为更改后的卡牌副名。
        /// </summary>
        public event Action<string> onChangeSubName;
        /// <summary>
        /// 当选择图片，参数为选择的图片的路径，可能为空。
        /// </summary>
        public event Action<string> onSelectImage;
        /// <summary>
        /// 当更改卡牌描述，参数为更改后的卡牌描述。
        /// </summary>
        public event Action<string> onChangeDesc;
        /// <summary>
        /// 当更改卡牌攻击力，参数为更改输入的攻击力。
        /// </summary>
        public event Action<string> onChangeAttack;
        /// <summary>
        /// 当更改卡牌生命值，参数为更改输入的生命值。
        /// </summary>
        public event Action<string> onChangeLife;
        /// <summary>
        /// 当更改卡牌标签，参数为更改后的卡牌标签。
        /// </summary>
        public event Action<string> onChangeTag;
        /// <summary>
        /// 当点击编辑效果按钮
        /// </summary>
        public event Action onClickEditEffectButton;
    }
}