using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UI
{
    partial class ActionNodeGraphPanel
    {
        #region 公有方法
        public void setPreviewNode(string nodeName,
            PortItemInfo[] inputViewData,
            ActionNodeValueInfo[] constsInfo,
            PortItemInfo[] outputViewData,
            string obsoleteMsg = null,
            int docKey = -1)
        {
            ActionNodeItem actionNodeItem = _simulationNode;

            _simulationNodeCanvasGroup.alpha = 0.5f;
            actionNodeItem.switchToActionNode();
            actionNodeItem.setName(nodeName);
            actionNodeItem.setInputs(inputViewData);
            actionNodeItem.setConsts(constsInfo);
            actionNodeItem.setOutputs(outputViewData);
            actionNodeItem.setObsoleteMsg(obsoleteMsg);
            actionNodeItem.setDocumentKey(docKey);
        }
        public void setPreviewEventNode(string nodeName,
            PortItemInfo[] inputViewData,
            PortItemInfo[] outputViewData)
        {
            ActionNodeItem actionNodeItem = _simulationNode;

            _simulationNodeCanvasGroup.alpha = 0.5f;
            actionNodeItem.switchToActionNode();
            actionNodeItem.setName(nodeName);
            actionNodeItem.setInputs(inputViewData);
            actionNodeItem.setOutputs(outputViewData);
            actionNodeItem.switchToEventEntryNode();
        }
        public void setPreviewActiveEntryNode(string nodeName,
            PortItemInfo[] inputViewData,
            PortItemInfo[] outputViewData,
            string[] targetCheckerTypes, (int typeIndex, string name, string errorTip)[] targetCheckers)
        {
            ActionNodeItem actionNodeItem = _simulationNode;
            actionNodeItem.setName(nodeName);
            actionNodeItem.switchToActiveEntryNode();
            actionNodeItem.setActiveEntry(targetCheckerTypes, targetCheckers);
            actionNodeItem.setInputs(inputViewData);
            actionNodeItem.setOutputs(outputViewData);
        }
        public void hidePreviewNode()
        {
            _simulationNodeCanvasGroup.alpha = 0;
        }
        public void setPreviewNodePosition(Vector2 pos)
        {
            (_simulationNode.transform as RectTransform).anchoredPosition = pos;
        }
        public Vector2 getPreviewNodePosition()
        {
            return (_simulationNode.transform as RectTransform).anchoredPosition;
        }
        public Vector2 getPreviewNodeSize()
        {
            var rectTransform = _simulationNode.transform as RectTransform;
            return rectTransform.rect.size;
        }
        public void disconnectPreviewPorts()
        {
            foreach (var lineData in _previewLines)
            {
                Destroy(lineData.line.gameObject);
            }
            _previewLines.Clear();
        }
        public void connectPreviewPorts(ActionNodeItem nodeItem, int nodePortIndex, int previewIndex, bool isOutput)
        {
            ActionNodeItem outputItem;
            ActionNodeItem inputItem;
            int outputIndex;
            int inputIndex;
            if (isOutput)
            {
                outputItem = nodeItem;
                outputIndex = nodePortIndex;
                inputItem = _simulationNode;
                inputIndex = previewIndex;
            }
            else
            {
                outputItem = _simulationNode;
                outputIndex = previewIndex;
                inputItem = nodeItem;
                inputIndex = nodePortIndex;
            }
            var outputPort = outputItem.getOutput(outputIndex);
            var inputPort = inputItem.getInput(inputIndex);
            var line = createPreviewLine(outputPort.gameObject, inputPort.gameObject);
            var outputColor = outputPort.GetColor();
            var inputColor = inputPort.GetColor();
            line.SetColor(outputColor, inputColor);

            _previewLines.Add(new ActionNodeConnectionData()
            {
                outputNode = outputItem,
                outputIndex = outputIndex,
                inputNode = inputItem,
                inputIndex = inputIndex,
                line = line
            });
        }
        #endregion
        #region 私有方法
        private ActionNodeGraphLine createPreviewLine(GameObject from, GameObject to)
        {
            ActionNodeGraphLine line = Instantiate(_previewLineTemplate, _previewLinesRoot);
            line.gameObject.SetActive(true);
            line.setPosition(from, to);
            return line;
        }
        #endregion
        #region 属性字段
        [Header("吸附预览/模拟")]
        [SerializeField]
        ActionNodeItem _simulationNode;
        [SerializeField]
        CanvasGroup _simulationNodeCanvasGroup;
        [SerializeField]
        RectTransform _previewLinesRoot;
        [SerializeField]
        List<ActionNodeConnectionData> _previewLines;
        [SerializeField]
        ActionNodeGraphLine _previewLineTemplate;
        #endregion
    }
}