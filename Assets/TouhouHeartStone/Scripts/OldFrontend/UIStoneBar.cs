using System.Collections.Generic;

using UnityEngine;

namespace TouhouHeartstone.OldFrontend
{
    public class UIStoneBar : MonoBehaviour
    {
        [SerializeField]
        UIStone prefab;

        [SerializeField]
        Transform spawnNode;

        List<UIStone> stoneList = new List<UIStone>();

        int _currentStone, _maxStone;

        public int CurrentStone
        {
            get
            {
                return _currentStone;
            }
            set
            {
                _currentStone = value;
                Reload();
            }
        }

        public int MaxStone
        {
            get { return _maxStone;  }
            set
            {
                _maxStone = value;
                Reload();
            }
        }

        void Reload()
        {
            while(stoneList.Count < MaxStone)
            {
                addStone();
            }

            for (int i = 0; i < stoneList.Count; i++)
            {
                stoneList[i].gameObject.SetActive(i < MaxStone);
                stoneList[i].SetLight(i < CurrentStone);
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
            CurrentStone = 0;
            MaxStone = 0;
        }

    }
}
