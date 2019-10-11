using System.Reflection;

using UnityEngine;

using TouhouHeartstone.Backend;
using System.IO;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;

using TouhouCardEngine;

namespace TouhouHeartstone.Frontend.Model
{
    public class GameModel : MonoBehaviour
    {
        public Game Game { get; } = new Game(new UnityGameEnv());
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
            CSharpScript.RunAsync(script, ScriptOptions.Default.AddReferences(
                                             typeof(HeartStoneRule).Assembly,
                                             Assembly.LoadFile(UnityEditor.EditorApplication.applicationContentsPath + "/MonoBleedingEdge/lib/mono/unityjit/Facades/netstandard.dll")), globals, typeof(EffectGlobals));
        }

        public T runFunc<T>(string script, EffectGlobals globals)
        {
            return CSharpScript.EvaluateAsync<T>(script, ScriptOptions.Default.AddReferences(
                                          typeof(HeartStoneRule).Assembly,
                                          Assembly.LoadFile(UnityEditor.EditorApplication.applicationContentsPath + "/MonoBleedingEdge/lib/mono/unityjit/Facades/netstandard.dll")), globals, typeof(EffectGlobals)).Result;
        }
    }
}
