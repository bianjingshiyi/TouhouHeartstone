using System.IO;

using TouhouCardEngine;
using TouhouHeartstone;

namespace Tests
{
    class UnitTestGameEnv : IGameEnvironment
    {
        public TextReader getFileReader(string path)
        {
            return new StreamReader(File.Open(path, FileMode.Open));
        }

        public string[] getFiles(string path, string searchPattern)
        {
            return new string[0];
        }

        public void runAction(string script, EffectGlobals globals)
        {
            throw new System.NotImplementedException();
        }

        public T runFunc<T>(string script, EffectGlobals globals)
        {
            throw new System.NotImplementedException();
        }

        public void runScript(string script, EffectGlobals globals)
        {
            throw new System.NotImplementedException();
        }
    }
}
