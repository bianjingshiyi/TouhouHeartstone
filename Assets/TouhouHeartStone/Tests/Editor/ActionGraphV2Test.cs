using NUnit.Framework;
using TouhouCardEngine;

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
            game.addActionDefine(constDefine);
            game.addActionDefine(compareDefine);
            ActionGraph graph = new ActionGraph();
            var compareNode = graph.createActionNode(compareDefine);
            var intNode1 = graph.createActionNode(constDefine);
            var intNode2 = graph.createActionNode(constDefine);

            graph.connect(intNode1.getOutputPort<ValueOutput>("result"), compareNode.getInputPort<ValueInput>("A"));
            graph.connect(intNode2.getOutputPort<ValueOutput>("result"), compareNode.getInputPort<ValueInput>("B"));

            Flow flow;
            flow = new Flow(game, null, null, null, null);
            compareNode.setInputDefaultValue("operator", CompareOperator.equals);
            intNode1.setInputDefaultValue("value", 40);
            intNode2.setInputDefaultValue("value", 18);
            Assert.AreEqual(false, game.getValue<bool>(flow, compareNode.getOutputPort<ValueOutput>("result")).Result);

            flow = new Flow(game, null, null, null, null);
            compareNode.setInputDefaultValue("operator", CompareOperator.unequals);
            intNode1.setInputDefaultValue("value", 40);
            intNode2.setInputDefaultValue("value", 40);
            Assert.AreEqual(false, game.getValue<bool>(flow, compareNode.getOutputPort<ValueOutput>("result")).Result);

            flow = new Flow(game, null, null, null, null);
            compareNode.setInputDefaultValue("operator", CompareOperator.equals);
            intNode1.setInputDefaultValue("value", 40);
            intNode2.setInputDefaultValue("value", 40);
            Assert.AreEqual(true, game.getValue<bool>(flow, compareNode.getOutputPort<ValueOutput>("result")).Result);
        }
        [Test]
        public void ParamsTest()
        {
            CardEngine game = new CardEngine();
            var constDefine = new IntegerConstActionDefine();
            var opDefine = new IntegerOperationActionDefine();
            game.addActionDefine(constDefine);
            game.addActionDefine(opDefine);
            ActionGraph graph = new ActionGraph();
            var intOpNode = graph.createActionNode(opDefine);
            var intNode1 = graph.createActionNode(constDefine);
            var intNode2 = graph.createActionNode(constDefine);

            intOpNode.setInputDefaultValue("operator", IntegerOperator.add);
            intNode1.setInputDefaultValue("value", 40);
            intNode2.setInputDefaultValue("value", 18);

            graph.connect(intNode1.getOutputPort<ValueOutput>("result"), intOpNode.getParamInputPort("arg", 0));
            graph.connect(intNode2.getOutputPort<ValueOutput>("result"), intOpNode.getParamInputPort("arg", 1));

            var flow = new Flow(game, null, null, null, null);
            Assert.AreEqual(58, game.getValue<int>(flow, intOpNode.getOutputPort<ValueOutput>("result")).Result);

        }
    }
}