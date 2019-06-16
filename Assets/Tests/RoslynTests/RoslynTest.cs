using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;

namespace Tests
{
    public class RoslynTest : MonoBehaviour
    {
        private void Start()
        {
            CSharpScript.RunAsync("UnityEngine.Debug.Log(\"Roslyn\");", ScriptOptions.Default.WithReferences(typeof(Debug).Assembly));
        }
    }
}