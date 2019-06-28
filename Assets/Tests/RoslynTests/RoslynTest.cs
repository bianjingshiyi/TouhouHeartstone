using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

using TouhouHeartstone.Backend;
using TouhouHeartstone.Backend.Builtin;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;

namespace Tests
{
    public static class Test
    {
        public static void test()
        {
            Debug.Log("Test");
        }
    }
    public class RoslynTest : MonoBehaviour
    {
        private void Start()
        {

            Debug.Log(typeof(object).Assembly.Location);
            CSharpScript.RunAsync("Tests.Test.test();", ScriptOptions.Default.AddReferences(typeof(Test).Assembly).AddReferences(Assembly.LoadFile("D:/Software/UnityXD/Unity2018.3/Editor/Data/MonoBleedingEdge/lib/mono/unityjit/Facades/netstandard.dll")));
            //CSharpScript.RunAsync("Tests.Test obj = new Tests.Test();obj.ToString();", ScriptOptions.Default.AddReferences(typeof(Test).Assembly).AddReferences("netstandard"));
            //TouhouHeartstone.CardEngine engine = new TouhouHeartstone.CardEngine(new HeartStoneRule(null), 0);
            //CSharpScript.RunAsync("engine.ToString();", ScriptOptions.Default.AddReferences(typeof(HeartStoneRule).Assembly), new Globals { engine = engine }, typeof(Globals));
        }
    }
}