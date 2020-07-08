using NUnit.Framework;
using System.Linq;
using UnityEngine;
using BJSYGameCore.Animations;
using System.CodeDom;
using System;
namespace Tests
{
    public class ShaderControllerGeneratorTests
    {
        [Test]
        public void generateControllerTest()
        {
            ShaderControllerGenerator generator = new ShaderControllerGenerator();
            Shader shader = Resources.Load<Shader>("TestShader");
            var unit = generator.generateGraphicController(shader, "Game");

            var Namespace = unit.Namespaces[0];
            Assert.AreEqual("Game", Namespace.Name);
            var Class = Namespace.Types[0];
            Assert.AreEqual(nameof(GraphMatPropCtrl), Class.BaseTypes[0].BaseType);
            Assert.AreEqual("TestShader", Class.Name);
            checkField(Class, typeof(int), "SHADER_ID", MemberAttributes.Public | MemberAttributes.Const, shader.GetInstanceID());
            checkField(Class, typeof(Color), "_Color");
            checkField(Class, typeof(float), "_Gray");
            var update = checkMethod(Class, typeof(void), "Update");
            checkCall(update, "SetColor", "_Color");
            checkCall(update, "SetFloat", "_Gray");
            var reset = checkMethod(Class, typeof(void), "Reset");
            checkAssign(reset, "_Gray", "GetFloat", "_Gray");
            checkAssign(reset, "_Color", "GetColor", "_Color");
        }

        private static void checkAssign(CodeMemberMethod reset, string fieldName, string methodName, string propName)
        {
            var assign = reset.Statements.OfType<CodeAssignStatement>().First(a => a.Left is CodeFieldReferenceExpression f && f.FieldName == fieldName);
            Assert.True(assign.Right is CodeMethodInvokeExpression i &&
                i.Method.TargetObject is CodePropertyReferenceExpression p && p.TargetObject is CodeBaseReferenceExpression && p.PropertyName == "material" &&
                i.Method.MethodName == methodName &&
                i.Parameters[0] is CodePrimitiveExpression v && (string)v.Value == propName);
        }

        void checkCall(CodeMemberMethod method, string methodName, string propName)
        {
            var call = method.Statements.OfType<CodeExpressionStatement>().Select(e => e.Expression).OfType<CodeMethodInvokeExpression>().First(c => c.Method.MethodName == methodName);
            var material = call.Method.TargetObject as CodePropertyReferenceExpression;
            var p1 = call.Parameters[0] as CodePrimitiveExpression;
            var p2 = call.Parameters[1] as CodeFieldReferenceExpression;
            Assert.AreEqual("material", material.PropertyName);
            Assert.AreEqual(methodName, call.Method.MethodName);
            Assert.AreEqual(propName, p1.Value);
            Assert.AreEqual(propName, p2.FieldName);
        }
        private static CodeMemberMethod checkMethod(CodeTypeDeclaration Class, Type returnType, string methodName)
        {
            CodeMemberMethod method = Class.Members.OfType<CodeMemberMethod>().FirstOrDefault(m => m.Name == methodName);
            Assert.True(method.Attributes.HasFlag(MemberAttributes.Family | MemberAttributes.Override));
            Assert.AreEqual(returnType.FullName, method.ReturnType.BaseType);
            Assert.AreEqual(0, method.Parameters.Count);
            return method;
        }
        private static CodeMemberField checkField(CodeTypeDeclaration Class, Type type, string fieldName, MemberAttributes flag = MemberAttributes.Public, object value = null)
        {
            CodeMemberField field = Class.Members.OfType<CodeMemberField>().FirstOrDefault(f => f.Name == fieldName);
            Assert.NotNull(field);
            Assert.True(field.Attributes.HasFlag(flag));
            Assert.AreEqual(type.FullName, field.Type.BaseType);
            if (value != null)
                Assert.True(field.InitExpression is CodePrimitiveExpression v && v.Value == value);
            return field;
        }
    }
}
