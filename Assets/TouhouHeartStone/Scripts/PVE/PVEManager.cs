using System.Threading.Tasks;
using BJSYGameCore;
using Controllers;
using MongoDB.Bson;
using UnityEngine;

namespace Game.PVE
{
    public class PVEManager : Manager
    {
        #region 公有方法
        public async Task saveData(SaveData data)
        {
            var serializable = new SerializableSaveData(data);
            var json = serializable.ToJson();
            var path = getSavePath();
            await main.fileManager.savePersistentFile(path, new string[]
            {
                json
            });
        }
        public void deleteData()
        {
            if (!hasData())
                return;
            var path = getSavePath();
            main.fileManager.deletePersistentFile(path);
        }
        public bool hasData()
        {
            var path = getSavePath();
            return main.fileManager.isPersistentFileExist(path);
        }
        public async Task<SaveData> loadData()
        {
            if (!hasData())
                return null;
            var path = getSavePath();
            var json = await main.fileManager.readPersistentTextFile(path);
            var seri = JsonHelper.fromJson<SerializableSaveData>(json);
            return seri.ToSaveData();
        }
        #endregion
        #region 私有方法
        private string getSavePath()
        {
            return _savePath;
        }
        #endregion
        #region 属性字段
        public MainManager main
        {
            get
            {
                if (!_main)
                {
                    _main = TransformHelper.findComponentInScene<MainManager>(gameObject.scene);
                }
                return _main;
            }
        }
        private MainManager _main;
        [SerializeField]
        private string _savePath = "pve.dds";
        #endregion
    }
}
