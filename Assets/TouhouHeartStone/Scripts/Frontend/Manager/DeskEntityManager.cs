using UnityEngine;
using System;
using System.Collections.Generic;

namespace TouhouHeartstone.Frontend.Manager
{
    public class DeskEntityManager : FrontendSubManager
    {
        [SerializeField]
        DeskEntity prefab;

        [SerializeField]
        Transform entitySpawnNode;

        List<DeskEntity> entityList = new List<DeskEntity>();

        public DeskEntity Insert(CardFace card, int pos)
        {
            var ett = Instantiate(prefab, entitySpawnNode);
            ett.SetInstanceID(card.InstanceID);
            ett.transform.localPosition = calculateEntityPos(entityList.Count + 1, pos);

            entityList.Insert(pos, ett);
            UpdateEntityPos();
            return ett;
        }

        /// <summary>
        /// 根据卡片的instanceID获取位置index
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetIndexByCardID(int id)
        {
            for (int i = 0; i < entityList.Count; i++)
            {
                if(entityList[i].InstanceID == id)
                {
                    return i;
                }
            }
            return -1; 
        }

        /// <summary>
        /// 根据index获取位置
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Vector3 GetPositionByIndex(int index)
        {
            return entityList[index].transform.position;
            // return entitySpawnNode.localToWorldMatrix.MultiplyVector(calculateEntityPos(entityList.Count, index));
        }

        /// <summary>
        /// 根据ID移除某个卡
        /// </summary>
        /// <param name="id"></param>
        public void RemoveEntityByInstanceID(int id)
        {
            int index = GetIndexByCardID(id);
            if(index >=0)
            {
                Destroy(entityList[index].gameObject);
                entityList.RemoveAt(index);
                UpdateEntityPos();
            }
        }

        Vector3 calculateEntityPos(int count, int pos)
        {
            Vector3 basePos = new Vector3(0, 0, 0);
            const float width = 1f;

            var tWidth = width * (count - 1);
            var offset = -tWidth / 2;
            basePos.x = offset + width * pos;

            return basePos;
        }

        /// <summary>
        /// 预先预留插入位置并返回将要插入的位置
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public int ReserveInsertSpace(Vector2 pos)
        {
            var localPos = screenToDeskPos(pos);

            var cnt = entityList.Count;
            int insertPos = cnt;

            // 查找插入位置
            for (int i = 0; i < cnt; i++)
            {
                if (calculateEntityPos(cnt, i).x > localPos.x)
                {
                    insertPos = i;
                    break;
                }
            }
            // 设置其他卡牌的位置
            for (int i = 0; i < cnt; i++)
            {
                setEntityPos(entityList[i],  calculateEntityPos(cnt + 1, i >= insertPos ? i + 1 : i));
            }

            return insertPos;
        }

        Vector3 screenToDeskPos(Vector3 screen)
        {
            var cam = Camera.main;
            var dist = cam.transform.position.y - entitySpawnNode.position.y;

            screen.z = dist;
            var worldPos = cam.ScreenToWorldPoint(screen);
            var localPos = entitySpawnNode.worldToLocalMatrix.MultiplyVector(worldPos);
            return localPos;
        }

        /// <summary>
        /// 更新卡片的位置
        /// </summary>
        public void UpdateEntityPos()
        {
            var cnt = entityList.Count;
            for (int i = 0; i < cnt; i++)
            {
                setEntityPos(entityList[i], calculateEntityPos(cnt, i));
            }
        }

        void setEntityPos(DeskEntity ett, Vector3 pos)
        {
            // todo: 设置动画
            ett.transform.localPosition = pos;
        }
    }
}
