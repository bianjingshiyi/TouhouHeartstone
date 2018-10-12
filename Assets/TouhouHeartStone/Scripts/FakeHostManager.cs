using System.IO;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;

using UnityEngine;

namespace TouhouHeartstone
{
    public class FakeHostManager : NetworkManager
    {
        public override bool isClient
        {
            get { return false; }
        }
        public override int id
        {
            get { return _id; }
        }
        [SerializeField]
        int _id;
        public void sendObject(object obj)
        {

        }
        public void broadcastObject(object obj)
        {
            if (UnityEngine.Random.Range(0f, 1f) > _loss)
                StartCoroutine(broadcastObjectCoroutine(obj));
        }
        [SerializeField]
        float _loss = 0.1f;
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
                    if (connections[i] is FakeClientManager)
                        (connections[i] as FakeClientManager).receiveBytes(bytes);
                }
            }
        }
        float lag
        {
            get { return _lag; }
        }
        [SerializeField]
        float _lag = 0.2f;
    }
}