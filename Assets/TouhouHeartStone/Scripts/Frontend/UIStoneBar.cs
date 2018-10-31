using System.Collections.Generic;

using UnityEngine;

namespace TouhouHeartstone.Frontend
{
    public class UIStoneBar : MonoBehaviour
    {
        [SerializeField]
        UIStone prefab;

        [SerializeField]
        Transform spawnNode;

        List<UIStone> stoneList = new List<UIStone>();

        public void Set(int curr, int max)
        {
            while(stoneList.Count < max)
            {
                addStone();
            }

            for (int i = 0; i < stoneList.Count; i++)
            {
                stoneList[i].gameObject.SetActive(i < max);
                stoneList[i].SetLight(i < curr);
            }
        }

        void addStone()
        {
            var pf = Instantiate(prefab, spawnNode);
            stoneList.Add(pf);

            pf.transform.localPosition = calcStonePos(stoneList.Count - 1);
        }

        Vector3 calcStonePos(int index)
        {
            return new Vector3(index * 0.5f, 0);
        }

        public void Start()
        {
            Set(5, 8);
        }

    }
}
