using MukioI18n;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// 边境求求你整理一下你那sb一样的StringTable吧
    /// </summary>
    public partial class StringTable
    {
        #region 行动
        [TranslateMsg("编辑器/行动_集合名称")]
        public const string EDITOR_CATEGORY__ACTION__NAME = "行动";

            #region 简单抽牌节点
            [TranslateMsg("编辑器/行动/简单抽牌节点_名称")]
            public const string EDITOR_NODE__SIMPLE_DRAW_CARD__NAME = "简单抽牌";
            [TranslateMsg("编辑器/行动/简单抽牌节点_参数_抽牌玩家")]
            public const string EDITOR_NODE__SIMPLE_DRAW_CARD__PLAYER = "Player";
            [TranslateMsg("编辑器/行动/简单抽牌节点_参数_抽牌事件")]
            public const string EDITOR_NODE__SIMPLE_DRAW_CARD__DRAW_EVENT = "Event";
            #endregion

            #region 简单召唤
            [TranslateMsg("编辑器/行动/简单召唤召唤物_名称")]
            public const string EDITOR_NODE__SIMPLE_CREATE_TOKEN__NAME = "简单召唤召唤物";
            [TranslateMsg("编辑器/行动/简单抽牌节点_参数_召唤玩家")]
            public const string EDITOR_NODE__SIMPLE_CREATE_TOKEN__PLAYER = "Player";
            [TranslateMsg("编辑器/行动/简单抽牌节点_参数_召唤物定义")]
            public const string EDITOR_NODE__SIMPLE_CREATE_TOKEN__DEFINE = "Define";
        #endregion


        #endregion
    }

}

