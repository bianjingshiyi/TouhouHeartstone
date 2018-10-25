using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace TouhouHeartstone
{
    public abstract class NetworkManager : THManager
    {
        public abstract bool isClient { get; }
        public abstract int id { get; }
        public int hostId
        {
            get
            {
                if (_hostId < 0)
                    _hostId = Array.FindIndex(connections, e => { return e is FakeHostManager; });
                return _hostId;
            }
        }
        [SerializeField]
        int _hostId = -1;
        public NetworkManager[] connections
        {
            get
            {
                if (_connections == null || _connections.Length <= 0)
                {
                    List<NetworkManager> connectionList = new List<NetworkManager>();
                    for (int i = 0; i < SceneManager.sceneCount; i++)
                    {
                        Scene scene = SceneManager.GetSceneAt(i);
                        foreach (GameObject obj in scene.GetRootGameObjects())
                        {
                            NetworkManager connection = obj.GetComponentInChildren<NetworkManager>();
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
        NetworkManager[] _connections;
        public void sendObject(int targetId, object obj)
        {
            if (obj == null)
                return;
            if (UnityEngine.Random.Range(0f, 1f) > _loss)
                StartCoroutine(sendObjectCoroutine(targetId, obj));
            else
                Debug.Log("Client丢包", this);
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
                NetworkManager target = Array.Find(connections, e => { return e.id == targetId; });
                if (target != null)
                    target.receiveBytes(id, bytes);
            }
        }
        public void broadcastObject(object obj)
        {
            if (obj == null)
                return;
            if (UnityEngine.Random.Range(0f, 1f) > _loss)
                StartCoroutine(broadcastObjectCoroutine(obj));
            else
                Debug.Log("Host丢包", this);
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
                        connections[i].receiveBytes(id, bytes);
                }
            }
        }
        private void receiveBytes(int senderId, byte[] bytes)
        {
            if (UnityEngine.Random.Range(0f, 1f) > _loss)
                StartCoroutine(receiveBytesCoroutine(senderId, bytes));
            else
                Debug.Log("Client丢包", this);
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
        protected abstract void onReceiveObject(int senderId, object obj);
        [SerializeField]
        float _loss = 0.1f;
        [SerializeField]
        float _lag = 0.2f;
    }
}