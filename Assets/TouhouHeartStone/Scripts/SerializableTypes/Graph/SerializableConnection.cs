using MessagePack;
using System;
using System.Linq;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace Game
{
    [MessagePackObject]
    [Serializable]
    public class SerializableConnection
    {
        /// <summary>
        /// 给序列化用的默认构造器
        /// </summary>
        public SerializableConnection()
        {
        }
        public SerializableConnection(NodeConnection connection)
        {
            sourceName = connection.source.define.name;
            sourceNodeId = connection.source.node.id;
            destName = connection.destination.define.name;
            destNodeId = connection.destination.node.id;
            if (connection.destination is ValueInput valueInput)
            {
                destParamIndex = valueInput.paramIndex + 1;
            }
        }
        public NodeConnection ToNodeConnection(ActionGraph graph)
        {
            var sourceNode = graph.nodes.First(n => n.id == sourceNodeId);
            var destNode = graph.nodes.First(n => n.id == destNodeId);
            var sourcePort = sourceNode.getOutputPort(sourceName);
            IPort destPort;
            if (destParamIndex > 0)
            {
                var paramIndex = destParamIndex - 1;
                if (destNode is ActionNode action)
                {
                    int length = destNode.getParamInputPorts(destName).Length;
                    while (paramIndex + 1 >= length && length > 0)
                    {
                        action.extendParamsPort(destName);
                        length = destNode.getParamInputPorts(destName).Length;
                    }
                }
                destPort = destNode.getParamInputPort(destName, paramIndex);
            }
            else
            {
                destPort = destNode.getInputPort(destName);
            }
            if (sourcePort == null || destPort == null)
                return new InvalidNodeConnection(sourcePort, sourceName, sourceNodeId, destPort, destName, destNodeId, destParamIndex);

            return new NodeConnection(sourcePort, destPort);
        }
        [Key(0)]
        public string sourceName;
        [Key(1)]
        public int sourceNodeId;
        [Key(2)]
        public string destName;
        [Key(3)]
        public int destParamIndex;
        [Key(4)]
        public int destNodeId;
    }
}
