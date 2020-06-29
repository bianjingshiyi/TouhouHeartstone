using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouHeartstone;

namespace UI
{
    public partial class FilterPanel
    {
        public event Action onFilterConditionChange;
        public event Action onSortChange;

        KeyValuePair<string, string>[] typeName = { 
            new KeyValuePair<string, string>("", "所有"),
            new KeyValuePair<string, string>("Master", "英雄"),
            new KeyValuePair<string, string>("Servant", "随从"),
            new KeyValuePair<string, string>("Skill", "技能"),
            new KeyValuePair<string, string>("Spell", "法术")
        };

        string filterKeyword = "";
        string filterType = "";
        int filterCost = 0;
        int filterRare = 0;
        int filterCharacter = 0;

        int sortMethod = 0;

        partial void onAwake()
        {
            DropdownCost.onValueChanged.AddListener(onCostFilterChange);
            DropdownRare.onValueChanged.AddListener(onRareFilterChange);
            DropdownCharacter.onValueChanged.AddListener(onCharacterFilterChange);
            DropdownType.onValueChanged.AddListener(onTypeFilterChange);
            InputField.onValueChanged.AddListener(onNameFilterChange);

            DropdownSort.onValueChanged.AddListener(onSortMethodChange);

            DropdownType.options.Clear();
            for (int i = 0; i < typeName.Length; i++)
            {
                DropdownType.options.Add(new UnityEngine.UI.Dropdown.OptionData(typeName[i].Value));
            }
            // todo: 填充角色列表

        }

        public bool cardFilter(CardDefine cd, CardSkinData sd)
        {
            // 过滤费用
            if (filterCost != 0)
            {
                if (filterCost < 8)
                {
                    if (cd.getCost() != filterCost) return false;
                }
                else
                {
                    if (cd.getCost() < 8) return false;
                }
            }

            // 过滤类型
            if (!string.IsNullOrEmpty(filterType))
            {
                if (!string.Equals(filterType, cd.type)) return false;
            }

            // 过滤稀有度
            if (filterRare != 0)
            {
                // todo: 稀有度判断
            }

            // 过滤角色
            if (filterCharacter != 0)
            {
                // todo: 角色判断
            }

            // 过滤名字
            if (!string.IsNullOrEmpty(filterKeyword))
            {
                if (!sd.name.Contains(filterKeyword)) return false;
            }


            return true;
        }

        void onCostFilterChange(int id)
        {
            filterCost = id;
            onFilterConditionChange?.Invoke();
        }

        void onRareFilterChange(int id)
        {
            filterRare = id;
            onFilterConditionChange?.Invoke();
        }

        void onCharacterFilterChange(int id)
        {
            // todo
            onFilterConditionChange?.Invoke();
        }

        void onTypeFilterChange(int id)
        {
            filterType = typeName[id].Key;
            onFilterConditionChange?.Invoke();
        }

        void onNameFilterChange(string filter)
        {
            filterKeyword = filter.Trim();
            onFilterConditionChange?.Invoke();
        }

        public int sortCompareMethod(BuildCardListItem a, BuildCardListItem b)
        {
            switch (sortMethod)
            {
                default:
                case 0:
                    return a.card.id - b.card.id;
                case 1:
                    return a.card.getCost() - b.card.getCost();
                case 2:
                    return a.card.getAttack() - b.card.getAttack();
            }
        }

        void onSortMethodChange(int id)
        {
            sortMethod = id;

            onSortChange?.Invoke();
        }
    }
}
