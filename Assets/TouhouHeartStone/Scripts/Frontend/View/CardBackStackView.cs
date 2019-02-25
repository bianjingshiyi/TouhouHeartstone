using IGensoukyo.Utilities;

using UnityEngine;
namespace TouhouHeartstone.Frontend.View
{
    public class CardBackStackView : ItemSpawner<MonoBehaviour>
    {
        [SerializeField]
        int Limit = 30;

        private int _Count;
        public int Count
        {
            get { return _Count; }
            set { _Count = value; onCountChange(); }
        }

        void onCountChange()
        {
            int count = Count > Limit ? Limit : Count;

            while (count > ItemCount)
                SpawnItem();

            for (int i = 0; i < ItemCount; i++)
            {
                GetItemAt(i).gameObject.SetActive(i < count);
            }
        }

        private void Start()
        {
            Count = 25;
        }
    }
}
