using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouHeartstone;

namespace UI
{
    public partial class FilterPanel
    {
        CardManager cardManager => ui.getManager<CardManager>();

        public event Action onFilterConditionChange;
        public event Action onSortChange;

        KeyValuePair<string, string>[] typeName = {
            new KeyValuePair<string, string>("", "所有"),
            new KeyValuePair<string, string>("Master", "英雄"),
            new KeyValuePair<string, string>("Servant", "随从"),
            new KeyValuePair<string, string>("Skill", "技能"),
            new KeyValuePair<string, string>("Spell", "法术")
        };
        enum SortMethod
        {
            ID,
            ID_R,
            Cost,
            Cost_R,
            Attack,
            Attack_R
        };

        string[] sortMethodsName = { "默认", "倒序", "费用升序", "费用降序", "攻击升序", "攻击降序" };

        string filterKeyword = "";
        string filterType = "";
        int filterCost = 0;
        int filterRare = 0;
        int filterCharacter = 0;

        int[] allCharacters = { };

        SortMethod sortMethod = 0;

        partial void onAwake()
        {
            DropdownCostDropdownLight.asDropdown.onValueChanged.AddListener(onCostFilterChange);
            DropdownRareDropdownLight.asDropdown.onValueChanged.AddListener(onRareFilterChange);
            DropdownCharacterDropdownLight.asDropdown.onValueChanged.AddListener(onCharacterFilterChange);
            DropdownTypeDropdownLight.asDropdown.onValueChanged.AddListener(onTypeFilterChange);
            InputField.onValueChanged.AddListener(onNameFilterChange);

            DropdownSortDropdownLight.asDropdown.onValueChanged.AddListener(onSortMethodChange);

            // 填充过滤类型
            DropdownTypeDropdownLight.asDropdown.options.Clear();
            for (int i = 0; i < typeName.Length; i++)
            {
                DropdownTypeDropdownLight.asDropdown.options.Add(new UnityEngine.UI.Dropdown.OptionData(typeName[i].Value));
            }

            // 填充排序方式
            DropdownSortDropdownLight.asDropdown.options.Clear();
            for (int i = 0; i < sortMethodsName.Length; i++)
            {
                DropdownSortDropdownLight.asDropdown.options.Add(new UnityEngine.UI.Dropdown.OptionData(sortMethodsName[i]));
            }

            // 填充角色列表
            var cards = cardManager.GetCardDefines(d => d.GetType().Assembly == typeof(THHGame).Assembly
            && d.GetType().GetCustomAttribute<IgnoreCardDefineAttribute>() == null);
            allCharacters = cards.getCharactersID();
            DropdownCharacterDropdownLight.asDropdown.options.Clear();
            DropdownCharacterDropdownLight.asDropdown.options.Add(new UnityEngine.UI.Dropdown.OptionData("所有"));
            for (int i = 0; i < allCharacters.Length; i++)
            {
                var id = allCharacters[i];
                var name = id == CATEGORY.CHARACTER_NEUTRAL ? "中立" : cardManager.getSkin(id)?.name;
                DropdownCharacterDropdownLight.asDropdown.options.Add(new UnityEngine.UI.Dropdown.OptionData(name));
            }
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
                if (cd.getCharacterID() != allCharacters?[filterCharacter - 1])
                    return false;
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
            filterCharacter = id;
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

        public int sortCompareMethod(CardDefine a, CardDefine b)
        {
            switch (sortMethod)
            {
                default:
                case SortMethod.ID:
                    return a.id - b.id;
                case SortMethod.ID_R:
                    return b.id - a.id;
                case SortMethod.Cost:
                    return a.getCost() - b.getCost();
                case SortMethod.Attack:
                    return a.getAttack() - b.getAttack();
                case SortMethod.Cost_R:
                    return b.getCost() - a.getCost();
                case SortMethod.Attack_R:
                    return b.getAttack() - a.getAttack();
            }
        }

        void onSortMethodChange(int id)
        {
            UberDebug.Log($"Sort method change: {id}");
            sortMethod = (SortMethod)id;

            onSortChange?.Invoke();
        }
    }
}
