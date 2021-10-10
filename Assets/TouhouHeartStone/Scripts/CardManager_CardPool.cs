using System;
using BJSYGameCore;
using System.Threading.Tasks;
using UI;
using Controllers;

namespace Game
{
    partial class CardManager
    {
        public async Task getCardPoolInfosAsync(FileManager fileManager, Action<string, CardPoolInfo> onCardPoolInfoRead)
        {
            var files = fileManager.getFiles(CARDPOOLS_DIR_NAME, "*.ddcp", true);
            for (int i = 0; i < files.Length; i++)
            {
                string path = files[i];
                string[] lines = await fileManager.readTextFile(path, lineCount: 1);
                if (lines == null || lines.Length < 1)
                    continue;
                onCardPoolInfoRead?.Invoke(files[i], JsonHelper.fromJson<CardPoolInfo>(lines[0]));
            }
        }
        const string CARDPOOLS_DIR_NAME = "CardPools";
    }
}
