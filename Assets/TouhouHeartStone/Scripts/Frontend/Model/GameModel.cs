using System.Reflection;
using System;

using UnityEngine;

using TouhouHeartstone.Backend;
using System.IO;

using TouhouCardEngine;

namespace TouhouHeartstone.Frontend.Model
{
    public class GameModel : MonoBehaviour
    {
        public THHGame Game { get; } = new THHGame(new UnityGameEnv());
    }
    class UnityGameEnv : IGameEnvironment
    {
        public TextReader getFileReader(string path)
        {
            return new StreamReader(File.Open(path, FileMode.Open));
        }
        public string[] getFiles(string path, string searchPattern)
        {
            return Directory.GetFiles(Application.streamingAssetsPath + "/" + path, searchPattern);
        }

        public void runAction(string script, EffectGlobals globals)
        {
            throw new NotImplementedException();
            //CSharpScript.RunAsync(script, ScriptOptions.Default.AddReferences(
            //                                 typeof(HeartStoneRule).Assembly,
            //                                 Assembly.LoadFile(UnityEditor.EditorApplication.applicationContentsPath + "/MonoBleedingEdge/lib/mono/unityjit/Facades/netstandard.dll")), globals, typeof(EffectGlobals));
        }

        public T runFunc<T>(string script, EffectGlobals globals)
        {
            throw new NotImplementedException();
            //return CSharpScript.EvaluateAsync<T>(script, ScriptOptions.Default.AddReferences(
            //                              typeof(HeartStoneRule).Assembly,
            //                              Assembly.LoadFile(UnityEditor.EditorApplication.applicationContentsPath + "/MonoBleedingEdge/lib/mono/unityjit/Facades/netstandard.dll")), globals, typeof(EffectGlobals)).Result;
        }
    }
}
