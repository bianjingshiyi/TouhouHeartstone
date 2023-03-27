using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;
using TouhouHeartstone;
using Game;
using TouhouCardEngine;
using Controllers;
using TouhouCardEngine.Interfaces;

namespace Tests
{
    public class ActionGraphV2Test
    {
        [Test]
        public void RunActionsTest()
        {
            CardEngine game = new CardEngine();
            var constDefine = new IntegerConstActionDefine();
            var compareDefine = new CompareActionDefine();
            game.addActionDefine("IntegerConst", constDefine);
            game.addActionDefine("Compare", compareDefine);
            ActionGraph graph = new ActionGraph();
            var compareNode = graph.createActionNode(compareDefine);
            var intNode1 = graph.createActionNode(constDefine);
            var intNode2 = graph.createActionNode(constDefine);

            graph.connect(intNode1.getOutputPort<ValueOutput>("result"), compareNode.getInputPort<ValueInput>("A"));
            graph.connect(intNode2.getOutputPort<ValueOutput>("result"), compareNode.getInputPort<ValueInput>("B"));

            compareNode.setConst("operator", CompareOperator.equals);
            intNode1.setConst("value", 40);
            intNode2.setConst("value", 18);
            Assert.AreEqual(false, game.getValue<bool>(null, null, null, compareNode.getOutputPort<ValueOutput>("result")).Result);

            compareNode.setConst("operator", CompareOperator.unequals);
            intNode1.setConst("value", 40);
            intNode2.setConst("value", 40);
            Assert.AreEqual(false, game.getValue<bool>(null, null, null, compareNode.getOutputPort<ValueOutput>("result")).Result);

            compareNode.setConst("operator", CompareOperator.equals);
            intNode1.setConst("value", 40);
            intNode2.setConst("value", 40);
            Assert.AreEqual(true, game.getValue<bool>(null, null, null, compareNode.getOutputPort<ValueOutput>("result")).Result);
        }
        [Test]
        public void ParamsTest()
        {
            CardEngine game = new CardEngine();
            var constDefine = new IntegerConstActionDefine();
            var opDefine = new IntegerOperationActionDefine();
            game.addActionDefine("IntegerConst", constDefine);
            game.addActionDefine("IntegerOperation", opDefine);
            ActionGraph graph = new ActionGraph();
            var intOpNode = graph.createActionNode(opDefine);
            var intNode1 = graph.createActionNode(constDefine);
            var intNode2 = graph.createActionNode(constDefine);

            intOpNode.setConst("operator", IntegerOperator.add);
            intNode1.setConst("value", 40);
            intNode2.setConst("value", 18);

            graph.connect(intNode1.getOutputPort<ValueOutput>("result"), intOpNode.getParamInputPort("arg", 0));
            graph.connect(intNode2.getOutputPort<ValueOutput>("result"), intOpNode.getParamInputPort("arg", 1));

            Assert.AreEqual(58, game.getValue<int>(null, null, null, intOpNode.getOutputPort<ValueOutput>("result")).Result);

        }
    }
}