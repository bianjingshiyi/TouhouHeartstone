using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace TouhouHeartstone.Backend
{
    class FakeNetworkManager : NetworkManager
    {
        public override bool isClient
        {
            get { return _isClient; }
        }
        [SerializeField]
        bool _isClient = true;
        public override int localPlayerId
        {
            get { return _localPlayerId; }
        }
        [SerializeField]
        int _localPlayerId = 0;
        public override int[] playersId
        {
            get
            {
                return connections.Select(e => { return e.localPlayerId; }).ToArray();
            }
        }
        FakeNetworkManager[] connections
        {
            get
            {
                if (_connections == null || _connections.Length < 1)
                {
                    List<FakeNetworkManager> connectionList = new List<FakeNetworkManager>();
                    for (int i = 0; i < SceneManager.sceneCount; i++)
                    {
                        Scene scene = SceneManager.GetSceneAt(i);
                        foreach (GameObject obj in scene.GetRootGameObjects())
                        {
                            FakeNetworkManager connection = obj.GetComponentInChildren<FakeNetworkManager>();
                            if (connection != null)
                                connectionList.Add(connection);
                        }
                    }
                    _connections = connectionList.ToArray();
                }
                return _connections;
            }
        }
        [SerializeField]
        FakeNetworkManager[] _connections = null;
        public override int hostId
        {
            get
            {
                if (_host == null)
                {
                    for (int i = 0; i < connections.Length; i++)
                    {
                        if (!connections[i].isClient)
                        {
                            _host = connections[i];
                            break;
                        }
                    }
                }
                return _host.localPlayerId;
            }
        }
        [SerializeField]
        FakeNetworkManager _host;
        public override void sendObject(int targetId, object obj)
        {
            if (obj == null)
                return;
            if (UnityEngine.Random.Range(0f, 1f) > _loss)
                StartCoroutine(sendObjectCoroutine(targetId, obj));
            else
                Debug.Log(this + "发送" + obj + "丢包", this);
        }
        private IEnumerator sendObjectCoroutine(int targetId, object obj)
        {
            yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(0, _lag));
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(stream, obj);
                byte[] bytes = new byte[stream.Length];
                stream.Position = 0;
                stream.Read(bytes, 0, (int)stream.Length);
                FakeNetworkManager target = Array.Find(connections, e => { return e.localPlayerId == targetId; });
                if (target != null)
                    target.receiveBytes(localPlayerId, bytes);
            }
        }
        public override void broadcastObject(object obj)
        {
            if (obj == null)
                return;
            if (UnityEngine.Random.Range(0f, 1f) > _loss)
                StartCoroutine(broadcastObjectCoroutine(obj));
            else
                Debug.Log(this + "发送" + obj + "丢包", this);
        }
        private IEnumerator broadcastObjectCoroutine(object obj)
        {
            yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(0, _lag));
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(stream, obj);
                byte[] bytes = new byte[stream.Length];
                stream.Position = 0;
                stream.Read(bytes, 0, (int)stream.Length);
                for (int i = 0; i < connections.Length; i++)
                {
                    if (connections[i] != this)
                        connections[i].receiveBytes(localPlayerId, bytes);
                }
            }
        }
        private void receiveBytes(int senderId, byte[] bytes)
        {
            if (UnityEngine.Random.Range(0f, 1f) > _loss)
                StartCoroutine(receiveBytesCoroutine(senderId, bytes));
            else
                Debug.Log(this + "接收丢包", this);
        }
        private IEnumerator receiveBytesCoroutine(int senderId, byte[] bytes)
        {
            yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(0, _lag));
            using (MemoryStream stream = new MemoryStream())
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Position = 0;
                BinaryFormatter bf = new BinaryFormatter();
                onReceiveObject(senderId, bf.Deserialize(stream));
            }
        }
        public override event Action<int, object> onReceiveObject;
        [SerializeField]
        float _loss = 0.1f;
        [SerializeField]
        float _lag = 0.2f;
    }
}