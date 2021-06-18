using System.Collections;
using System.Linq;
using UnityEngine.EventSystems;
using TouhouCardEngine;
using TouhouHeartstone;
using System.Reflection;
using TouhouHeartstone.Builtin;
using System;
using BJSYGameCore.UI;
using System.IO.Compression;
using System.IO;
using MukioI18n;
using NitoriNetwork.Common;
using System.Threading.Tasks;

namespace UI
{
    public partial class ActionNodeGraphPanel
    {
        /// <summary>
        /// 在指定位置上创建一个具有指定名称，输入，常量值和输出的节点。
        /// </summary>
        /// <param name="nodeName"></param>
        /// <param name="outputNames"></param>
        /// <returns></returns>
        public ActionNodeItem createNode(float posX, float posY, string nodeName, string[] inputNames, ActionNodeValueInfo[] constsInfo, string[] outputNames)
        {
            //当对应名称数组长度为0的时候，直接隐藏列表根节点。
            throw new NotImplementedException();
        }
        /// <summary>
        /// 在指定位置上创建一个触发器节点。
        /// </summary>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <param name="eventName"></param>
        /// <param name="targetCheckerCount"></param>
        /// <returns></returns>
        public ActionNodeItem createTriggerNode(float posX, float posY, string eventName, int targetCheckerCount)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 将节点的指定输出连接到目标节点的输入上
        /// </summary>
        /// <param name="node"></param>
        /// <param name="outputIndex"></param>
        /// <param name="targetNode"></param>
        /// <param name="inputIndex"></param>
        public void connectNode(ActionNodeItem node, int outputIndex, ActionNodeItem targetNode, int inputIndex)
        {
            if (node == targetNode)
                throw new ArgumentException("一个节点不能连接自己");
            throw new NotImplementedException();
        }
        /// <summary>
        /// 断开指定节点输出和目标节点输入的连接
        /// </summary>
        /// <param name="node"></param>
        /// <param name="ouputIndex"></param>
        /// <param name="targetNode"></param>
        /// <param name="inputIndex"></param>
        public void unconnectNode(ActionNodeItem node, int ouputIndex, ActionNodeItem targetNode, int inputIndex)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 清空图上的
        /// </summary>
        public void clearGraph()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 在指定坐标展开菜单，左上角对齐。
        /// </summary>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <param name="options"></param>
        public void openMenu(float posX, float posY, string[] options)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 关闭菜单。
        /// </summary>
        public void closeMenu()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 向玩家显示一条错误提示
        /// </summary>
        /// <param name="msg"></param>
        public void showErrorTip(string msg)
        {
            throw new NotImplementedException();
        }
        public void setReturnButtonActive(bool value)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 当点击背景，参数为点击的x坐标与y坐标。
        /// </summary>
        public event Action<float, float> onClickBackground;
        /// <summary>
        /// 当长按背景，参数为长按点的x坐标与y坐标
        /// </summary>
        public event Action<float, float> onLongPressBackground;
        /// <summary>
        /// 当点击菜单项，参数为菜单项索引
        /// </summary>
        public event Action<int> onClickMenuItem;
        /// <summary>
        /// 当连接节点，参数为输出节点和索引以及输入节点和索引
        /// </summary>
        public event Action<ActionNodeItem, int, ActionNodeItem, int> onConnectNodes;
        /// <summary>
        /// 当长按节点，参数为长按的节点
        /// </summary>
        public event Action<ActionNodeItem> onLongPressActionNode;
        /// <summary>
        /// 当节点的常量值输入改变，参数为被改变的节点，常量值索引和改变的值
        /// </summary>
        public event Action<ActionNodeItem, int, object> onActionNodeValueChange;
        /// <summary>
        /// 当触发器节点的事件名称改变，参数为更改后的事件名称
        /// </summary>
        public event Action<string> onChangeTriggerEventName;
        /// <summary>
        /// 当点击触发器节点的编辑条件按钮
        /// </summary>
        public event Action onClickTriggerEditConditionButton;
        /// <summary>
        /// 当点击触发器节点的添加目标按钮
        /// </summary>
        public event Action onClickAddTargetCheckerButton;
        /// <summary>
        /// 当点击触发器节点的编辑目标按钮，参数为点击的按钮的索引
        /// </summary>
        public event Action<int> onClickEditTargetCheckerButton;
        /// <summary>
        /// 当点击触发器节点的删除目标按钮，参数为点击的按钮的索引
        /// </summary>
        public event Action<int> onClickRemoveTargetCheckerButton;
        /// <summary>
        /// 当点击返回之前编辑的动作图按钮
        /// </summary>
        public event Action onClickReturnButton;
    }
    public partial class ActionNodeItem
    {
        public void addTargetCheckerItem()
        {
            throw new NotImplementedException();
        }
        public void removeTargetCheckerItemAt(int index)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 获取位置
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void getPos(out float x, out float y)
        {
            throw new NotImplementedException();
        }
    }
    public struct ActionNodeValueInfo
    {
        public string name;
        public ActionNodeValueType type;
        public object value;
    }
    public enum ActionNodeValueType
    {
        /// <summary>
        /// 整数
        /// </summary>
        integer,
        /// <summary>
        /// 实数
        /// </summary>
        real,
        /// <summary>
        /// 真值
        /// </summary>
        boolean,
        /// <summary>
        /// 字符串
        /// </summary>
        text
    }
}