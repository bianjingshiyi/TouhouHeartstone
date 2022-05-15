using System.Collections.Generic;
using TouhouCardEngine;

namespace Game
{
    partial class CardManager
    {
        #region 公有方法
        public GeneratedActionDefine createActionDefine(int cardPoolId, string name)
        {
            if (!tryGetCardPool(cardPoolId, out CardPool cardPool))
            {
                throw new KeyNotFoundException("不存在卡池" + cardPoolId);
            }
            //分配Id
            int id = 0;
            for (int i = 0; i < cardPool.actionDefineList.Count; i++)
            {
                if (cardPool.actionDefineList[i].id > id)
                {
                    id = cardPool.actionDefineList[i].id;
                }
            }
            id++;
            SerializableActionDefine actionDefine = new SerializableActionDefine()
            {
                id = id,
                name = name
            };
            cardPool.actionDefineList.Add(actionDefine);
            GeneratedActionDefine generatedActionDefine = new GeneratedActionDefine(actionDefine, game.editor.getTypeByName);
            _seriGeneActDefDict.Add(actionDefine, generatedActionDefine);
            return generatedActionDefine;
        }
        public bool removeActionDefine(int cardPoolId, GeneratedActionDefine actionDefine)
        {
            if (!tryGetCardPool(cardPoolId, out CardPool cardPool))
            {
                throw new KeyNotFoundException("不存在卡池" + cardPoolId);
            }
            SerializableActionDefine serializableActionDefine = null;
            foreach (var pair in _seriGeneActDefDict)
            {
                if (pair.Value == actionDefine)
                {
                    serializableActionDefine = pair.Key;
                    break;
                }
            }
            if (serializableActionDefine == null)
                return false;
            _seriGeneActDefDict.Remove(serializableActionDefine);
            return cardPool.actionDefineList.Remove(serializableActionDefine);
        }
        public bool getActionDefine(int cardPoolId, int id, out GeneratedActionDefine actionDefine)
        {
            if (!tryGetCardPool(cardPoolId, out CardPool cardPool))
            {
                throw new KeyNotFoundException("不存在卡池" + cardPoolId);
            }
            SerializableActionDefine serializableActionDefine = cardPool.actionDefineList.Find(a => a.id == id);
            if (serializableActionDefine == null)
            {
                actionDefine = null;
                return false;
            }
            actionDefine = _seriGeneActDefDict[serializableActionDefine];
            if (actionDefine == null)
            {
                actionDefine = new GeneratedActionDefine(serializableActionDefine, game.editor.getTypeByName);
                _seriGeneActDefDict.Add(serializableActionDefine, actionDefine);
            }
            return true;
        }
        public GeneratedActionDefine[] getActionDefines(int cardPoolId)
        {
            if (!tryGetCardPool(cardPoolId, out CardPool cardPool))
            {
                throw new KeyNotFoundException("不存在卡池" + cardPoolId);
            }
            return cardPool.actionDefineList.ConvertAll(s =>
            {
                if (!_seriGeneActDefDict.TryGetValue(s, out GeneratedActionDefine actionDefine))
                {
                    actionDefine = new GeneratedActionDefine(s, game.editor.getTypeByName);
                    _seriGeneActDefDict.Add(s, actionDefine);
                }
                return actionDefine;
            }).ToArray();
        }
        public SerializableActionDefine getSerializableActionDefine(GeneratedActionDefine actionDefine)
        {
            foreach (var pair in _seriGeneActDefDict)
            {
                if (pair.Value == actionDefine)
                    return pair.Key;
            }
            return null;
        }
        #endregion
        #region 属性字段
        Dictionary<SerializableActionDefine, GeneratedActionDefine> _seriGeneActDefDict =
            new Dictionary<SerializableActionDefine, GeneratedActionDefine>();
        #endregion
    }
}
