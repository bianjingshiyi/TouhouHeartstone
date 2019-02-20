using System.Collections.Generic;
using UnityEngine;

namespace IGensoukyo.Utilities
{
    public abstract class ItemSpawner<T> : MonoBehaviour where T : MonoBehaviour
    {
        [SerializeField]
        protected T prefab;

        [SerializeField]
        protected Transform _spawnRoot;

        Transform spawnRoot => _spawnRoot ?? this.transform ;

        protected List<T> itemList = new List<T>();

        protected int ItemCount => itemList.Count;

        protected T SpawnItem()
        {
            T item = Instantiate(prefab, spawnRoot);
            itemList.Add(item);
            item.gameObject.SetActive(true);
            return item;
        }

        protected T GetItemAt(int index)
        {
            return itemList[index];
        }

        protected bool RemoveItem(T item)
        {
            Destroy(item.gameObject);
            return itemList.Remove(item);
        }

        protected void RemoveItemAt(int index)
        {
            var item = itemList[index];
            itemList.RemoveAt(index);
            Destroy(item.gameObject);
        }

        protected void RemoveAllItem()
        {
            foreach (var item in itemList)
            {
                Destroy(item.gameObject);
            }
            itemList.Clear();
        }
    }
}
