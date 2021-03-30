using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouHeartstone;
using MukioI18n;

namespace UI
{
    public partial class FilterPanel
    {
        CardManager cardManager => ui.getManager<CardManager>();

        LangManager langManager => ui.getManager<LangManager>();

        public event Action onFilterConditionChange;
        public event Action onSortChange;

        [TranslateMsg]
        const string FILTER_TYPE_ALL = "所有";

        [TranslateMsg]
        const string FILTER_TYPE_MASTER = "英雄";

        [TranslateMsg]
        const string FILTER_TYPE_SERVANT = "仆从";

        [TranslateMsg]
        const string FILTER_TYPE_SPELL = "法术";

        [TranslateMsg]
        const string FILTER_TYPE_ITEM = "道具";

        KeyValuePair<string, string>[] typeName = {
            new KeyValuePair<string, string>("", FILTER_TYPE_ALL),
            new KeyValuePair<string, string>("Master", FILTER_TYPE_MASTER),
            new KeyValuePair<string, string>("Servant", FILTER_TYPE_SERVANT),
            new KeyValuePair<string, string>("Spell", FILTER_TYPE_SPELL),
            new KeyValuePair<string, string>("Item", FILTER_TYPE_ITEM)
        };
        enum SortMethod
        {
            Cost,
            Cost_R,
            Attack,
            Attack_R
        };

        [TranslateMsg]
        const string SORT_BY_COST = "费用升序";
        [TranslateMsg]
        const string SORT_BY_COST_REVERSE = "费用降序";
        [TranslateMsg]
        const string SORT_BY_ATTACK = "攻击升序";
        [TranslateMsg]
        const string SORT_BY_ATTACK_REVERSE = "攻击降序";

        string[] sortMethodsName = { SORT_BY_COST, SORT_BY_COST_REVERSE, SORT_BY_ATTACK, SORT_BY_ATTACK_REVERSE };

        string filterKeyword = "";
        string filterType = "";
        int filterCost = 0;
        int filterRare = 0;
        int filterCharacter = 0;

        int[] allCharacters = { };

        SortMethod sortMethod = 0;

        [TranslateMsg]
        const string CHARACTER_NEUTRAL = "中立";

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
                DropdownTypeDropdownLight.asDropdown.options.Add(new UnityEngine.UI.Dropdown.OptionData(langManager._(typeName[i].Value)));
            }

            // 填充排序方式
            DropdownSortDropdownLight.asDropdown.options.Clear();
            for (int i = 0; i < sortMethodsName.Length; i++)
            {
                DropdownSortDropdownLight.asDropdown.options.Add(new UnityEngine.UI.Dropdown.OptionData(langManager._(sortMethodsName[i])));
            }

            // 填充角色列表
            allCharacters = cardManager.GetCardDefines(d => d.GetType().Assembly == typeof(THHGame).Assembly
            && d is MasterCardDefine
            && d.GetType().GetCustomAttribute<IgnoreCardDefineAttribute>() == null).Select(d => d.id).ToArray();

            // 添加中立卡的过滤
            allCharacters = allCharacters.Prepend(CATEGORY.CHARACTER_NEUTRAL).ToArray();

            DropdownCharacterDropdownLight.asDropdown.options.Clear();
            for (int i = 0; i < allCharacters.Length; i++)
            {
                var id = allCharacters[i];
                var name = id == CATEGORY.CHARACTER_NEUTRAL ? langManager._(CHARACTER_NEUTRAL) : cardManager.getSkin(id)?.name;
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
            if (cd.getCharacterID() != allCharacters?[filterCharacter])
                return false;

            // 过滤名字
            if (!string.IsNullOrEmpty(filterKeyword))
            {
                if (!sd.name.Contains(filterKeyword) && !sd.desc.Contains(filterKeyword)) return false;
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

        public void OnEnable()
        {
            ResetCondition();
        }

        public void ResetCondition()
        {
            DropdownCostDropdownLight.asDropdown.value = 0;
            DropdownRareDropdownLight.asDropdown.value = 0;
            DropdownCharacterDropdownLight.asDropdown.value = 0;
            DropdownTypeDropdownLight.asDropdown.value = 0;
            InputField.text = "";
        }

        public int sortCompareMethod(CardDefine a, CardDefine b)
        {
            if (a == null) return 1;
            if (b == null) return -1;

            switch (sortMethod)
            {
                default:
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
            sortMethod = (SortMethod)id;

            onSortChange?.Invoke();
        }
    }
}
