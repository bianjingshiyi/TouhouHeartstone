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
            return ett;
        }

        Vector3 calculateEntityPos(int count, int pos)
        {
            Vector3 basePos = new Vector3(0, 0, 0);
            const float width = 0.4f;

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
            // todo: 座标的转换。
            var cnt = entityList.Count;

            int insertPos = cnt;

            // 查找插入位置
            for (int i = 0; i < cnt; i++)
            {
                if (calculateEntityPos(cnt, i).x > pos.x)
                {
                    insertPos = i;
                    break;
                }
            }

            // 设置其他卡牌的位置
            for (int i = 0; i < cnt; i++)
            {
                // todo: 设置动画
                entityList[i].transform.localPosition = calculateEntityPos(cnt + 1, i >= cnt ? i + 1 : i);
            }

            return insertPos;
        }

        /// <summary>
        /// 更新卡片的位置
        /// </summary>
        void updateCardPos()
        {
            var cnt = entityList.Count;
            for (int i = 0; i < cnt; i++)
            {
                entityList[i].transform.localPosition = calculateEntityPos(cnt, i);
            }
        }

    }
}
