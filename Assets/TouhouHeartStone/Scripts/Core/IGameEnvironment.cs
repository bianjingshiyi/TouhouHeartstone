using System.IO;
using Microsoft.CodeAnalysis.Scripting;

namespace TouhouHeartstone
{
    public interface IGameEnvironment
    {
        Script createScript(string script);
        string[] getFiles(string path, string searchPattern);
        TextReader getFileReader(string path);
    }
}