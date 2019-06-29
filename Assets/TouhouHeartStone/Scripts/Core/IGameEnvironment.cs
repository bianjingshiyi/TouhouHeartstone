using System.IO;
using Microsoft.CodeAnalysis.Scripting;

namespace TouhouHeartstone
{
    public interface IGameEnvironment
    {
        void runScript(string script, EffectGlobals globals);
        string[] getFiles(string path, string searchPattern);
        TextReader getFileReader(string path);
    }
}