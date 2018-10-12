using System.IO;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;

using UnityEngine;

namespace TouhouHeartstone
{
    public class FakeClientManager : NetworkManager
    {
        public override bool isClient
        {
            get { return true; }
        }
        public override int id
        {
            get { return _id; }
        }
        [SerializeField]
        int _id;
        public void receiveBytes(byte[] bytes)
        {
            if (UnityEngine.Random.Range(0f, 1f) > _loss)
                StartCoroutine(receiveBytesCoroutine(bytes));
        }
        [SerializeField]
        float _loss;
        private IEnumerator receiveBytesCoroutine(byte[] bytes)
        {
            yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(0, _lag));
            using (MemoryStream stream = new MemoryStream())
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Position = 0;
                BinaryFormatter bf = new BinaryFormatter();
                onReceiveObject(bf.Deserialize(stream));
            }
        }
        [SerializeField]
        float _lag = 0.2f;
        void onReceiveObject(object obj)
        {
            if (obj is FirstPlayerRecord)
            {
                (obj as FirstPlayerRecord).apply(game);
            }
        }
    }
    public class RecordManager : THManager
    {

    }
}