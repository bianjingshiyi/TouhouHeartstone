using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class GameRuleEntryNode : Node, IEntryNode
    {
        #region 公有方法

        #region 构造器
        public GameRuleEntryNode(int id, GameRuleLogicType logicType)
        {
            this.id = id;
            this.logicType = logicType;
            foreach (var portDefine in getInputDefines())
            {
                inputList.Add(portDefine.GetPortType() == PortType.Control ? new ControlInput(this, portDefine) : new ValueInput(this, portDefine));
            }
            outputList.Add(new ControlOutput(this, actionPortDefine));
            foreach (var portDefine in getOutputDefines())
            {
                outputList.Add(portDefine.GetPortType() == PortType.Control ? new ControlOutput(this, portDefine) : new ValueOutput(this, portDefine));
            }
        }
        #endregion

        public override Task<ControlOutput> run(Flow flow)
        {
            foreach (var output in outputPorts.OfType<ValueOutput>())
            {
                flow.setValue(output, flow.env.GetArgument(output.name));
            }
            return Task.FromResult(getActionOutputPort());
        }
        public void Define()
        {
            DefinitionInputs();
            DefinitionOutputs();
        }
        public ControlOutput getActionOutputPort()
        {
            return outputList?.FirstOrDefault() as ControlOutput;
        }
        #endregion

        #region 私有方法

        private void DefinitionInputs()
        {
            ControlInput controlInput(PortDefine portDefine)
                 => inputList.OfType<ControlInput>().FirstOrDefault(o => o.define.Equals(portDefine)) ??
                 new ControlInput(this, portDefine);
            ValueInput valueInput(PortDefine portDefine)
                 => inputList.OfType<ValueInput>().FirstOrDefault(o => o.define.Equals(portDefine)) ??
                 new ValueInput(this, portDefine);

            var inputs = new List<IPort>();
            foreach (var portDefine in getInputDefines())
            {
                inputs.Add(portDefine.GetPortType() == PortType.Control ? controlInput(portDefine) : valueInput(portDefine));
            }
            foreach (var lostPort in inputList.Except(inputs))
            {
                graph.disconnectAll(lostPort);
            }
            inputList.Clear();
        }
        private void DefinitionOutputs()
        {
            ControlOutput controlOutput(PortDefine portDefine)
                 => outputList.OfType<ControlOutput>().FirstOrDefault(o => o.define.Equals(portDefine)) ??
                 new ControlOutput(this, portDefine);
            ValueOutput valueOutput(PortDefine portDefine)
                 => outputList.OfType<ValueOutput>().FirstOrDefault(o => o.define.Equals(portDefine)) ??
                 new ValueOutput(this, portDefine);


            var outputs = new List<IPort>()
            {
                controlOutput(actionPortDefine),
            };
            foreach (var portDefine in getOutputDefines())
            {
                outputs.Add(portDefine.GetPortType() == PortType.Control ? controlOutput(portDefine) : valueOutput(portDefine));
            }

            foreach (var lostPort in outputList.Except(outputs))
            {
                graph.disconnectAll(lostPort);
            }
            outputList.Clear();
            outputList.AddRange(outputs);
        }

        protected virtual IEnumerable<PortDefine> getOutputDefines()
        {
            if (logicType == GameRuleLogicType.PlayerEnter || logicType == GameRuleLogicType.PlayerExit)
            {
                yield return playerPortDefine;
            }
        }
        protected virtual IEnumerable<PortDefine> getInputDefines()
        {
            yield break;
        }

        #endregion

        public const string actionPortName = "action";
        public const string playerPortName = "玩家";
        public GameRuleLogicType logicType { get; private set; }
        private static PortDefine actionPortDefine = PortDefine.Control(actionPortName, string.Empty);
        private static PortDefine playerPortDefine = PortDefine.Value(typeof(Player), playerPortName);
    }
}