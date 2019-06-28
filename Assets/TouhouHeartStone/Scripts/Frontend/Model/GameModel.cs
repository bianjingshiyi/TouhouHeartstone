using System.Reflection;

using UnityEngine;

using TouhouHeartstone.Backend;
using System.IO;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;

namespace TouhouHeartstone.Frontend.Model
{
    public class GameModel : MonoBehaviour
    {
        public Game Game { get; } = new Game(new UnityGameEnv());
    }
    class UnityGameEnv : IGameEnvironment
    {
        public Script createScript(string script)
        {
            return CSharpScript.Create(script, ScriptOptions.Default.AddReferences(typeof(HeartStoneRule).Assembly, Assembly.LoadFile("D:/Software/UnityXD/Unity2018.3/Editor/Data/MonoBleedingEdge/lib/mono/unityjit/Facades/netstandard.dll")), typeof(EffectGlobals));
        }
        public TextReader getFileReader(string path)
        {
            return new StreamReader(File.Open(path, FileMode.Open));
        }
        public string[] getFiles(string path, string searchPattern)
        {
            return Directory.GetFiles(Application.streamingAssetsPath + "/" + path, searchPattern);
        }
    }
}
